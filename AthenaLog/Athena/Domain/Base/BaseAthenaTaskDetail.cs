
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AthenaLog.Athena.Infrastructure.Utils.Enum;

namespace AthenaLog.Athena.Domain.Base
{
    public abstract class BaseAthenaTaskDetail : IDisposable
    {
        protected Object historyListLock = new Object();
        protected IAWSHelper _awsHelper;
        protected IAthenaRepository _athenaRepository;
        public Dictionary<string, AthenaTaskDetailLogDto> logDic = new Dictionary<string, AthenaTaskDetailLogDto>();
        public SysAtheneTaskDetailRecordEntity _detailRecordEntity = new SysAtheneTaskDetailRecordEntity();
        public List<SysAthenaTaskRecordFailedEntity> _recordFailedEntityList = new List<SysAthenaTaskRecordFailedEntity>();
        public AthenaTaskHistoryListDto historyList = new AthenaTaskHistoryListDto();

        private bool disposedValue;

        public virtual void Run() 
        {
            try
            {
                Begin();
                Composition();
                SendToS3();
            }
            catch (Exception ex)
            {
                _detailRecordEntity.status = 2;
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
            _detailRecordEntity.msg = JsonConvert.SerializeObject(historyList.data);
            _detailRecordEntity.status = 0;
            _detailRecordEntity.task_start_time = DateTime.Now;
            _detailRecordEntity.update_time = DateTime.Now;
            var isUpdate = _athenaRepository.UpdateAthenaTaskRecordDetail(_detailRecordEntity);
        }
        public abstract void Composition();
        public virtual void SendToS3()
        {
            Parallel.ForEach(logDic, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (log) =>
            {
                string s3Path = log.Key;
                string value = string.Join(Environment.NewLine, log.Value.dataList.Select(m=>JsonConvert.SerializeObject(m)));
                byte[] file =  Encoding.UTF8.GetBytes(value);

                var isUpload = _awsHelper.SendFileToS3(file, s3Path,S3DataType.Data, false);

                log.Value.result.isSuccess = isUpload.Status;
                log.Value.result.msg = isUpload.Status ? string.Empty : isUpload.ResultMessage;
                lock (historyListLock)
                {
                    historyList.Add(log.Value.FormatMessage());
                }
            });

            _detailRecordEntity.status = 1;
            historyList.Add("執行完成");
        }
        public virtual void Record()
        {
            try
            {
                _athenaRepository.DeleteAthenaTaskRecordFailedByTaskDetailId(_detailRecordEntity.task_detail_id);
                _recordFailedEntityList = logDic.Values
                                        .Where(m => !m.result.isSuccess)
                                        .Select(m =>
                                        {
                                            var item = new SysAthenaTaskRecordFailedEntity();
                                            item.task_detail_id = _detailRecordEntity.task_detail_id;
                                            item.prefix = m.prefix;
                                            item.total = m.total;
                                            item.content = string.Join(Environment.NewLine, m.dataList);
                                            return item;
                                        }).ToList();

                _athenaRepository.InsertAthenaTaskRecordFailed(_recordFailedEntityList);
            }
            catch (Exception ex)
            {
                _detailRecordEntity.status = 2;
                historyList.Add($"{this.GetType().Name}.{MethodBase.GetCurrentMethod().Name} 發生錯誤: {ex}");
            }


            _detailRecordEntity.upload_count = logDic.Values.Where(m => m.result.isSuccess)
                                               .Sum(m => m.total);

            _detailRecordEntity.failed_count = logDic.Values.Where(m => !m.result.isSuccess)
                                                .Sum(m => m.total);

            _detailRecordEntity.is_completed = (_detailRecordEntity.status == 1 && _detailRecordEntity.failed_count == 0) ? 1 : 0;
            _detailRecordEntity.msg = JsonConvert.SerializeObject(historyList.data);
            _detailRecordEntity.task_end_time = DateTime.Now;
            _detailRecordEntity.update_time = DateTime.Now;

            _athenaRepository.UpdateAthenaTaskRecordDetail(_detailRecordEntity);

        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 處置受控狀態 (受控物件)
                }

                // TODO: 釋出非受控資源 (非受控物件) 並覆寫完成項
                // TODO: 將大型欄位設為 Null
                disposedValue = true;
            }
        }

        // // TODO: 僅有當 'Dispose(bool disposing)' 具有會釋出非受控資源的程式碼時，才覆寫完成項
        // ~BaseAthenaTaskDetail()
        // {
        //     // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 請勿變更此程式碼。請將清除程式碼放入 'Dispose(bool disposing)' 方法
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
