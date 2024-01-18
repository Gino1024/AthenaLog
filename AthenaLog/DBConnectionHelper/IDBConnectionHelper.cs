using System.Data.Common;

namespace AthenaLog.Common.DBConnectionHelper
{
    public interface IDBConnectionHelper
    {
        DbConnection CreateDbConnection(string connectionIdentifier);
    }
}
