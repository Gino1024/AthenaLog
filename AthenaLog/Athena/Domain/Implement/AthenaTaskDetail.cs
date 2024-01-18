
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaTaskDetail : BaseAthenaTaskDetail
    {
        protected AthenaTaskQueryDto _athenaTaskQueryDto;
        public AthenaTaskDetail()
        {

        }
        public AthenaTaskDetail(Guid taskGuid,
            IAthenaRepository athenaRepository, 
            AthenaTaskQueryDto athenaTaskQueryDto,
            IAWSHelper awsHelper
            )
        {
            _athenaRepository = athenaRepository;
            _awsHelper = awsHelper;
            _athenaTaskQueryDto = athenaTaskQueryDto;
            _detailRecordEntity.task_detail_id = Guid.NewGuid();
            _detailRecordEntity.task_id = taskGuid;
            _detailRecordEntity.page = _athenaTaskQueryDto.page.Value;
            _detailRecordEntity.page_per_count = _athenaTaskQueryDto.perPageCount.Value;
        }
        public override void Run()
        {
            base.Run();
        }
        public override void Begin()
        {
            historyList.Add("執行開始");
            base.Begin();
        }
        public override void Composition()
        {

            //GetSourceData
            var tasksDetailList = _athenaRepository.GetAthenaTaskDetailByQuery(_athenaTaskQueryDto).ToList();
            
            //Organize
            for (int i = 0; i < tasksDetailList.Count; i++)
            {
                var partitionByArr = _athenaTaskQueryDto.partitionBy.Split(',');
                var properties = tasksDetailList[i] as IDictionary<string, object>;
                //var log = JsonConvert.SerializeObject(tasksDetailList[i]);
                List<string> partitionValue = new List<string>();
                
                //標記為應釋放記憶體
                //tasksDetailList[i] = null;

                foreach (var partColumn in partitionByArr)
                {
                    if (properties.TryGetValue(partColumn, out var tempValue))
                    {
                        if (tempValue == null)
                            throw new Exception($"{_athenaTaskQueryDto.tableName} partitionBy對應欄位不應有Null值");

                        if (tempValue is DateTime tempDateTime)
                        {
                            string year = tempDateTime.Year.ToString().PadLeft(4, '0');
                            string month = tempDateTime.Month.ToString().PadLeft(2, '0');
                            string day = tempDateTime.Day.ToString().PadLeft(2, '0');

                            partitionValue.Add($"year={year}/month={month}/day={day}");
                        }
                        else
                        {
                            partitionValue.Add($"{partColumn}={tempValue}");
                        }
                    }
                }

                string fileName = $"{_detailRecordEntity.task_detail_id}.json";
                var prefix = $"Athena/{_athenaTaskQueryDto.tableName}/{string.Join("/", partitionValue)}/{fileName}.json";

                if (logDic.ContainsKey(prefix))
                {
                    logDic[prefix].dataList.Add(tasksDetailList[i]);
                    logDic[prefix].total += 1;
                }
                else
                {
                    List<object> dataList = new List<object>() { tasksDetailList[i] };
                    AthenaTaskDetailLogDto detailLogDto = new AthenaTaskDetailLogDto();
                    detailLogDto.dataList = dataList;
                    detailLogDto.prefix = prefix;
                    detailLogDto.total = 1;
                    logDic.Add(prefix, detailLogDto);
                }
            }
        }
        public override void SendToS3()
        {
            base.SendToS3();
        }
        public override void Record()
        {
            base.Record();
        }
    }
}
