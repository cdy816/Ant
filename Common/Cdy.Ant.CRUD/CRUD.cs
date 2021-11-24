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
    public class CRUD
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IDbConnection GetPostgreSQLConnection(string server,int port,string userId,string password,string database)
        {
            return  new NpgsqlConnection(String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};", server, port, userId, password, database));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IDbConnection GetSQLiteConnection(string database)
        {
            // return new SqliteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            return new SqliteConnection("Data Source="+ database);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IDbConnection GetMySqlConnection(string server, int port, string userId, string password, string database)
        {
            return new MySqlConnection(String.Format("Server={0};Port={1};User Id={2};Password={3};Database={4};",server, port, userId, password, database));
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="server"></param>
        ///// <param name="userId"></param>
        ///// <param name="password"></param>
        ///// <param name="database"></param>
        ///// <returns></returns>
        //public static IDbConnection GetDB2Connection(string server, string userId, string password, string database)
        //{
        //    return new DB2Connection(string.Format("Server={0};UID={1};PWD={2};Database={3};", server, userId, password, database));
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IDbConnection GetSqlServerConnection(string server, string database)
        {
            return new SqlConnection(string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True",server,database));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static IDbConnection GetSqlServerConnection(string server,  string userId, string password, string database)
        {
            return new SqlConnection(string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", server, database,userId,password));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class CRUDExtend
    {
        /// <summary>
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this IDbConnection db,string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.Query<T>(db, sql,param,transaction,buffered,commandTimeout,commandType);
        }

        /// <summary>
        /// Return a sequence of dynamic objects with properties matching the columns.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static IEnumerable<dynamic> Query(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return SqlMapper.Query(db, sql, param, transaction, buffered, commandTimeout, commandType);
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
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public static int Execute(this IDbConnection db, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            
            return SqlMapper.Execute(db, sql, param, transaction, commandTimeout, commandType);
        }
    }
}
