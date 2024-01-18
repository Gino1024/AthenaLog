using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Entity
{
    public class SysAthenaTaskRecordFailedEntity
    {
        public int id { get; set; }
        public Guid task_detail_id { get; set; }
        public string prefix { get; set; }
        public int total { get; set; }
        public string content { get; set; }
    }
}
