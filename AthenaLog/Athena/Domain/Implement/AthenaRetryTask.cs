
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaRetryTask : AthenaTask
    {
        public AthenaRetryTask(SysAthenaTaskRecordEntity entity, 
            IAthenaRepository athenaRepository,
            IAWSHelper awsHelper) : base()
        {
            _athenaRepository = athenaRepository;
            _awsHelper = awsHelper;
            _taskRecordEntity = entity;
        }
        public override void Run()
        {
            base.Run();
        }

        public override void Begin()
        {
            historyList.data = JsonConvert.DeserializeObject<List<AthenaTaskHistoryDto>>(_taskRecordEntity.msg);
            historyList.Add("重新執行");
            base.Begin();
        }
        public override void Verify()
        {
            //RetryTask 不需要重新驗證
            verify.isSuccess = true;
            verify.msg = "Retry Skip Veify";
        }
        public override void Composition()
        {
            if (!verify.isSuccess)
                return;

            var detailRecordEntityList = _athenaRepository.GetAthenaDetailRecordByTaskId(_taskRecordEntity.task_id);
            //檢查是否有detail
            if (detailRecordEntityList.Any())
            {
                foreach (var detailRecordEntity in detailRecordEntityList)
                {
                    BaseAthenaTaskDetail athenaTaskDetail = null;

                    if (detailRecordEntity.status == 1)
                    {
                        //處理上傳部分失敗子任務
                        athenaTaskDetail = new AthenaRetryTaskDetail(detailRecordEntity, _athenaRepository, _awsHelper);
                        detailTasks.Add(athenaTaskDetail);
                    }
                    else if (detailRecordEntity.status == 0 || detailRecordEntity.status == 2)
                    {
                        //處理發生Exception的子任務
                        athenaTaskDetail = new AthenaRetryErrorTaskDetail(_taskRecordEntity, detailRecordEntity, false, _athenaRepository, _awsHelper); 
                        detailTasks.Add(athenaTaskDetail);
                    }

                }
            }
            else
            {
                base.Composition();
            }
        }
        public override void Excuting()
        {
            base.Excuting();
        }
        public override void Record()
        {
            _taskRecordEntity.upload_count = _taskRecordEntity.upload_count +
                                    detailTasks
                                   .Sum(m => m.logDic.Where(o => o.Value.result.isSuccess).Sum(o => o.Value.total));

            _taskRecordEntity.failed_count = detailTasks
                                   .Sum(m => m.logDic.Where(o => !o.Value.result.isSuccess).Sum(o => o.Value.total));

            base.Record();
        }
    }
}
