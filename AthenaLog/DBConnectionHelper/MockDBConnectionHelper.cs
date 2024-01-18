using System.Data.Common;

namespace AthenaLog.Common.DBConnectionHelper
{
    public class MockDBConnectionHelper : IDBConnectionHelper
    {
        /// <summary>
        /// 模擬開啟連線
        /// </summary>
        /// <param name="connectionIdentifier"></param>
        /// <returns></returns>
        public DbConnection CreateDbConnection(string connectionIdentifier)
        {
            DbConnection connection = null;
            return connection;
        }
    }
}
