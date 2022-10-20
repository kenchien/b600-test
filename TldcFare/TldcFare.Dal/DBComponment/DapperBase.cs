using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace TldcFare.Dal.DBComponment
{
    public class DapperBase : IDapper
    {
        private readonly string _connectionString;

        public DapperBase(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection IDbConnection {
            get {
                IDbConnection _conn = new MySqlConnection(_connectionString);
                return _conn;
            }
        }

        public List<T> GetList<T>(string sqlString, object param = null, CommandType? commandType = CommandType.Text,
            int? commandTimeout = 180)
        {
            var list = new List<T>();

            using (var db = IDbConnection as System.Data.Common.DbConnection)
            {
                //db.Query 是dapper的延伸Extension,還有很多不錯的函數可以用
                IEnumerable<T> ts = db.Query<T>(sqlString, param, null, true, commandTimeout, commandType);

                if (ts != null)
                    list = ts.ToList();
            }

            return list;
        }

        /// <summary>
        /// 直接呼叫底層DbConnection.Execute
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="tra"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public int ExecSql(string sql, object param = null, IDbTransaction tra = null,
            int? commandTimeout = 6000, CommandType? commandType = CommandType.Text)
        {
            using var db = IDbConnection as System.Data.Common.DbConnection;
            int resultCount = db.Execute(sql, param, tra, commandTimeout, commandType);

            return resultCount;
        }

        public DataTable GetDataTable(string sqlString, object param = null, IDbTransaction tra = null,
            int? commandTimeout = 6000, CommandType? commandType = CommandType.Text)
        {
            DataSet ds = new DataSet();//ken,如果撈取的資料有pkey重複會產生錯誤,外面加一層DataSet.EnforceConstraints=false
            ds.EnforceConstraints = false;
            DataTable dt = ds.Tables.Add();
            //DataTable dt = new DataTable();

            using var db = IDbConnection as System.Data.Common.DbConnection;
            dt.Load(db.ExecuteReader(sqlString, param, tra, commandTimeout, commandType));

            return dt;
        }

        /// <summary>
        /// 直接呼叫底層DbConnection.ExecuteScalar
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="tra"></param>
        /// <param name="commandTimeout"></param>
        /// <param name="commandType"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object param = null, IDbTransaction tra = null,
            int? commandTimeout = 6000, CommandType? commandType = CommandType.Text)
        {
            using var db = IDbConnection as System.Data.Common.DbConnection;

            var resultCount = db.ExecuteScalar(sql, param, tra, commandTimeout, commandType);

            return resultCount;
        }

        /// <summary>
        /// insert and update (每個月初都要更新元件license,買斷要800美元,麻煩)
        /// </summary>
        /// <param name="DataSource"></param>
        /// <param name="DestinationTableName">要更新哪個table</param>
        /// <param name="AutoMapKeyName">p key</param>
        /// <returns></returns>
        //public void BulkMerge(object DataSource, string DestinationTableName, string AutoMapKeyName = null)
        //{
        //    using var db = IDbConnection as System.Data.Common.DbConnection;
        //    db.Open();

        //    //細部參數請參考 https://bulk-operations.net/options-batch
        //    using var bulk = new BulkOperation(db)
        //    {
        //        DestinationTableName = DestinationTableName,
        //    };
        //    if (!string.IsNullOrEmpty(AutoMapKeyName))
        //        bulk.AutoMapKeyName = AutoMapKeyName;

        //    bulk.BulkMerge(DataSource);
        //}


    }
}