using DBSqlite;
using NetSocket;
using RedisClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static Task taskTime;
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
            //UDPSession session = new UDPSession();
            //session.Port = 7777;
            //session.Host = "192.168.3.182";
            //session.Bind();
            //session.OnDataReceived += Session_OnDataReceived;
            //Console.Read();
            //for (int i = 0; i < 1000; i++)
            //{
            //     Redis.Instance.Insert(i.ToString(), "sd_" + i);
            //}
            // var keys=Redis.Instance.GetAllKeys();
            //Console.WriteLine(keys.Result.Length);
            //Console.WriteLine(Redis.Instance.Get("100"));
            //Console.Read();
            Task.Factory.StartNew(()=>{
                Test();
            });
            Console.Read();
            Console.Read();

        }

        private static bool Process()
        {
            
                while (true)
                {
                    Console.WriteLine(DateTime.Now);
                    Thread.Sleep(1000);
                }
            return true;
            
        }
        private static void Test()
        {
            var cancelTokenSource = new CancellationTokenSource(10000);
            var result=   cancelTokenSource.Token.Register(() => {
                Console.WriteLine("超时");
               


            });
            var task= Task.Factory.StartNew(() =>
            {
               
                return   Process();


            }, cancelTokenSource.Token);
            //try
            //{
            //    task.Wait(cancelTokenSource.Token);
            //    Console.WriteLine(task.Result);
            //}
            //catch(OperationCanceledException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            // Console.WriteLine(task.Result);
          
            Console.WriteLine("Done");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
           

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
