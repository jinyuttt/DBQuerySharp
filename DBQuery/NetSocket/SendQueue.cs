using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSocket
{
    public delegate void PushLossReset(object sender, AsyncUdpUserToken[] list);
    public  class SendQueue
    {
        AsyncUdpUserToken AsyncUdp;
        AutoResetEvent resetEvent = null;
        public event PushLossReset PushLossReset;
        private const int MaxWaitTime = 10;
        private DateTime lastTime = DateTime.Now;
        public long packageID = 0;
        public SendQueue(AsyncUdpUserToken token)
        {
            AsyncUdp = token;
            resetEvent = new AutoResetEvent(false);
            Check();
        }

        private void Check()
        {
            Task.Factory.StartNew(() =>
            {
                resetEvent.WaitOne();
               
                List<AsyncUdpUserToken> list = new List<AsyncUdpUserToken>();
                if (AsyncUdp.ListPack.Count >0)
                {
                    foreach(AsyncUdpUserToken item in AsyncUdp.ListPack)
                    {
                        if(item!=null)
                        {
                            list.Add(item);
                        }
                    }
                }
                if(PushLossReset!=null)
                {
                    PushLossReset(this, list.ToArray());
                }
                if(list.Count>0&&(DateTime.Now-lastTime).TotalSeconds<MaxWaitTime)
                {
                    Check();
                }
            });
        }

        /// <summary>
        /// 接收完成
        /// </summary>
        /// <param name="seq"></param>
        public  void Add(int  seq)
        {
            lastTime = DateTime.Now;
            if (AsyncUdp.ListPack!=null)
            {
                if(AsyncUdp.ListPack.Count>seq)
                {
                    AsyncUdpUserToken current = AsyncUdp.ListPack[seq];
                    packageID = current.DataPackage.packageID;
                    current.FreeCache();
                    AsyncUdp.ListPack[seq] = null;
                }
            }
        }

        /// <summary>
        /// 接收完成
        /// </summary>
        /// <param name="seq"></param>
        /// <returns></returns>
        public AsyncUdpUserToken GetAsyncUdpUserToken(int seq)
        {
            lastTime = DateTime.Now;
            if (AsyncUdp.ListPack != null)
            {
                if (AsyncUdp.ListPack.Count > seq)
                {
                    AsyncUdpUserToken current = AsyncUdp.ListPack[seq];
                    return current;
                }
            }
            return null;
        }

        /// <summary>
        /// 清除
        /// </summary>
        public void Clear()
        {
            AsyncUdp.FreeCache();
            AsyncUdp.ListPack.Clear();
            resetEvent.Set();

        }
    }
}
