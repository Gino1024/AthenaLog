using AthenaLog.Athena.Infrastructure.Repository.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Repository.Interface
{
    public interface IAthenaRepository
    {
        IEnumerable<SysAthenaTaskEntity> GetAthenaTaskAll();
        IEnumerable<SysAthenaTaskRecordEntity> GetAthenaTaskRecordAllLastRecordByTargetList(IEnumerable<string> targetNames);
        IEnumerable<TableSchemaEntity> GetTargetTableInfo(IEnumerable<string> targetNames);
        int GetCurrentTaskDataCountByQuery(AthenaTaskQueryDto queryDto);
        IEnumerable<Object> GetAthenaTaskDetailByQuery(AthenaTaskQueryDto queryDto);
        IEnumerable<SysAthenaTaskRecordEntity> GetAthenaTaskRecordNotCompleted();
        IEnumerable<SysAtheneTaskDetailRecordEntity> GetAthenaDetailRecordByTaskId(Guid taskId);
        IEnumerable<SysAthenaTaskRecordFailedEntity> GetRecordFailedByTaskDetailId(Guid taskDetailId);
        bool InsertAthenaTaskRecord(IEnumerable<SysAthenaTaskRecordEntity> instance);
        bool UpdateAthenaTaskRecord(SysAthenaTaskRecordEntity instance);
        bool InsertAthenaTaskRecordDetail(IEnumerable<SysAtheneTaskDetailRecordEntity> instance);
        bool UpdateAthenaTaskRecordDetail(SysAtheneTaskDetailRecordEntity instance);
        bool InsertAthenaTaskRecordFailed(IEnumerable<SysAthenaTaskRecordFailedEntity> instance);
        bool DeleteAthenaTaskRecordFailedByTaskDetailId(Guid taskDetailId);
    }
}
