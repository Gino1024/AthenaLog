using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Interface;
using AthenaLog.Athena.Infrastructure;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaFactory : BaseAthenaFactory, IAthenaFactory
    {
        private readonly IAthenaRepository _athenaRepository;
        private readonly IAWSHelper _awsHelper;
        private readonly IAthenaTaskValidator _athenaTaskValidator;
        public AthenaFactory(IAthenaRepository athenaRepository,
            IAWSHelper awsHelper,
            IAthenaTaskValidator athenaTaskValidator)
        {
            _athenaTaskValidator = athenaTaskValidator;
            _athenaRepository = athenaRepository;
            _awsHelper = awsHelper;
        }
        public override Dictionary<string, List<BaseAthenaTask>> CreateTasks()
        {
            CreateRetryTasks();
            CreateNormalTasks();

            return athenaTasks;
        }
        public void CreateNormalTasks()
        {
            List<SysAthenaTaskRecordEntity> taskRecordEntity = new List<SysAthenaTaskRecordEntity>();
            var athenaTasksList = _athenaRepository.GetAthenaTaskAll();
            var targetNames = athenaTasksList.Select(m => m.target);

            var tasksLastRecords = _athenaRepository
                                    .GetAthenaTaskRecordAllLastRecordByTargetList(targetNames)
                                    .ToDictionary(m => m.target, m => m);

            var targetTableInfos = _athenaRepository
                                    .GetTargetTableInfo(targetNames)
                                    .GroupBy(e => e.table_name)
                                    .ToDictionary(m => m.Key, m => m);

            foreach (var item in athenaTasksList)
            {
                var guid = Guid.NewGuid();
                tasksLastRecords.TryGetValue(item.target, out var lastRecord);
                targetTableInfos.TryGetValue(item.target, out var tableInfo);
                var task = new AthenaTask(guid, item, lastRecord, tableInfo, _athenaRepository, _awsHelper, _athenaTaskValidator);
                taskRecordEntity.Add(task._taskRecordEntity);
                AddAthenaTasks(item.target, task);
            }

            var isInsert = _athenaRepository.InsertAthenaTaskRecord(taskRecordEntity);
        }
        public void CreateRetryTasks()
        {
            List<SysAthenaTaskRecordEntity> taskRecordEntity = new List<SysAthenaTaskRecordEntity>();
            //取得Record 上傳未完成(is_completed = 0) 且執行過或執行失敗的紀錄(status in (1,2))
            var athenaRetryTasks = _athenaRepository.GetAthenaTaskRecordNotCompleted();

            foreach (var item in athenaRetryTasks)
            {
                var task = new AthenaRetryTask(item, _athenaRepository, _awsHelper);
                taskRecordEntity.Add(task._taskRecordEntity);
                AddAthenaTasks(item.target, task);
            }
        }
        private void AddAthenaTasks(string target, BaseAthenaTask task)
        {
            if (athenaTasks.ContainsKey(target))
            {
                athenaTasks[target].Add(task);
            }
            else
            {
                List<BaseAthenaTask> taskList = new List<BaseAthenaTask>();
                taskList.Add(task);
                athenaTasks.Add(target, taskList);
            }
        }
    }
}
