
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Domain.Implement;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Base
{
    public abstract class BaseAthenaTask
    {
        protected IAthenaRepository _athenaRepository;
        protected IAWSHelper _awsHelper;
        public SysAthenaTaskRecordEntity _taskRecordEntity { get; set; }
        /// <summary>
        /// 驗證資訊
        /// </summary>
        public ResultDto verify { get; set; } = new ResultDto();
        public AthenaTaskHistoryListDto historyList = new AthenaTaskHistoryListDto();
        /// <summary>
        /// 批次作業
        /// </summary>
        public List<BaseAthenaTaskDetail> detailTasks { get; set; } = new List<BaseAthenaTaskDetail>();
        public virtual void Run() 
        {
            try
            {
                Begin();
                Verify();
                Composition();
                Excuting();
            }
            catch (Exception ex)
            {
                _taskRecordEntity.status = 2;
                historyList.Add($"{this.GetType().Name}.{MethodBase.GetCurrentMethod().Name} 發生錯誤: {ex}");
            }

            try
            {
                Record();
            }
            catch (Exception ex2)
            {
                historyList.Add($"{this.GetType().Name}.{MethodBase.GetCurrentMethod().Name} 發生錯誤: {ex2}");
            }
        }
        public virtual void Begin()
        {
            _taskRecordEntity.msg = JsonConvert.SerializeObject(historyList.data);
            _taskRecordEntity.status = 0;
            _taskRecordEntity.task_start_time = DateTime.Now;
            _taskRecordEntity.update_time = DateTime.Now;
            _athenaRepository.UpdateAthenaTaskRecord(_taskRecordEntity);
        }
        public abstract void Verify();
        public abstract void Composition();
        public virtual void Excuting()
        {
            if (!verify.isSuccess)
            {
                _taskRecordEntity.status = 3;
                return;
            }

            foreach (var detailTask in detailTasks)
            {
                detailTask.Run();
                detailTask.Dispose();
            }

            _taskRecordEntity.status = 1;
            historyList.Add("執行完成");
        }
        public virtual void Record() {
            _taskRecordEntity.msg = JsonConvert.SerializeObject(historyList.data);
            _taskRecordEntity.is_completed = 
                (
                   _taskRecordEntity.status == 1 
                   && !detailTasks.Where(m=>m._detailRecordEntity.is_completed == 0
                ).Any()) ? 1 : 0;
            _taskRecordEntity.task_end_time = DateTime.Now;
            _taskRecordEntity.update_time = DateTime.Now;

            var isUpdate = _athenaRepository.UpdateAthenaTaskRecord(_taskRecordEntity);
        }
    }
}
