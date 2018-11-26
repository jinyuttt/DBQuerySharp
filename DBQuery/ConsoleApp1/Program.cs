using DBSqlite;
using NetSocket;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // SqliteHelper hp = new SqliteHelper();
            // hp.CreateEmptyDB("mydb.sqlite");
            // string sql= "CREATE TABLE t1(id varchar(4), score int)";
            // hp.ExecuteUpdate(sql);
            // sql = "insert into t1(id,score) values('q',123)";
            // hp.ExecuteUpdate(sql);
            //var ds= hp.GetSelect("select * from t1");
            // Console.WriteLine(ds.Tables.Count);
            //TCPServer ser = new TCPServer();
            //ser.Start(7777);
            //Console.Read();
            UDPSession session = new UDPSession();
            session.Port = 7777;
            session.Host = "127.0.0.1";
            session.Bind();
            session.OnDataReceived += Session_OnDataReceived;
            Console.Read();

        }

        private static void Session_OnDataReceived(object sender, AsyncUdpUserToken token)
        {
            if(token.Length==0)
            {
                token.Length = token.Data.Length;
            }
            Console.WriteLine(System.Text.Encoding.Default.GetString(token.Data, token.Offset, token.Length));
        }
    }
}
