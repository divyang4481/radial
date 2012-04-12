﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;

namespace Radial.DataLite
{
    /// <summary>
    /// 数据会话
    /// </summary>
    public class DbSession : IDisposable
    {
        #region Fields

        DataSourceType _dsType;
        DbConnection _connection;
        DbTransaction _transaction;
        SqlQuery _sqlQuery;

        int _commandTimeout=30;//default 30 seconds

        #endregion

        #region Event

        /// <summary>
        /// SQL日志事件，在执行语句前触发
        /// </summary>
        public event LogEventHandler Log;

        #endregion

        #region Constructors

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        public DbSession()
            : this(0)
        {
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="substitution">占位符替换方法</param>
        public DbSession(PlaceholderSubstitution substitution)
            : this(0, substitution)
        {
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="settingsIndex">设置索引(从0开始)</param>
        public DbSession(int settingsIndex)
            : this(settingsIndex, null)
        {
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="settingsIndex">设置索引(从0开始)</param>
        /// <param name="substitution">占位符替换方法</param>
        public DbSession(int settingsIndex, PlaceholderSubstitution substitution)
        {
            ConnectionGroupSection section = ConnectionGroupSection.Read();
            ConnectionSettings settings = section.Connections[settingsIndex];
            if (settings == null)
                throw new ArgumentException("无法找到索引为\"" + settingsIndex + "\"的数据库设置");

            string connectionString = settings.ConnectionString;

            if (substitution != null)
                connectionString = substitution(connectionString);

            Initialize(connectionString, settings.DataSourceType);
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="settingsName">设置名称(不区分大小写)</param>
        public DbSession(string settingsName)
            : this(settingsName, null)
        {
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="settingsName">设置名称(不区分大小写)</param>
        /// <param name="substitution">占位符替换方法</param>
        public DbSession(string settingsName, PlaceholderSubstitution substitution)
        {
            ConnectionGroupSection section = ConnectionGroupSection.Read();
            ConnectionSettings settings = section.Connections[settingsName];
            if (settings == null)
                throw new ArgumentException("无法找到名称为\"" + settingsName + "\"的数据库设置");
            
            string connectionString = settings.ConnectionString;

            if (substitution != null)
                connectionString = substitution(connectionString);

            Initialize(connectionString, settings.DataSourceType);
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dsType">数据源类型</param>
        public DbSession(string connectionString, DataSourceType dsType)
        {
            Initialize(connectionString, dsType);
        }

        /// <summary>
        /// 初始化数据会话
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="dsType">数据源类型</param>
        private void Initialize(string connectionString, DataSourceType dsType)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", "用于打开连接的字符串不能为空");

            SqlQuery = QueryFactory.CreateSqlQueryInstance(dsType);
            DataSourceType = dsType;
            Connection = SqlQuery.DbProvider.CreateConnection();
            Connection.ConnectionString = connectionString;
        }

        #endregion

        #region Properties

        /// <summary>
        /// 获取打开连接的字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
        }

        /// <summary>
        /// 获取数据源类型
        /// </summary>
        public DataSourceType DataSourceType
        {
            get
            {
                return _dsType;
            }
            private set
            {
                _dsType = value;
            }
        }

        /// <summary>
        /// 获取或设置数据库连接
        /// </summary>
        private DbConnection Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;
            }
        }

        /// <summary>
        /// 获取要在数据源执行的DbTransaction事务对象
        /// </summary>
        public DbTransaction Transaction
        {
            get
            {
                return _transaction;
            }
            private set
            {
                _transaction = value;
            }
        }

        /// <summary>
        /// 获取或设置Sql语句查询类实例
        /// </summary>
        private SqlQuery SqlQuery
        {
            get
            {
                return _sqlQuery;
            }
            set
            {
                _sqlQuery = value;
            }
        }

        /// <summary>
        /// 获取或设置在终止执行命令的尝试并生成错误之前的等待时间(以秒为单位，默认值为 30 秒)
        /// </summary>
        public int CommandTimeout
        {
            get
            {
                return _commandTimeout;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
                _commandTimeout = value;
            }
        }

        #endregion

        #region Transaction

        /// <summary>
        /// 开启DbTransaction事务
        /// </summary>
        public void BeginTransaction()
        {
            BeginTransaction(IsolationLevel.Unspecified);
        }


        /// <summary>
        /// 开启DbTransaction事务
        /// </summary>
        /// <param name="level">锁定行为</param>
        public void BeginTransaction(IsolationLevel level)
        {
            OpenConnection();
            if (Transaction == null || Transaction.Connection == null)
                Transaction = Connection.BeginTransaction(level);
        }

        /// <summary>
        /// 提交DbTransaction事务(需调用EndTransaction关闭事务)
        /// </summary>
        public void Commit()
        {
            Commit(false);
        }

        /// <summary>
        /// 提交DbTransaction事务
        /// </summary>
        /// <param name="endTransaction">提交后关闭事务</param>
        public void Commit(bool endTransaction)
        {
            if (Transaction != null)
            {
                Transaction.Commit();
                if (endTransaction)
                    EndTransaction();
            }
        }

        /// <summary>
        /// 回滚DbTransaction事务(需调用EndTransaction关闭事务)
        /// </summary>
        public void Rollback()
        {
            Rollback(false);
        }

        /// <summary>
        /// 回滚DbTransaction事务
        /// </summary>
        /// <param name="endTransaction">回滚后关闭事务</param>
        public void Rollback(bool endTransaction)
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
                if (endTransaction)
                    EndTransaction();
            }
        }

        /// <summary>
        /// 关闭DbTransaction事务
        /// </summary>
        public void EndTransaction()
        {
            if (Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }
        }

        #endregion

        #region Connection

        /// <summary>
        /// 打开数据连接
        /// </summary>
        private void OpenConnection()
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Closed)
                    Connection.Open();
            }
        }

        /// <summary>
        /// 关闭数据连接
        /// </summary>
        private void CloseConnection()
        {
            if (Connection != null)
            {
                if (Connection.State == ConnectionState.Closed)
                    return;
                try
                {
                    Connection.Close();
                    EndTransaction();
                }
                finally
                {
                    Connection.Dispose();
                }
            }
        }

        #endregion

        #region Log Block

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        private void RenderLog(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                return;

            string cmdText = command.CommandText;

            List<string> paramTextList = new List<string>();

            foreach (DbParameter p in command.Parameters)
            {
                paramTextList.Add(string.Format("{0}={1}", p.ParameterName, p.Value.ToString()));
            }

            string paramText = string.Join(",", paramTextList.ToArray());
            string logText = string.Format("{0} ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            if (command.CommandType == CommandType.Text)
                logText += string.Format("{0}{1}", cmdText, Environment.NewLine);
            if (command.CommandType == CommandType.StoredProcedure)
                logText += string.Format("Stored Procedure Name:{0}{1}", cmdText, Environment.NewLine);
            if (command.CommandType == CommandType.TableDirect)
                logText += string.Format("Table Name:{0}{1}", cmdText, Environment.NewLine);

            if (!string.IsNullOrEmpty(paramText))
                logText += paramText;
            if (Connection != null && Connection.State == ConnectionState.Open)
                logText += string.Format("{0}--DataSource:{1} Version:{2}", string.IsNullOrEmpty(paramText) ? string.Empty : " ", DataSourceType, Connection.ServerVersion);
            if (Transaction != null)
                logText += " With DbTransaction";
            logText += Environment.NewLine;

            OnLog(new LogEventArgs(logText));
        }

        /// <summary>
        /// 触发日志输出事件
        /// </summary>
        /// <param name="e">日志事件参数</param>
        private void OnLog(LogEventArgs e)
        {
            if (Log == null)
                return;
            if (e == null)
                throw new ArgumentException("日志事件参数不能为空");

            Log(this, e);
        }

        #endregion

        #region Create DbParameter

        /// <summary>
        /// 返回实现 System.Data.Common.DbParameter 类的提供程序的类的一个新实例。
        /// </summary>
        /// <returns>System.Data.Common.DbParameter 的新实例。</returns>
        public DbParameter CreateParameter()
        {
            return SqlQuery.DbProvider.CreateParameter();
        }

        #endregion

        #region Private Execute Methods

        /// <summary>
        /// ExecuteDataSet方法
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        /// <returns>DataSet对象</returns>
        private DataSet ExecuteDataSet(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentNullException("command", "DbCommand对象及其命令文本不能为空");

            DataSet ds = new DataSet();

            if (!command.CommandText.EndsWith(";"))
                command.CommandText += ";";

            command.Connection = Connection;
            command.CommandTimeout = CommandTimeout;

            if (Transaction != null)
                command.Transaction = Transaction;

           
            DbDataAdapter adapter = SqlQuery.DbProvider.CreateDataAdapter();
            adapter.SelectCommand = command;

            OpenConnection();

            RenderLog(command);

            adapter.Fill(ds);

            return ds;
        }

        /// <summary>
        /// ExecuteDataTable方法
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        /// <returns>DataTable对象</returns>
        private DataTable ExecuteDataTable(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentNullException("command", "DbCommand对象及其命令文本不能为空");

            DataTable dt = new DataTable();

            if (!command.CommandText.EndsWith(";"))
                command.CommandText += ";";

            command.Connection = Connection;
            command.CommandTimeout = CommandTimeout;

            if (Transaction != null)
                command.Transaction = Transaction;

            DbDataAdapter adapter = SqlQuery.DbProvider.CreateDataAdapter();
            adapter.SelectCommand = command;

            OpenConnection();

            RenderLog(command);

            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// ExecuteDataReader方法
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        /// <returns>DataReader对象</returns>
        private DbDataReader ExecuteDataReader(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentNullException("command", "DbCommand对象及其命令文本不能为空");

            if (!command.CommandText.EndsWith(";"))
                command.CommandText += ";";

            command.Connection = Connection;
            command.CommandTimeout = CommandTimeout;

            if (Transaction != null)
                command.Transaction = Transaction;

            OpenConnection();

            RenderLog(command);
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// ExecuteNonQuery方法
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        /// <returns>影响的行数</returns>
        private int ExecuteNonQuery(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentNullException("command", "DbCommand对象及其命令文本不能为空");

            if (!command.CommandText.EndsWith(";"))
                command.CommandText += ";";

            command.Connection = Connection;
            command.CommandTimeout = CommandTimeout;

            if (Transaction != null)
                command.Transaction = Transaction;

            OpenConnection();

            RenderLog(command);
            int count = command.ExecuteNonQuery();
            return count;
        }

        /// <summary>
        /// ExecuteScalar方法
        /// </summary>
        /// <param name="command">DbCommand对象</param>
        /// <returns>object对象</returns>
        private object ExecuteScalar(DbCommand command)
        {
            if (command == null || string.IsNullOrEmpty(command.CommandText))
                throw new ArgumentNullException("command", "DbCommand对象及其命令文本不能为空");

            if (!command.CommandText.EndsWith(";"))
                command.CommandText += ";";

            command.Connection = Connection;
            command.CommandTimeout = CommandTimeout;

            if (Transaction != null)
                command.Transaction = Transaction;

            OpenConnection();

            RenderLog(command);
            object obj = command.ExecuteScalar();
            return obj;
        }

        #endregion

        #region Public Execute Methods

        /// <summary>
        /// ExecuteDataSet方法
        /// </summary>
        /// <param name="cmdText">命令文本</param>
        /// <param name="paramValues">参数数组</param>
        /// <returns>DataSet对象</returns>
        public DataSet ExecuteDataSet(string cmdText, params object[] paramValues)
        {
            if (string.IsNullOrEmpty(cmdText))
                throw new ArgumentNullException("cmdText", "cmdText不能为空");

            TextCommandData data = new TextCommandData(cmdText, paramValues);

            return ExecuteDataSet(data);
        }

        /// <summary>
        /// ExecuteDataTable方法
        /// </summary>
        /// <param name="cmdText">命令文本</param>
        /// <param name="paramValues">参数数组</param>
        /// <returns>DataTable对象</returns>
        public DataTable ExecuteDataTable(string cmdText, params object[] paramValues)
        {
            if (string.IsNullOrEmpty(cmdText))
                throw new ArgumentNullException("cmdText", "cmdText不能为空");

            TextCommandData data = new TextCommandData(cmdText, paramValues);

            return ExecuteDataTable(data);
        }

        /// <summary>
        /// ExecuteDataReader方法
        /// </summary>
        /// <param name="cmdText">命令文本</param>
        /// <param name="paramValues">参数数组</param>
        /// <returns>DataReader对象</returns>
        public DbDataReader ExecuteDataReader(string cmdText, params object[] paramValues)
        {
            if (string.IsNullOrEmpty(cmdText))
                throw new ArgumentNullException("cmdText", "cmdText不能为空");

            TextCommandData data = new TextCommandData(cmdText, paramValues);

            return ExecuteDataReader(data);
        }

        /// <summary>
        /// ExecuteNonQuery方法
        /// </summary>
        /// <param name="cmdText">命令文本</param>
        /// <param name="paramValues">参数数组</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(string cmdText, params object[] paramValues)
        {
            if (string.IsNullOrEmpty(cmdText))
                throw new ArgumentNullException("cmdText", "cmdText不能为空");

            TextCommandData data = new TextCommandData(cmdText, paramValues);

            return ExecuteNonQuery(data);
        }

        /// <summary>
        /// ExecuteScalar方法
        /// </summary>
        /// <param name="cmdText">命令文本</param>
        /// <param name="paramValues">参数数组</param>
        /// <returns>object对象</returns>
        public object ExecuteScalar(string cmdText, params object[] paramValues)
        {
            if (string.IsNullOrEmpty(cmdText))
                throw new ArgumentNullException("cmdText", "cmdText不能为空");

            TextCommandData data = new TextCommandData(cmdText, paramValues);

            return ExecuteScalar(data);
        }

        /// <summary>
        /// ExecuteDataSet方法
        /// </summary>
        /// <param name="cmdData">文本命令对象</param>
        /// <returns>DataSet对象</returns>
        public DataSet ExecuteDataSet(TextCommandData cmdData)
        {
            if (cmdData == null)
                throw new ArgumentNullException("cmdData","cmdData不能为空");

            DbCommand cmd = cmdData.CreateCommand(SqlQuery.DbProvider, SqlQuery.CreateParameterName);

            return ExecuteDataSet(cmd);
        }

        /// <summary>
        /// ExecuteDataTable方法
        /// </summary>
        /// <param name="cmdData">文本命令对象</param>
        /// <returns>DataTable对象</returns>
        public DataTable ExecuteDataTable(TextCommandData cmdData)
        {
            if (cmdData == null)
                throw new ArgumentNullException("cmdData", "cmdData不能为空");

            DbCommand cmd = cmdData.CreateCommand(SqlQuery.DbProvider, SqlQuery.CreateParameterName);

            return ExecuteDataTable(cmd);
        }

        /// <summary>
        /// ExecuteDataReader方法
        /// </summary>
        /// <param name="cmdData">文本命令对象</param>
        /// <returns>DbDataReader对象</returns>
        public DbDataReader ExecuteDataReader(TextCommandData cmdData)
        {
            if (cmdData == null)
                throw new ArgumentNullException("cmdData", "cmdData不能为空");

            DbCommand cmd = cmdData.CreateCommand(SqlQuery.DbProvider, SqlQuery.CreateParameterName);

            return ExecuteDataReader(cmd);
        }

        /// <summary>
        /// ExecuteNonQuery方法
        /// </summary>
        /// <param name="cmdData">文本命令对象</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(TextCommandData cmdData)
        {
            if (cmdData == null)
                throw new ArgumentNullException("cmdData", "cmdData不能为空");

            DbCommand cmd = cmdData.CreateCommand(SqlQuery.DbProvider, SqlQuery.CreateParameterName);

            return ExecuteNonQuery(cmd);
        }

        /// <summary>
        /// ExecuteScalar方法
        /// </summary>
        /// <param name="cmdData">文本命令对象</param>
        /// <returns>object对象</returns>
        public object ExecuteScalar(TextCommandData cmdData)
        {
            if (cmdData == null)
                throw new ArgumentNullException("cmdData", "cmdData不能为空");

            DbCommand cmd = cmdData.CreateCommand(SqlQuery.DbProvider, SqlQuery.CreateParameterName);

            return ExecuteScalar(cmd);
        }

        #endregion

        #region Public Execute Stored Procedure Methods

        /// <summary>
        /// Stored Procedure ExecuteDataSet
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>DataSet对象</returns>
        public DataSet ExecuteSpDataSet(string spName, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentNullException("spName", "存储过程名不能为空");

            DbCommand cmd = SqlQuery.DbProvider.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (CommandTimeout < 0)
                throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
            cmd.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    //check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }

                    cmd.Parameters.Add(p);
                }
            }
            return ExecuteDataSet(cmd);
        }


        /// <summary>
        /// Stored Procedure ExecuteDataTable
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>DataTable对象</returns>
        public DataTable ExecuteSpDataTable(string spName, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentNullException("spName", "存储过程名不能为空");

            DbCommand cmd = SqlQuery.DbProvider.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (CommandTimeout < 0)
                throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
            cmd.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    //check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }

                    cmd.Parameters.Add(p);
                }
            }

            return ExecuteDataTable(cmd);
        }


        /// <summary>
        /// Stored Procedure ExecuteDataReader
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>DbDataReader对象</returns>
        public DbDataReader ExecuteSpDataReader(string spName, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentNullException("spName", "存储过程名不能为空");

            DbCommand cmd = SqlQuery.DbProvider.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (CommandTimeout < 0)
                throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
            cmd.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    //check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }

                    cmd.Parameters.Add(p);
                }
            }
            return ExecuteDataReader(cmd);
        }


        /// <summary>
        /// Stored Procedure ExecuteNonQuery
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>影响的行数</returns>
        public int ExecuteSpNonQuery(string spName, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentNullException("spName", "存储过程名不能为空");

            DbCommand cmd = SqlQuery.DbProvider.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (CommandTimeout < 0)
                throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
            cmd.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    //check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }

                    cmd.Parameters.Add(p);
                }
            }
            return ExecuteNonQuery(cmd);
        }


        /// <summary>
        /// Stored Procedure ExecuteScalar
        /// </summary>
        /// <param name="spName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>object对象</returns>
        public object ExecuteSpScalar(string spName, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentNullException("spName", "存储过程名不能为空");

            DbCommand cmd = SqlQuery.DbProvider.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = spName;

            if (CommandTimeout < 0)
                throw new ArgumentException("CommandTimeout所分配的属性值不得小于0");
            cmd.CommandTimeout = CommandTimeout;

            if (parameters != null)
            {
                foreach (DbParameter p in parameters)
                {
                    //check for derived output value with no value assigned
                    if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                    {
                        p.Value = DBNull.Value;
                    }

                    cmd.Parameters.Add(p);
                }
            }

            return ExecuteScalar(cmd);
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            CloseConnection();
        }

        #endregion
    }
}
