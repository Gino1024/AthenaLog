using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Entity
{
    public class TableSchemaEntity
    {
        public string table_name { get; set; }
        public string column_name { get; set; }
        public string data_type { get; set; }
    }
}
