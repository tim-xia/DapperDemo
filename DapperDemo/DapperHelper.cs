using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;

namespace DapperDemo
{
    public class DapperHelper
    {
        /// <summary>
        /// Sqls the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public static SqlConnection SqlConnection()
        {
            string sqlconnectionString = ConfigurationManager.ConnectionStrings["sqlconnectionString"].ToString();
            var connection = new SqlConnection(sqlconnectionString);
            connection.Open();
            return connection;
        }
        /// <summary>
        /// Mies the sql connection.
        /// </summary>
        /// <returns>The sql connection.</returns>
        public static MySqlConnection MySqlConnection()
        {
            string mysqlconnectionString = ConfigurationManager.ConnectionStrings["mysqlconnectionString"].ToString();
            var connection = new MySqlConnection(mysqlconnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns>The insert.</returns>
        public static int Insert()
        {
            int result = 0;
            using (var conn = DapperHelper.MySqlConnection())
            {
                Users user = new Users();
                user.Name = "Tim";
                user.Age = 30;
                string sqlCommandText = @"Insert into Users(Name,Age) Values(@Name,@Age)";
                result = conn.Execute(sqlCommandText, user);
            }
            return result;
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <returns>The insert.</returns>
        public static int BulkInsert()
        {
            int result = 0;
            using (var conn = DapperHelper.MySqlConnection())
            {
                IList<Users> users = new List<Users>();
                users.Add(new Users { Name = "Tim", Age = 30 });
                users.Add(new Users { Name = "Tim", Age = 25 });
                string sqlCommandText = @"Insert into Users(Name,Age) Values(@Name,@Age)";
                result = conn.Execute(sqlCommandText, users);
            }
            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns>The delete.</returns>
        public static int Delete()
        {
            int result = 0;
            using (var conn = DapperHelper.MySqlConnection())
            {
                Users user = new Users();
                user.ID = 2;
                string sqlCommandText = @"delete from Users where Id=@Id";
                result = conn.Execute(sqlCommandText, user);
            }
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns>The update.</returns>
        public static int Update()
        {
            int result = 0;
            using (IDbConnection conn = DapperHelper.MySqlConnection())
            {
                Users user = new Users();
                user.ID = 2;
                user.Name = "tim";
                user.Age = 31;
                string sqlCommandText = @"update users set Age=@Age where Id=@ID";
                result = conn.Execute(sqlCommandText, user);
            }
            return result;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns>The search.</returns>
        public static Users Search()
        {
            Users user = null;
            using (var conn = DapperHelper.MySqlConnection())
            {
                string sqlCommandText = @"select * from users where ID=@ID";
                user = conn.Query<Users>(sqlCommandText, new { ID = 2 }).FirstOrDefault();
                //防sql注入的写法 参数化
                //var p = new DynamicParameters();
                //p.Add("@ID", 2);
                //user=conn.Query<Users>(sqlCommandText,p).FirstOrDefault();

            }
            return user;
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <returns>The list.</returns>
        public static IList<Users> SearchList()
        {
            IList<Users> users = new List<Users>();
            int pageIndex = 0;
            int pageSize = 2;
            using(var conn=DapperHelper.MySqlConnection())
            {
                IDbTransaction trans = conn.BeginTransaction(); //增加事务
                string sqlCommandText =string.Format(@"select * from users limit {0},{1}",pageIndex*pageSize,pageSize);
                users = conn.Query<Users>(sqlCommandText,null,trans).ToList();
                trans.Commit();
            }
            return users;
        }

    }
}
