#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ZMQNetSocket
* 项目描述 ：
* 类 名 称 ：ZMQServer
* 类 描 述 ：
* 命名空间 ：ZMQNetSocket
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2018
* 更新时间 ：2018
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZeroMQ;

namespace ZMQNetSocket
{
    /* ============================================================================== 
    * 功能描述：ZMQServer 
    * 创 建 者：jinyu 
    * 修 改 者：jinyu 
    * 创建日期：2018 
    * 修改日期：2018 
    * ==============================================================================*/

   public class ZMQServer
    {
        public void Start(int port)
        {
            using (var responder = new ZSocket(ZSocketType.REP))
            {
                // Bind
                responder.Bind("tcp://*:"+port);

                while (true)
                {
                    // Receive
                    using (ZFrame request = responder.ReceiveFrame())
                    {
                       
                        // Do some work
                        Thread.Sleep(1);
                        // Send
                        responder.Send(new ZFrame(""));
                    }
                }
            }
        }

        public void StartRsp(int port)
        {
            using (var server = new ResponseSocket("@tcp://localhost:5556"))// bind
            {
                byte[] m1 = server.ReceiveFrameBytes();
                Console.WriteLine("From Client: {0}", m1);

                // Send a response back from the server
                server.SendFrame("Hi Back");

            }
        }
    }
}
