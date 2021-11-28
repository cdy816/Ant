using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Collections.Generic;

namespace Cdy.Ant
{
    /// <summary>
    /// 
    /// </summary>
    public class SqlDatabase
    {
        /// <summary>
        /// 获取PostgreSQL 数据库连接
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="port">端口</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static IDbConnection GetPostgreSQLConnection(string server,int port,string userId,string password,string database)
        {
            return  new NpgsqlConnection(String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", server, port, userId, password, database));
        }

        /// <summary>
        /// 获取SqlLite 数据库连接
        /// </summary>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static IDbConnection GetSQLiteConnection(string database)
        {
            // return new SqliteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            return new SqliteConnection("Data Source="+ database);
        }

        /// <summary>
        /// 获取MySql 数据库连接
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="port">端口</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static IDbConnection GetMySqlConnection(string server, int port, string userId, string password, string database)
        {
            return new MySqlConnection(String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",server, port, userId, password, database));
        }

        /// <summary>
        /// 获取SqlServer 数据库连接
        /// 信任连接
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static IDbConnection GetSqlServerConnection(string server, string database)
        {
            return new SqlConnection(string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True",server,database));
        }

        /// <summary>
        /// 获取SqlServer 数据库连接
        /// 标准安全连接
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库</param>
        /// <returns></returns>
        public static IDbConnection GetSqlServerConnection(string server,  string userId, string password, string database)
        {
            return new SqlConnection(string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", server, database,userId,password));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class SqlDatabaseExtend
    {
        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql"> The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered"> Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">The command timeout (in seconds).</param>
        /// <param name="commandType"> The type of command to execute.</param>
        /// <returns>  A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed  (case insensitive).</returns>
        public static IEnumerable<T> Query<T>(this IDbConnection db,string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.Query<T>(db, sql,param,transaction,buffered,commandTimeout,commandType);
        }

        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="result">Query result:A sequence of data of the supplied type; if a basic type (int, string, etc) is queried then the data from the first column is assumed, otherwise an instance is created per row, and a direct column-name===member-name mapping is assumed  (case insensitive).</param>
        /// <returns></returns>
        public static IDbConnection Query<T>(this IDbConnection db, string sql, out IEnumerable<T> result)
        {
            result = SqlMapper.Query<T>(db, sql);
            return db;
        }

        /// <summary>
        /// Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="param">The parameters to pass, if any.</param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="commandTimeout">>The command timeout (in seconds).</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <returns>each row can be accessed via "dynamic", or by casting to an IDictionary<string,object></returns>
        public static IEnumerable<dynamic> Query(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.Query(db, sql, param, transaction, buffered, commandTimeout, commandType);
        }

        /// <summary>
        /// Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for the query.</param>
        /// <param name="result">Query result:each row can be accessed via "dynamic", or by casting to an IDictionary<string,object></param>
        /// <returns></returns>
        public static IDbConnection Query(this IDbConnection db, string sql, out IEnumerable<dynamic> result)
        {
            result = SqlMapper.Query(db, sql);
            return db;
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction">The transaction to use, if any.</param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static T QueryFirst<T>(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.QueryFirst<T>(db, sql, param, transaction,  commandTimeout, commandType);
        }

        /// <summary>
        /// Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static dynamic QueryFirst(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.QueryFirst(db, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Executes a single-row query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static T QuerySingle<T>(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.QuerySingle<T>(db, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Return a dynamic object with properties matching the columns.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static dynamic QuerySingle(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.QuerySingle(db, sql, param, transaction, commandTimeout, commandType);
        }

        /// <summary>
        /// Execute parameterized SQL.
        /// 
        /// 添加数据
        /// using (IDbConnection db = new SqlConnection("SqlServerConnection"))
        /// {
        ///        People people = new People{ Name = "越本山", Gender = "男", Phone = "15110834789"   };
        ///        string sql = "insert into People (Name,Gender,Phone)values(@Name,@Gender,@Phone)";
        ///        var result = db.Execute(sql, people);
        ///        var result = db.Execute(sql, new { Name = "越本山", Gender = "男", Phone = "15110834789" });
        /// }
        /// 
        /// 删除数据
        /// 
        ///  string sql = "delete from People where Id=@Id";
        ///  var result = db.Execute(sql, new { Id = 2 });
        /// 
        /// 更新数据
        /// 
        /// string sql = "update People set Name=@Name,Gender=@Gender,Phone=@Phone where Id=@Id";
        /// var result = db.Execute(sql, people);
        /// 
        /// </summary>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="result">The number of rows affected.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <returns></returns>
        public static IDbConnection Execute(this IDbConnection db, string sql,out int result,object param = null)
        {
            result = SqlMapper.Execute(db, sql, param);
            return db;
        }

        /// <summary>
        /// Execute parameterized SQL.
        /// 
        /// 添加数据
        /// using (IDbConnection db = new SqlConnection("SqlServerConnection"))
        /// {
        ///        People people = new People{ Name = "越本山", Gender = "男", Phone = "15110834789"   };
        ///        string sql = "insert into People (Name,Gender,Phone)values(@Name,@Gender,@Phone)";
        ///        var result = db.Execute(sql, people);
        ///        var result = db.Execute(sql, new { Name = "越本山", Gender = "男", Phone = "15110834789" });
        /// }
        /// 
        /// 删除数据
        /// 
        ///  string sql = "delete from People where Id=@Id";
        ///  var result = db.Execute(sql, new { Id = 2 });
        /// 
        /// 更新数据
        /// 
        /// string sql = "update People set Name=@Name,Gender=@Gender,Phone=@Phone where Id=@Id";
        /// var result = db.Execute(sql, people);
        /// 
        /// </summary>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <returns></returns>
        public static IDbConnection Execute(this IDbConnection db, string sql,  object param = null)
        {
            SqlMapper.Execute(db, sql, param);
            return db;
        }

        /// <summary>
        /// Execute parameterized SQL.
        /// 
        /// 添加数据
        /// using (IDbConnection db = new SqlConnection("SqlServerConnection"))
        /// {
        ///        People people = new People{ Name = "越本山", Gender = "男", Phone = "15110834789"   };
        ///        string sql = "insert into People (Name,Gender,Phone)values(@Name,@Gender,@Phone)";
        ///        var result = db.Execute(sql, people);
        ///        var result = db.Execute(sql, new { Name = "越本山", Gender = "男", Phone = "15110834789" });
        /// }
        /// 
        /// 删除数据
        /// 
        ///  string sql = "delete from People where Id=@Id";
        ///  var result = db.Execute(sql, new { Id = 2 });
        /// 
        /// 更新数据
        /// 
        /// string sql = "update People set Name=@Name,Gender=@Gender,Phone=@Phone where Id=@Id";
        /// var result = db.Execute(sql, people);
        /// 
        /// </summary>
        /// <param name="db">The connection to query on.</param>
        /// <param name="sql">The SQL to execute for this query.</param>
        /// <param name="param">The parameters to use for this query.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>The number of rows affected.</returns>
        public static int Execute(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.Execute(db, sql, param, transaction, commandTimeout, commandType);
        }
    }
}
