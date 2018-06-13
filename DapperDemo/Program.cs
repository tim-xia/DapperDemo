using System;
using System.Collections.Generic;

namespace DapperDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // DapperHelper.Insert();
            // DapperHelper.BulkInsert();
            DapperHelper.Update();
            IList<Users> list = DapperHelper.SearchList();
            Users user = DapperHelper.Search();
            Console.Write(list.ToString());
            Console.WriteLine(user.ToString());

        }
    }
}
