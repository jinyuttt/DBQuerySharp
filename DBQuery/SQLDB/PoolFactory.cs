/**
* 命名空间: SQLDB 
* 类 名：PoolFactory 
* CLR版本： 4.0.30319.42000
* 版本 ：v1.0
* Copyright (c) jinyu  
*/
using Hikari;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace SQLDB
{

    /// <summary>
    /// 功能描述    ：PoolFactory 加载连接池
    /// 创 建 者    ：jinyu
    /// 创建日期    ：2018
    /// 最后修改者  ：jinyu
    /// 最后修改日期：2018
    /// </summary>
   internal class PoolFactory
    {
        public  readonly static PoolFactory Instance = new PoolFactory();
        private object lock_obj = new object();
        private Dictionary<string, HikariDataSource> dicSource = new Dictionary<string, HikariDataSource>();
        private string cfgPath = "dbconfig";
        private string cfgFile = "hikari";
        /// <summary>
        /// 配置文件路径
        /// </summary>
        public  string PoolCfgPath { get { return cfgPath; } set { cfgPath = value; } }
        private PoolFactory()
        {

        }



        /// <summary>
        /// 加载连接池配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private IDbConnection CreatePool(string name)
        {
            lock (lock_obj)
            {
                HikariDataSource hikari = null;
                if (dicSource.TryGetValue(name, out hikari))
                {
                   return hikari.GetConnection();
                }
                else
                {
                    string file = Path.Combine(cfgPath, name + ".cfg");
                    if(!File.Exists(file))
                    {
                        throw new Exception("没有配置文件" + file);
                    }
                    HikariConfig hikariConfig = new HikariConfig();
                    hikariConfig.LoadConfig(file);
                    hikari = new HikariDataSource(hikariConfig);
                    dicSource[name] = hikari;
                    return hikari.GetConnection();
                }
            }
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDbConnection GetDbConnection(string name=null)
        {
            if(string.IsNullOrEmpty(name))
            {
                name = cfgFile;//使用默认名称
            }
              HikariDataSource hikari = null;
            if (dicSource.TryGetValue(name, out hikari))
            {
                return hikari.GetConnection();
            }
            else
            {
                return CreatePool(name);
            }
        }

        #region ADO.NET对象

        /// <summary>
        /// 获取驱动对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDbDataAdapter CreateDataAdapter (string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = cfgFile;//使用默认名称
            }
            HikariDataSource hikari = null;
            if (dicSource.TryGetValue(name, out hikari))
            {
                return hikari.DataAdapter;
            }
            else
            {
                CreatePool(name);
                if (dicSource.TryGetValue(name, out hikari))
                {
                    return hikari.DataAdapter;
                }
                else
                { return null; }

            }
        }

        /// <summary>
        /// 获取驱动对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDbCommand CreateDbCommand (string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = cfgFile;//使用默认名称
            }
            HikariDataSource hikari = null;
            if (dicSource.TryGetValue(name, out hikari))
            {
                return hikari.DbCommand;
            }
            else
            {
                CreatePool(name);
                if (dicSource.TryGetValue(name, out hikari))
                {
                    return hikari.DbCommand;
                }
                else
                { return null; }

            }
        }

        /// <summary>
        /// 获取驱动对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IDbDataParameter CreateDataParameter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = cfgFile;//使用默认名称
            }
            HikariDataSource hikari = null;
            if (dicSource.TryGetValue(name, out hikari))
            {
                return hikari.DataParameter;
            }
            else
            {
                CreatePool(name);
                if (dicSource.TryGetValue(name, out hikari))
                {
                    return hikari.DataParameter;
                }
                else
                { return null; }

            }
        }
        #endregion 

        public void ClearPool(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = cfgFile;//使用默认名称
            }
            HikariDataSource hikari = null;
            lock (lock_obj)
            {
                if (dicSource.TryGetValue(name, out hikari))
                {
                    dicSource.Remove(name);
                    hikari.Close();
                }
                
            }
        }

        /// <summary>
        /// 关闭所有
        /// </summary>
        public void ClearAllPool()
        {
            lock(lock_obj)
            {
                string[] keys =new string[ dicSource.Count];
                dicSource.Keys.CopyTo(keys, 0);
                foreach(string name in keys)
                {
                    ClearPool(name);
                }
            }
        }
    }
}
