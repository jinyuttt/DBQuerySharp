using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
namespace DBModel
{
    public enum DBServerType
    {
        [Description("关系数据库")]
        ServerSQL,
        [Description("本地SQL数据库")]
        LocalSQL,
        [Description("内存SQL数据库")]
        MemorySQL,
        [Description("内存NoSQL数据库")]
        MemoryNoSQL,
        [Description("本地KV数据库")]
        LocalKV,
        [Description("其它数据库,redis,mongdb等")]
        NoSQL
    }

   /// <summary>
   /// 传输请求
   /// </summary>
  public  class DBTransfer
    {
       
       public DBTransfer()
        {
            TimeOut = -1;
        }
        /// <summary>
        /// SQL语句
        /// </summary>
        public string SQL { get; set; }

        /// <summary>
        /// SQL参数
        /// </summary>
        public Dictionary<string,DBParameter> SQLParamter { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DBServerType DBServerType { get; set; }

        /// <summary>
        /// 是否是查询SQL
        /// </summary>
        public bool IsQuery { get; set; }

        /// <summary>
        /// 执行SQL是否是获取单值
        /// </summary>
        public bool IsScala { get; set; }

        /// <summary>
        /// 查询时是否转换成Model
        /// </summary>
        public bool IsModel { get; set; }

        /// <summary>
        /// IsModel=true,转换的model名称（包括名称空间）
        /// </summary>
        public string ModelCls { get; set; }

        /// <summary>
        /// 需要单独设置时，否则直接使用空间名称相同dll
        ///
        /// </summary>
        public string ModelDLL { get; set; }

        /// <summary>
        /// NoSQL参数
        /// </summary>
        public Dictionary<object,object> Paramter { get; set; }

        /// <summary>
        /// 数据库名称或者配置名称(SQL)
        /// </summary>
        public string DBCfg { get; set; }

        /// <summary>
        /// 请求ID,用于辨别返回请求
        /// 将会与返回对应，处理客户端的异步
        /// </summary>
        public long RequestID { get; set; }

        /// <summary>
        /// 执行超时，不是网络超时(秒)
        ///-1，采用服务端设置 0，此次执行用不超时，大于0则超时时间
        ///默认-1
        /// </summary>
        public int TimeOut { get; set; }
   
    }
}
