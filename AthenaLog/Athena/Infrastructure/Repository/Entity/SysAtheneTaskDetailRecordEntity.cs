using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Entity
{
    public class SysAtheneTaskDetailRecordEntity
    {
        public Guid task_detail_id { get; set; }
        public Guid task_id { get; set; }
        public int upload_count { get; set; }
        public int failed_count { get; set; }
        public string msg { get; set; } = string.Empty;
        public DateTime? task_start_time { get; set; }
        public DateTime? task_end_time { get; set; }
        public int page { get; set; }
        public int page_per_count { get; set; }
        /// <summary>
        /// 0:執行中, 1:執行完成, 2:執行失敗
        /// </summary>
        public int status { get; set; }
        public int is_completed { get; set; }
        public DateTime? update_time { get; set; }
    }
}
