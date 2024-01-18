
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Interface;
using AthenaLog.Athena.Infrastructure.Repository.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaTask : BaseAthenaTask
    {
        protected readonly IAthenaTaskValidator _athenaTaskValidator;
        public AthenaTask()
        {

        }
        public AthenaTask(Guid guid,
            SysAthenaTaskEntity athenaTasks, 
            SysAthenaTaskRecordEntity lastSysAthenaTaskRecordEntity,
            IEnumerable<TableSchemaEntity> targetTableSchema,
            IAthenaRepository athenaRepository, 
            IAWSHelper awsHelper,
            IAthenaTaskValidator athenaTaskValidator)
        {
            _sysAthenaTaskEntity = athenaTasks;
            _lastSysAthenaTaskRecordEntity = lastSysAthenaTaskRecordEntity;
            _targetTableSchema = targetTableSchema;
            _athenaRepository = athenaRepository;
            _athenaTaskValidator = athenaTaskValidator;
            _awsHelper = awsHelper;

            _taskRecordEntity = new SysAthenaTaskRecordEntity(guid, _sysAthenaTaskEntity);
            _taskRecordEntity.log_start_time = (_lastSysAthenaTaskRecordEntity == null) 
                                                ? Convert.ToDateTime("1911/1/1 00:00:00") 
                                                : lastSysAthenaTaskRecordEntity.log_end_time;         
        }
        /// <summary>
        /// 主要任務資訊
        /// </summary>
        public SysAthenaTaskEntity _sysAthenaTaskEntity { get; set; }
        /// <summary>
        /// 最後一筆執行過的紀錄
        /// </summary>
        public SysAthenaTaskRecordEntity _lastSysAthenaTaskRecordEntity { get; set; }
        /// <summary>
        /// TargetTable欄位描述
        /// </summary>
        public IEnumerable<TableSchemaEntity> _targetTableSchema { get; set; }
        public override void Run()
        {
            base.Run();
        }
        public override void Begin()
        {
            historyList.Add("執行開始");
            base.Begin();
        }
        /// <summary>
        /// 驗證任務資料是否合法
        /// </summary>
        public override void Verify()
        {
            verify = _athenaTaskValidator.Verify(this);
            historyList.Add($"{verify.msg}");
        }
        /// <summary>
        /// 設定詳細任務資訊, 每筆詳細任務僅執行處理設定筆數資料, 避免記憶體占用過多
        /// </summary>
        public override void Composition()
        {
            if (!verify.isSuccess)
                return;

            string perPageCountByConfig = ConfigurationManager.AppSettings.Get("perPageCount");
            if(!int.TryParse(perPageCountByConfig, out int perPageCount))
                perPageCount = 500;

            AthenaTaskQueryDto queryDto = new AthenaTaskQueryDto(_taskRecordEntity);

            var detailDataCount = _athenaRepository.GetCurrentTaskDataCountByQuery(queryDto);
            var page = Math.Ceiling((double)detailDataCount / perPageCount);
            for (int i = 1; i <= page; i++)
            {
                AthenaTaskQueryDto detailQueryDto = queryDto.DeepCopy();
                detailQueryDto.page = i;
                detailQueryDto.perPageCount = perPageCount;

                AthenaTaskDetail athenaTaskDetail = new AthenaTaskDetail(_taskRecordEntity.task_id, _athenaRepository, detailQueryDto, _awsHelper);
                detailTasks.Add(athenaTaskDetail);
            }
            var details = detailTasks.Select(m => m._detailRecordEntity);
            var isInsert = _athenaRepository.InsertAthenaTaskRecordDetail(details);
        }
        public override void Excuting()
        {
            base.Excuting();
        }
        public override void Record()
        {
            _taskRecordEntity.upload_count = detailTasks
                                        .Sum(m => m.logDic.Where(o => o.Value.result.isSuccess).Sum(o => o.Value.total));
            _taskRecordEntity.failed_count = detailTasks
                                   .Sum(m => m.logDic.Where(o => !o.Value.result.isSuccess).Sum(o => o.Value.total));

            base.Record();
        }
    }
}
