
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
    public class AthenaRetryTaskDetail : BaseAthenaTaskDetail
    {
        public AthenaRetryTaskDetail(SysAtheneTaskDetailRecordEntity detailRecordEntity,
            IAthenaRepository athenaRepository,
            IAWSHelper awsHelper
        )
        {
            _athenaRepository = athenaRepository;
            _awsHelper = awsHelper;
            _detailRecordEntity = detailRecordEntity;
        }
        public override void Run()
        {
            base.Run();
        }
        public override void Begin()
        {
            historyList.data = JsonConvert.DeserializeObject<List<AthenaTaskHistoryDto>>(_detailRecordEntity.msg);
            historyList.Add("重新執行");
            base.Begin();
        }
        public override void Composition()
        {
            var logs = _athenaRepository.GetRecordFailedByTaskDetailId(_detailRecordEntity.task_detail_id).ToList();

            for (int i = 0; i < logs.Count; i++)
            {
                if (logDic.ContainsKey(logs[i].prefix))
                {
                    logDic[logs[i].prefix].dataList.Add(logs[i].content);
                    logDic[logs[i].prefix].total += logs[i].total;
                }
                else
                {
                    List<object> dataList = new List<object>() { logs[i].content };
                    AthenaTaskDetailLogDto detailLogDto = new AthenaTaskDetailLogDto();
                    detailLogDto.prefix = logs[i].prefix;
                    detailLogDto.total = logs[i].total;
                    detailLogDto.dataList = dataList;

                    logDic.Add(logs[i].prefix, detailLogDto);
                }

                //標記為應釋放記憶體
                logs[i] = null;
            }
        }
        public override void SendToS3()
        {
            base.SendToS3();
        }
        public override void Record()
        {
            _detailRecordEntity.upload_count = _detailRecordEntity.upload_count +
                        logDic.Where(o => o.Value.result.isSuccess).Sum(o => o.Value.total);

            _detailRecordEntity.failed_count = (logDic.Any())
                ? logDic.Where(o => !o.Value.result.isSuccess).Sum(o => o.Value.total)
                : _detailRecordEntity.failed_count;

            base.Record();
        }
    }
}
