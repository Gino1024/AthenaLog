using AthenaLog.Athena.Infrastructure.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Dto
{
    public class AthenaTaskQueryDto
    {
        public AthenaTaskQueryDto()
        {

        }
        public AthenaTaskQueryDto(SysAthenaTaskRecordEntity sysAthenaTaskRecordEntity)
        {
            tableName = sysAthenaTaskRecordEntity.target;
            partitionBy = sysAthenaTaskRecordEntity.partition_by;
            logScopeBy = sysAthenaTaskRecordEntity.log_scope_by;
            order_by = sysAthenaTaskRecordEntity.order_by;
            logStartTime = sysAthenaTaskRecordEntity.log_start_time.Value;
            logEndTime = sysAthenaTaskRecordEntity.log_end_time.Value;
        }
        public AthenaTaskQueryDto(SysAthenaTaskRecordEntity sysAthenaTaskRecordEntity, SysAtheneTaskDetailRecordEntity detailRecordEntity)
        {
            tableName = sysAthenaTaskRecordEntity.target;
            partitionBy = sysAthenaTaskRecordEntity.partition_by;
            logScopeBy = sysAthenaTaskRecordEntity.log_scope_by;
            order_by = sysAthenaTaskRecordEntity.order_by;
            logStartTime = sysAthenaTaskRecordEntity.log_start_time.Value;
            logEndTime = sysAthenaTaskRecordEntity.log_end_time.Value;
            page = detailRecordEntity.page;
            perPageCount = detailRecordEntity.page_per_count;
        }
        public string tableName { get; set; }
        public DateTime logStartTime { get; set; }
        public DateTime logEndTime { get; set; }
        public string logScopeBy { get; set; }
        public string partitionBy { get; set; }
        public string order_by { get; set; }
        public int? page { get; set; }
        public int? perPageCount { get; set; }

        public AthenaTaskQueryDto DeepCopy()
        {
            AthenaTaskQueryDto instance = new AthenaTaskQueryDto();
            instance.tableName = tableName;
            instance.logStartTime = logStartTime;
            instance.logEndTime = logEndTime;
            instance.logScopeBy = logScopeBy;
            instance.partitionBy = partitionBy;
            instance.order_by = order_by;
            instance.page = page;
            instance.perPageCount = perPageCount;
            return instance;
        }
    }
}
