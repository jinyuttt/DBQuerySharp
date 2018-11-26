/**
* 命名空间: NetSocket 
* 类 名：UDPSession 
* CLR版本： 4.0.30319.42000
* 版本 ：v1.0
* Copyright (c) jinyu  
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NetSocket
{

    /// <summary>
    /// 功能描述    ：UDPSession 数据协议分包，同时简单的重传
    /// 创 建 者    ：jinyu
    /// 创建日期    ：2018
    /// 最后修改者  ：jinyu
    /// 最后修改日期：2018
    /// </summary>
  public  class UDPSession
    {
        UDPPack uDPPack = null;
        public bool EnableHeart { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 默认65535
        /// bind之前设置
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// 缓存区间最大大小(M或者个数)
        /// 默认1024
        /// 这里是2个相同的缓存区；合理设置
        /// </summary>
        public int TotalBufSize { get; set; }
        /// <summary>
        /// 缓存数据实体
        /// 默认100
        /// </summary>
        public int TokenMaxLeftNum { get; set; }

        public event OnReceiveUdpData OnDataReceived;
        private const int MByte = 1024 * 1024;
        private const int WaitTime = 10;//10s;
    
        /// <summary>
        /// 发送队列
        /// </summary>
        ConcurrentDictionary<long, SendQueue> dicSendQueue = null;

        /// <summary>
        /// 接收队列
        /// </summary>
        ConcurrentDictionary<long, RecvicePool> dicPool = null;

        /// <summary>
        /// 接收完成序列
        /// </summary>
        ConcurrentDictionary<long, DateTime> dicSucess = null;

        private const int MaxWaitSucess = 20;//维持接收完成的时间

        private DateTime minTime = DateTime.Now;//完成的ID移除

        public UDPSession()
        {
            BufferSize = 65535;
            TotalBufSize = 1024;
            TokenMaxLeftNum = Environment.ProcessorCount * 10;
            if (TokenMaxLeftNum == 0)
            {
                TokenMaxLeftNum = 100;
            }
            dicSucess = new ConcurrentDictionary<long, DateTime>();

        }

        public void Bind()
        {
            uDPPack = new UDPPack();
            uDPPack.BufferSize = BufferSize;
            uDPPack.EnableHeart = EnableHeart;
            uDPPack.Host = Host;
            uDPPack.IsFixCache = false;
            uDPPack.Port = Port;
            uDPPack.TotalBufSize = TotalBufSize;
            uDPPack.TokenMaxLeftNum = TokenMaxLeftNum;
            uDPPack.IsProtolUnPack = false;
            uDPPack.Bind();
            uDPPack.OnDataReceived += UDPSocket_OnDataReceived;
            StartReceive();

        }

        /// <summary>
        /// 接收底层的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="token"></param>
        private void UDPSocket_OnDataReceived(object sender, AsyncUdpUserToken token)
        {
            if (dicPool == null)
            {
                dicPool = new ConcurrentDictionary<long, RecvicePool>();
            }
          
           switch (token.Data[token.Offset])
            {
                case 0:
                    {
                        //数据
                       
                        UDPDataPackage package = new UDPDataPackage();
                        package.UnPack(token.Data, token.Offset, token.Length);
                        RecvicePool pool = null;
                        if (dicSucess.ContainsKey(package.packageID))
                        {
                            return;//无用数据了；
                        }
                            if (dicPool.TryGetValue(package.packageID, out pool))
                        {
                            pool.Add(package);
                        }
                        else
                        {
                            pool = new RecvicePool();
                            pool.OnLossData += Pool_OnLossData;
                            pool.OnReviceData += Pool_OnReviceData;
                            dicPool[package.packageID] = pool;
                            pool.Add(package);
                        }
                    }
                    break;
                case 1:
                    {
                        //接收完成序列
                        SendQueue sendQueue = null;
                        LosPackage rsp = new LosPackage(token.Data);
                       
                        if (dicSendQueue.TryGetValue(rsp.packageID,out sendQueue))
                        {
                            sendQueue.Add(rsp.packageSeq);
                        }
                        if (dicSucess.ContainsKey(rsp.packageID))
                        {
                            return;//无用数据了；
                        }

                    }
                    break;

                case 2:
                    {
                        //丢失序列
                        SendQueue sendQueue = null;
                        LosPackage rsp = new LosPackage(token.Data);
                        if (dicSucess.ContainsKey(rsp.packageID))
                        {
                            return;//无用数据了；
                        }
                        if (dicSendQueue.TryGetValue(rsp.packageID, out sendQueue))
                        {
                            AsyncUdpUserToken  resend=  sendQueue.GetAsyncUdpUserToken(rsp.packageSeq);
                            if(resend!=null)
                               uDPPack.Send(resend, 0);
                        }
                    }
                    break;
                case 3:
                    {
                        //完成接收
                        SendQueue sendQueue = null;
                        LosPackage rsp = new LosPackage(token.Data);
                       
                        if (dicSendQueue.TryRemove(rsp.packageID, out sendQueue))
                        {
                            sendQueue.Clear();
                        }
                    }
                    break;
            }
            CheckSucess();

        }

        /// <summary>
        /// 接收完成的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="state"></param>
        private void Pool_OnReviceData(object sender, long id, byte[] data, RecviceState state)
        {
            if (OnDataReceived != null)
            {
                AsyncUdpUserToken token = new AsyncUdpUserToken();
                token.Data = data;
                Task.Factory.StartNew(() =>
                {
                    OnDataReceived(this, token);
                });
                RecvicePool recvice = sender as RecvicePool;
                if(recvice!=null)
                {
                    recvice.Clear();
                }
                recvice.OnLossData -= Pool_OnLossData;
                recvice.OnReviceData -= Pool_OnReviceData;
                dicPool.TryRemove(id, out recvice);
                dicSucess[id] = DateTime.Now;
                AsyncUdpUserToken suecess = new AsyncUdpUserToken();
                LosPackage package = new LosPackage();
                package.packageID = id;
                package.Pack();
                suecess.Data = package.PData;
                uDPPack.Send(suecess, 0);
                Console.WriteLine("sucess:" + id);
            }
        }

        /// <summary>
        /// 请求发送丢失数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="list"></param>
        private void Pool_OnLossData(object sender, LosPackage[] list)
        {
            AsyncUdpUserToken token = new AsyncUdpUserToken();
            foreach (LosPackage los in list)
            {
                los.Pack();
                token.Data = los.PData;
                uDPPack.Send(token, 0);

            }
           
           
        }

        /// <summary>
        /// 接收
        /// </summary>
        private void StartReceive()
        {
            uDPPack.StartReceive();
        }

      

        /// <summary>
     /// 发送数据
     /// </summary>
     /// <param name="token"></param>
        public void SendPackage(AsyncUdpUserToken token)
        {
            if(uDPPack==null)
            {
                Bind();
            }
            //使用了分包缓存
            token.ListPack = new List<AsyncUdpUserToken>();
            uDPPack.SendProtol(token);
            if (dicSendQueue == null)
            {
                dicSendQueue = new ConcurrentDictionary<long, SendQueue>();
            }
            SendQueue sendList = new SendQueue(token);
            dicSendQueue[token.DataPackage.packageID] = sendList;
            sendList.PushLossReset += SendList_PushLossReset;
        }


        /// <summary>
        /// 重发丢失数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="list"></param>
        private void SendList_PushLossReset(object sender, AsyncUdpUserToken[] list)
        {
            foreach(AsyncUdpUserToken token in list)
            {
                uDPPack.Send(token, 0);
            }
            SendQueue sendQueue = sender as SendQueue;
            if(sendQueue!=null)
            {
                //已经全部接收了，没有丢失
                sendQueue.Clear();
                sendQueue.PushLossReset -= SendList_PushLossReset;
            }
            dicSucess[sendQueue.packageID] = DateTime.Now;
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void SendPackage(byte[] data, string host, int port)
        {
                AsyncUdpUserToken token = new AsyncUdpUserToken();
                token.Data = data;
                token.Offset = 0;
                token.Length = data.Length;
                token.Remote = new IPEndPoint(IPAddress.Parse(host), port);
                SendPackage(token);
            }


        /// <summary>
        /// 关闭
        /// </summary>
        private void Close()
        {
            uDPPack.Close();
            this.dicPool.Clear();
            this.dicSendQueue.Clear();
            this.dicSucess.Clear();
           
        }

        /// <summary>
        /// 移除到期成功ID
        /// </summary>
        private void CheckSucess()
        {
            if((DateTime.Now-minTime).TotalSeconds<MaxWaitSucess)
            {
                return;
            }
            long id = -1;
            DateTime date;
            minTime = DateTime.Now;
            ConcurrentBag<long> bag = new ConcurrentBag<long>();
            Parallel.ForEach(dicSucess, (kv) =>
            {
                if ((DateTime.Now - kv.Value).TotalSeconds > MaxWaitSucess)
                {
                    bag.Add(kv.Key);
                    if(minTime>kv.Value)
                    {
                        minTime = kv.Value;
                    }
                }
            });
            do
            {
                if (bag.TryTake(out id))
                {
                    dicSucess.TryRemove(id, out date);
                }
            } while (!bag.IsEmpty);
        }
         

    

      

    }
}
