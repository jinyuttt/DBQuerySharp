using ISQLDB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace SQLDB
{

   /// <summary>
   /// 采用自定义连接池实现
   /// 需要通过配置文件配置连接池信息
   /// 
   /// </summary>
   public class DBAcessPool: SQLAcess
    {
        /// <summary>
        /// 当前连接
        /// </summary>
        private IDbConnection Connection = null;

        private IDbCommand Command = null;
        private string poolCfgName = "hikari";


        private readonly static DBAcess instance = new DBAcess();
        
        /// <summary>
        /// 当前连接
        /// </summary>
        public IDbConnection Current { get { return Connection; } }

        public string DBName { get { return poolCfgName; } set { poolCfgName = value; } }

        public override void Close()
        {
           if(null!=Connection)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
           if(null!=Command)
            {
                Command.Dispose();
                Command = null;
            }
        }


        public override IDbDataAdapter CreateDataAdapter()
        {
            return PoolFactory.Instance.CreateDataAdapter(DBName);
        }

        public override int ExecuteUpdate(string sql, bool scalar = false)
        {
            IDbCommand command = NewCommand(sql);
            int r = -1;
            if (scalar)
            {
                r = (int)command.ExecuteScalar();
            }
            else
            {
                r = command.ExecuteNonQuery();
            }
            return r;
        }



        public override IDataReader GetDataReader(IDbConnection connection, string sql)
        {
            IDbCommand command = NewCommand(connection);
            command.CommandText = sql;
            
            return command.ExecuteReader();
        }

        public override IDataReader GetDataReader(string sql)
        {
            return GetDataReader(NewConnect(), sql);
        }
        public override IDbTransaction GetDbTransaction(IDbConnection con)
        {
            return con.BeginTransaction();
        }


        public override IDbTransaction GetTransaction()
        {
            return NewConnect().BeginTransaction();
        }


        public override IDbCommand NewCommand(IDbConnection connection)
        {
            Command =  connection.CreateCommand();
            return Command;
        }

        public override IDbCommand NewCommand(IDbConnection connection, string sql)
        {
            IDbCommand command = NewCommand(connection);
            command.CommandText = sql;
            return command;
        }

        public override IDbCommand NewCommand(string sql)
        {
            return NewCommand(NewConnect(), sql);
        }

        public override IDbConnection NewConnect()
        {
           
            IDbConnection con = PoolFactory.Instance.GetDbConnection();
            con.Open();
            Connection = con;
            return con;

        }

        private IDbDataParameter[] CreateParameters(Dictionary<string, DBParameter> paramObj)
        {
            List<IDbDataParameter> list = new List<IDbDataParameter>();
            foreach (KeyValuePair<string, DBParameter> kv in paramObj)
            {
                IDbDataParameter npgsql = GetDataParameter();
                npgsql.ParameterName = kv.Key.Trim().StartsWith("@") ? kv.Key : "@" + kv.Key;
                npgsql.Value = kv.Value;
                npgsql.DbType = EnumConvert.ToEnum<DbType>(kv.Value.DbType);
                npgsql.Direction = EnumConvert.ToEnum<ParameterDirection>(kv.Value.ParameterDirection);
                list.Add(npgsql);

            }
            return list.ToArray();
        }


        public override int ExecuteUpdateWithParameter(string sql, bool scalar = false, Dictionary<string, DBParameter> param = null)
        {
            if (param == null || param.Count == 0)
            {
                return ExecuteUpdate(sql, scalar);
            }
            else
            {
                int r = -1;
                IDbCommand command = this.NewCommandWithParameter(sql, param);

                if (scalar)
                {
                    r = (int)command.ExecuteScalar();
                }
                else
                {
                    r = command.ExecuteNonQuery();
                }
                return r;
            }
        }

        public override IDataReader GetDataReaderWithParameter(IDbConnection connection, string sql, Dictionary<string, DBParameter> param = null)
        {
            if (param == null || param.Count == 0)
            {
                return this.GetDataReader(connection, sql);
            }
            else
            {

                IDbCommand command = this.NewCommand(connection, sql);
                IDbDataParameter[] parameters = CreateParameters(param);
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                return command.ExecuteReader();
            }
        }

        public override IDataReader GetDataReaderWithParameter(string sql, Dictionary<string, DBParameter> param = null)
        {
            if (param == null || param.Count == 0)
            {
                return this.GetDataReader(sql);
            }
            else
            {

                IDbCommand command = this.NewCommandWithParameter(sql, param);
                return command.ExecuteReader();
            }
        }

        public override DataSet GetSelectWithParameter(string sql, Dictionary<string, DBParameter> param = null)
        {
            if (param == null || param.Count == 0)
            {
                return this.GetSelect(sql);
            }
            else
            {

                IDbCommand command = this.NewCommandWithParameter(sql, param);
                DataSet ds = new DataSet();
                IDbDataAdapter adapter = CreateDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(ds);

                return ds;
            }
        }

        public override IDbCommand NewCommandWithParameter(string sql, Dictionary<string, DBParameter> param = null)
        {
            if (param == null || param.Count == 0)
            {
                return this.NewCommand(sql);
            }
            else
            {

                IDbCommand command = this.NewCommand(sql);
                IDbDataParameter[] parameters = CreateParameters(param);
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                return command;
            }
        }

        public override IDbCommand CreateCommand()
        {
            return PoolFactory.Instance.CreateDbCommand(DBName);
        }

        public override IDbDataParameter GetDataParameter()
        {
            return PoolFactory.Instance.CreateDataParameter(DBName);
        }

        public override DataSet GetSelect(string sql)
        {
            IDbCommand command = this.NewCommand(sql);
            DataSet ds = new DataSet();
            IDbDataAdapter adapter = CreateDataAdapter();
            adapter.SelectCommand = command;
            adapter.Fill(ds);
            command.Dispose();
            return ds;
        }
    }
}
