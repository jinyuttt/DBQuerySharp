#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ZMQNetSocket
* 项目描述 ：
* 类 名 称 ：ZMQClient
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


using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using ZeroMQ;

namespace ZMQNetSocket
{
    /* ============================================================================== 
    * 功能描述：ZMQClient 
    * 创 建 者：jinyu 
    * 修 改 者：jinyu 
    * 创建日期：2018 
    * 修改日期：2018 
    * ==============================================================================*/

   public class ZMQClient
    {
        public void Connect(string host, int port)
        {
            using (var client = new RequestSocket(">tcp://localhost:5556"))  // connect
            {

            }
        }
        public void ConnectZMQ(string host, int port)
        {
            using (var requester = new ZSocket(ZSocketType.REQ))
            {
                // Connect
                requester.Connect("tcp://127.0.0.1:5555");

                for (int n = 0; n < 10; ++n)
                {
                    string requestText = "Hello";
                    Console.Write("Sending {0}...", requestText);

                    // Send
                    requester.Send(new ZFrame(requestText));

                    // Receive
                    using (ZFrame reply = requester.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {0} {1}!", requestText, reply.ReadString());
                    }
                }
            }
        }

    }
}
