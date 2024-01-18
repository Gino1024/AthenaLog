using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Entity
{
    public class SysAthenaTaskEntity
    {
        /// <summary>
        /// 目標資料表
        /// </summary>
        public string target { get; set; }
        /// <summary>
        /// 資料存放上S3提供給Athena查詢的局部依據欄位(由逗號分隔,可多個欄位)
        /// </summary>W
        public string partition_by { get; set; }
        /// <summary>
        /// 取得資料的範圍依據欄位(單一欄位 ex: create_date)
        /// </summary>
        public string log_scope_by { get; set; }
        /// <summary>
        /// 取得資料的排序依據欄位(單一欄位)
        /// </summary>
        public string order_by { get; set; }
        /// <summary>
        /// 1: 啟用 0: 停用
        /// </summary>
        public string is_enable { get; set; }
    }
}
