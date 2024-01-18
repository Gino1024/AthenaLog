using AthenaLog.Common.DBConnectionHelper;
using AthenaLog.Athena.Infrastructure.Repository.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using System;
using System.Collections.Generic;
using Dapper;

namespace AthenaLog.Athena.Infrastructure.Repository.Implement
{
    public class AthenaRepository : IAthenaRepository
    {
        private IDBConnectionHelper _dbConnectionHelper;
        public AthenaRepository(IDBConnectionHelper dbConnectionHelper)
        {
            _dbConnectionHelper = dbConnectionHelper;
        }
        public IEnumerable<SysAthenaTaskEntity> GetAthenaTaskAll()
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"select * 
                                from sys_athena_task
                                where is_enable = '1'";
                var result = conn.Query<SysAthenaTaskEntity>(sql);
                return result;
            }
        }
        public IEnumerable<SysAthenaTaskRecordEntity> GetAthenaTaskRecordAllLastRecordByTargetList(IEnumerable<string> targetNames)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"with tmpRecord as (
	                                select  target,
		                                partition_by,
		                                log_scope_by,
		                                order_by,
                                        log_start_time,
                                        log_end_time,
		                                ROW_NUMBER() over (partition by target order by log_end_time desc) as rn
	                                from 
		                                sys_athena_task_record
	                                where target in @targets
                                    and  status in (0,1,2)
                                )
                                select * from tmpRecord
                                where rn = '1'";
                DynamicParameters paras = new DynamicParameters();
                paras.Add("@targets", targetNames);

                var result = conn.Query<SysAthenaTaskRecordEntity>(sql, paras);
                return result;
            }
        }
        public int GetCurrentTaskDataCountByQuery(AthenaTaskQueryDto queryDto)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = $@"select count(1) 
                                from 
                                    {queryDto.tableName}
                                where 
                                    {queryDto.logScopeBy} > @logStartTime and {queryDto.logScopeBy} <= @logEndTime";

                DynamicParameters paras = new DynamicParameters();
                paras.Add("@logStartTime", queryDto.logStartTime);
                paras.Add("@logEndTime", queryDto.logEndTime);

                var result = conn.ExecuteScalar<int>(sql, paras);
                return result;
            }
        }
        public IEnumerable<Object> GetAthenaTaskDetailByQuery(AthenaTaskQueryDto queryDto)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = $@"select *
                                from 
                                    {queryDto.tableName}
                                where 
                                    {queryDto.logScopeBy} > @logStartTime and {queryDto.logScopeBy} <= @logEndTime
                                order by
                                    {queryDto.order_by}
                               ";

                DynamicParameters paras = new DynamicParameters();
                paras.Add("@logStartTime", queryDto.logStartTime);
                paras.Add("@logEndTime", queryDto.logEndTime);;

                if (queryDto.page.HasValue && queryDto.perPageCount.HasValue)
                {
                    sql += @"offset @offset rows
                             fetch next @next rows only";

                    int offset = (queryDto.page.Value - 1) * queryDto.perPageCount.Value;

                    paras.Add("@offset", offset);
                    paras.Add("@next", queryDto.perPageCount);
                }

                var result = conn.Query(sql, paras);
                return result;
            }
        }
        public IEnumerable<TableSchemaEntity> GetTargetTableInfo(IEnumerable<string> targetNames)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"SELECT
                                    table_name,
                                    column_name,
                                    data_type
                                FROM
                                    information_schema.columns
                                WHERE
                                    table_name IN @targets";
                DynamicParameters paras = new DynamicParameters();
                paras.Add("@targets", targetNames);

                var result = conn.Query<TableSchemaEntity>(sql, paras);
                return result;
            }
        }
        public IEnumerable<SysAthenaTaskRecordEntity> GetAthenaTaskRecordNotCompleted()
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"SELECT
                                    *
                                FROM
                                    sys_athena_task_record
                                WHERE
                                    is_completed = 0
                                and status in (1,2)";

                var result = conn.Query<SysAthenaTaskRecordEntity>(sql);
                return result;
            }
        }
        public IEnumerable<SysAtheneTaskDetailRecordEntity> GetAthenaDetailRecordByTaskId(Guid taskId)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"SELECT
                                    *
                                FROM
                                    [sys_athena_task_detail_record]
                                WHERE
                                    task_id = @task_id";

                DynamicParameters paras = new DynamicParameters();
                paras.Add("task_id", taskId);

                var result = conn.Query<SysAtheneTaskDetailRecordEntity>(sql, paras);
                return result;
            }
        }
        public IEnumerable<SysAthenaTaskRecordFailedEntity> GetRecordFailedByTaskDetailId(Guid taskDetailId)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"SELECT
                                    *
                                FROM
                                    [sys_athena_task_record_detail_failed]
                                WHERE
                                    task_detail_id = @task_detail_id";

                DynamicParameters paras = new DynamicParameters();
                paras.Add("task_detail_id", taskDetailId);
                var result = conn.Query<SysAthenaTaskRecordFailedEntity>(sql, paras);
                return result;
            }
        }
        public bool InsertAthenaTaskRecord(IEnumerable<SysAthenaTaskRecordEntity> instance)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"INSERT INTO [ec3api].[sys_athena_task_record]
                                   ([task_id],
                                    [target],
                                    [partition_by],
                                    [log_scope_by],
                                    [order_by],
                                    [upload_count],
                                    [msg],
                                    [log_start_time],
                                    [log_end_time],
                                    [is_completed],
                                    [status],
                                    [task_start_time],
                                    [task_end_time])
                             VALUES
                                   (@task_id,
                                    @target,
                                    @partition_by,
                                    @log_scope_by,
                                    @order_by,
                                    @upload_count,
                                    @msg,
                                    @log_start_time,
                                    @log_end_time,
                                    @is_completed,
                                    @status,
                                    @task_start_time,
                                    @task_end_time)";

                var result = conn.Execute(sql, instance) > 0;
                return result;
            }
        }
        public bool UpdateAthenaTaskRecord(SysAthenaTaskRecordEntity instance)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"Update sys_athena_task_record
                               Set  upload_count = @upload_count,
                                    failed_count = @failed_count,
                                    msg = @msg,
                                    status = @status,
                                    is_completed = @is_completed,
                                    task_start_time = @task_start_time,
                                    task_end_time = @task_end_time,
                                    update_time = @update_time 
                               Where task_id = @task_id
                            ";

                DynamicParameters paras = new DynamicParameters(instance);

                var result = conn.Execute(sql, paras) > 0;
                return result;
            }
        }
        public bool InsertAthenaTaskRecordDetail(IEnumerable<SysAtheneTaskDetailRecordEntity> instance)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"INSERT INTO [ec3api].[sys_athena_task_detail_record]
                                       ([task_detail_id]
                                       ,[task_id]
                                       ,[upload_count]
                                       ,[failed_count]
                                       ,[msg]
                                       ,[task_start_time]
                                       ,[task_end_time]
                                       ,[page]
                                       ,[page_per_count]
                                       ,[status]
                                       ,[is_completed]
                                       ,[update_time])
                                 VALUES
                                       (@task_detail_id
                                       ,@task_id
                                       ,@upload_count
                                       ,@failed_count
                                       ,@msg
                                       ,@task_start_time
                                       ,@task_end_time
                                       ,@page
                                       ,@page_per_count
                                       ,@status
                                       ,@is_completed
                                       ,@update_time) ";

                var result = conn.Execute(sql, instance) > 0;
                return result;
            }
        }
        public bool InsertAthenaTaskRecordFailed(IEnumerable<SysAthenaTaskRecordFailedEntity> instance)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"INSERT INTO [ec3api].[sys_athena_task_record_detail_failed]
                                       ([task_detail_id]
                                       ,[prefix]
		                               ,[total]
                                       ,[content])
                                 VALUES
                                       (@task_detail_id,
                                       @prefix,
                                       @total,
		                               @content)";

                var result = conn.Execute(sql, instance) > 0;
                return result;
            }
        }
        public bool DeleteAthenaTaskRecordFailedByTaskDetailId(Guid taskDetailId)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"Delete [ec3api].[sys_athena_task_record_detail_failed]
                               Where task_detail_id = @task_detail_id";

                DynamicParameters paras = new DynamicParameters();
                paras.Add("@task_detail_id", taskDetailId);

                var result = conn.Execute(sql, paras) > 0;
                return result;
            }
        }
        public bool UpdateAthenaTaskRecordDetail(SysAtheneTaskDetailRecordEntity instance)
        {
            using (var conn = _dbConnectionHelper.CreateDbConnection("EC3DB"))
            {
                string sql = @"UPDATE [ec3api].[sys_athena_task_detail_record]
                               SET upload_count = @upload_count
                                  ,failed_count = @failed_count
                                  ,msg = @msg
                                  ,task_start_time = @task_start_time
                                  ,task_end_time = @task_end_time
                                  ,page = @page
                                  ,page_per_count =@page_per_count
                                  ,status = @status
                                  ,is_completed =@is_completed
                                  ,update_time = @update_time
                             WHERE task_detail_id = @task_detail_id
                                                        ";

                DynamicParameters paras = new DynamicParameters(instance);

                var result = conn.Execute(sql, paras) > 0;
                return result;
            }
        }
    }
}
