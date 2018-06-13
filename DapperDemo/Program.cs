using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace DapperDemo
{
    class Program
    {
       public static readonly string sqlConnectionString = ConfigurationManager.ConnectionStrings["sqlconnectionString"].ToString();

        static void Main(string[] args)
        {
            DapperHelper dapperHelper = new DapperHelper(sqlConnectionString, DapperHelper.Providers.SqlServer);
            Users users = new Users { ID = 3, Name = "test", Age = 32, Sex = "男" };
            dapperHelper.Execute(DapperHelper.CompileInsert<Users>(new Users()), users);
            IEnumerable<Users> list= dapperHelper.Query<Users>("select * from Users",null);
        }
    }
}
