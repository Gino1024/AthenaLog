using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Entity
{
    public class SysAthenaTaskRecordEntity
    {
        public SysAthenaTaskRecordEntity()
        {

        }
        public SysAthenaTaskRecordEntity(Guid guid, SysAthenaTaskEntity sysAthenaTaskEntity)
        {
            this.task_id = guid;
            this.target = sysAthenaTaskEntity.target;
            this.partition_by = sysAthenaTaskEntity.partition_by;
            this.log_scope_by = sysAthenaTaskEntity.log_scope_by;
            this.order_by = sysAthenaTaskEntity.order_by;
            this.is_completed = 0;
            this.log_end_time = DateTime.Now;
            this.task_end_time = null;
        }
        /// <summary>
        /// Primary Key
        /// </summary>
        public Guid task_id { get; set; }
        /// <summary>
        /// 目標資料表
        /// </summary>
        public string target { get; set; }
        /// <summary>
        /// 資料存放上S3提供給Athena查詢的局部依據欄位
        /// </summary>
        public string partition_by { get; set; }
        /// <summary>
        /// 取得資料的範圍依據欄位
        /// </summary>
        public string log_scope_by { get; set; }
        /// <summary>
        /// 取得資料的排序依據欄位(單一欄位)
        /// </summary>
        public string order_by { get; set; }
        /// <summary>
        /// 上傳筆數
        /// </summary>
        public int upload_count { get; set; }
        /// <summary>
        /// 失敗筆數
        /// </summary>
        public int failed_count { get; set; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string msg { get; set; } = string.Empty;
        /// <summary>
        /// log_scope_by 起始時間
        /// </summary>
        public DateTime? log_start_time { get; set; }
        /// <summary>
        /// log_scope_by 結束時間
        /// </summary>
        public DateTime? log_end_time { get; set; }
        /// <summary>
        /// 是否有執行成功, 除了驗證失敗及Exception外, 執行完成都算成功
        /// 0: 執行中, 1: 執行完成, 2: 執行失敗
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 狀態(0: 詳細項目尚未完成, 1: 詳細項目皆執行完成
        /// </summary>
        public int is_completed { get; set; }
        public DateTime? task_start_time { get; set; }
        public DateTime? task_end_time { get; set; }
        public DateTime? update_time { get; set; }
    }
}
