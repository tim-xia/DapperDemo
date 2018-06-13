using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace DapperDemo
{
    public class DapperHelper : IDisposable
    {
        public enum Providers
        {
            SqlServer,
            Mysql
        }

        private IDbConnection Connection { get; }

        public DapperHelper(IDbConnection dbConnection)
        {
            Connection = dbConnection;
        }
        /// <summary>
        /// 支持已知数据库操作
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providers"></param>
        public DapperHelper(string connectionString, Providers providers)
        {
            switch (providers)
            {
                case Providers.Mysql:
                    Connection = new MySqlConnection(connectionString);
                    break;
                case Providers.SqlServer:
                    Connection = new SqlConnection(connectionString);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// 回收连接
        /// </summary>
        public void Dispose() => Connection.Dispose();

        #region 通用方法  
        public IDbTransaction BeginTransaction() => Connection.BeginTransaction();
        public IDbTransaction BeginTransaction(IsolationLevel il) => Connection.BeginTransaction(il);
        public void ChangeDatabase(string databaseName) => Connection.ChangeDatabase(databaseName);
        public void Close() => Connection.Close();
        public IDbCommand CreateCommand() => Connection.CreateCommand();

        public int Execute(string sql, object param = null) => Connection.Execute(sql, param);
        public IDataReader ExecuteReader(string sql, object param = null) => Connection.ExecuteReader(sql, param);
        public object ExecuteScalar(string sql, object param = null) => Connection.ExecuteScalar(sql, param);
        public SqlMapper.GridReader QueryMultiple(string sql, object param = null) => Connection.QueryMultiple(sql, param);

        public IEnumerable<dynamic> Query(string sql, object param = null) => Connection.Query(sql, param);
        public dynamic QueryFirst(string sql, object param = null) => Connection.QueryFirst(sql, param);
        public dynamic QueryFirstOrDefault(string sql, object param = null) => Connection.QueryFirstOrDefault(sql, param);
        public dynamic QuerySingle(string sql, object param = null) => Connection.QuerySingle(sql, param);
        public dynamic QuerySingleOrDefaul(string sql, object param = null) => Connection.QuerySingleOrDefault(sql, param);

        public IEnumerable<T> Query<T>(string sql, object param = null) => Connection.Query<T>(sql, param);
        public T QueryFirst<T>(string sql, object param = null) => Connection.QueryFirst<T>(sql, param);
        public T QueryFirstOrDefault<T>(string sql, object param = null) => Connection.QueryFirstOrDefault<T>(sql, param);
        public T QuerySingle<T>(string sql, object param = null) => Connection.QuerySingle<T>(sql, param);
        public T QuerySingleOrDefault<T>(string sql, object param = null) => Connection.QuerySingleOrDefault<T>(sql, param);

        public Task<int> ExecuteAsync(string sql, object param = null) => Connection.ExecuteAsync(sql, param);
        public Task<IDataReader> ExecuteReaderAsync(string sql, object param = null) => Connection.ExecuteReaderAsync(sql, param);
        public Task<object> ExecuteScalarAsync(string sql, object param = null) => Connection.ExecuteScalarAsync(sql, param);
        public Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object param = null) => Connection.QueryMultipleAsync(sql, param);

        public Task<IEnumerable<dynamic>> QueryAsync(string sql, object param = null) => Connection.QueryAsync(sql, param);
        public Task<dynamic> QueryFirstAsync(string sql, object param = null) => Connection.QueryFirstAsync(sql, param);
        public Task<dynamic> QueryFirstOrDefaultAsync(string sql, object param = null) => Connection.QueryFirstOrDefaultAsync(sql, param);
        public Task<dynamic> QuerySingleAsync(string sql, object param = null) => Connection.QuerySingleAsync(sql, param);
        public Task<dynamic> QuerySingleOrDefaultAsync(string sql, object param = null) => Connection.QuerySingleOrDefaultAsync(sql, param);

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null) => Connection.QueryAsync<T>(sql, param);
        public Task<T> QueryFirstAsync<T>(string sql, object param = null) => Connection.QueryFirstAsync<T>(sql, param);
        public Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null) => Connection.QueryFirstOrDefaultAsync<T>(sql, param);
        public Task<T> QuerySingleAsync<T>(string sql, object param = null) => Connection.QuerySingleAsync<T>(sql, param);
        public Task<T> QuerySingleOrDefaultAsync<T>(string sql, object param = null) => Connection.QuerySingleOrDefaultAsync<T>(sql, param);

        #endregion

        #region 参数拼接  
        /// <summary>  
        /// 获取类名字符串  
        /// </summary>  
        /// <typeparam name="T"></typeparam>  
        /// <returns></returns>  
        public static string ClassName<T>() => typeof(T).ToString().Split('.').Last();

        /// <summary>  
        /// 属性名称拼接并附加连接符  
        /// </summary>  
        /// <param name="separator">分隔符</param>  
        /// <param name="param">要拼接的动态类型</param>  
        /// <param name="isParam">是否为参数,即是否增加前缀'@'</param>  
        /// <returns></returns>  
        public static string Joint(string separator, object param, bool isParam = false)
        {
            var prefix = isParam ? "@" : string.Empty;
            var propertys = param.GetType().GetProperties().Select(t => $"{prefix}{t.Name}").ToArray();
            var joint = new StringBuilder();
            for (var i = 0; i < propertys.Length; i++)
            {
                joint.Append(i != 0 ? $"{separator}{propertys[i]}" : propertys[i]);
            }
            return joint.ToString();
        }

        /// <summary>  
        /// 以"param=@param"格式拼接属性名称并附加连接符  
        /// </summary>  
        /// <param name="separator">分隔符</param>  
        /// <param name="param">要拼接的动态类型</param>  
        /// <returns></returns>  
        public static string ParamJoint(string separator, object param)
        {
            var propertys = param.GetType().GetProperties().Where(t => t.GetValue(param) != null).Select(t => t.Name).Select(t => $"{t}=@{t}").ToArray();
            var joint = new StringBuilder();
            for (var i = 0; i < propertys.Length; i++)
            {
                joint.Append(i != 0 ? $"{separator}{propertys[i]}" : propertys[i]);
            }
            return joint.ToString();
        }

        /// <summary>  
        /// 将参数名和参数值拼接并附加连接符,用于where语句拼接  
        /// </summary>  
        /// <param name="separator"></param>  
        /// <param name="param"></param>  
        /// <returns></returns>  
        public static string ValueJoint(string separator, object param)
        {
            var joint = new StringBuilder();
            var count = 0;
            foreach (var item in param.GetType().GetProperties())
            {
                var value = item.GetValue(param, null);
                if (value == null) continue;
                var slice = $"{item.Name}=\'{value}\'";
                joint.Append(count != 0 ? $"{separator}{slice}" : slice);
                count++;
            }
            return joint.ToString();
        }

        #endregion

        #region 语句拼接  
        public static string CompileInsert<T>(object param)
        {
            return $"insert into {ClassName<T>()}({Joint(",", param)}) values ({Joint(",", param, true)})";
        }

        public static string CompileDelete<T>(object param)
        {
            return $"delete from {ClassName<T>()} where {ParamJoint(" and ", param)}";
        }

        public static string CompileUpdate<T>(object setParam, object whereParam)
        {
            return $"update {ClassName<T>()} set {ValueJoint(",", setParam)} where {ValueJoint(" and ", whereParam)}";
        }

        public static string CompileSelect<T>(object param)
        {
            return $"select {Joint(",", param)} from {ClassName<T>()}";
        }

        #endregion

        #region 便捷查询  
        public static T GetQuery<T>(DapperHelper conn, dynamic param)
        {
            return conn.QueryFirstOrDefault<T>($"select * from {ClassName<T>()} where {ParamJoint(" and ", param)}", param);
        }

        #endregion

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
