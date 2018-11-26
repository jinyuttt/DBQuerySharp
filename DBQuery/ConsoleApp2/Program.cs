using NetSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            UDPSession session = new UDPSession();
            //session.Host = "127.0.0.1";
            //session.Port = 7777;
            while (true)
            {
                session.SendPackage(Encoding.Default.GetBytes("ddd_" + DateTime.Now.ToString()), "127.0.0.1", 7777);
                Thread.Sleep(2000);
            }
        }
    }
}
