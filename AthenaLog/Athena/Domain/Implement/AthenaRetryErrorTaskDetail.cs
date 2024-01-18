
using Newtonsoft.Json;
using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Dto;
using AthenaLog.Athena.Infrastructure.Repository.Entity;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AthenaLog.Athena.Domain.Implement
{
    public class AthenaRetryErrorTaskDetail : AthenaTaskDetail
    {
        protected SysAthenaTaskRecordEntity _sysAthenaTaskRecordEntity;
        public AthenaRetryErrorTaskDetail(SysAthenaTaskRecordEntity sysAthenaTaskRecordEntity,
            SysAtheneTaskDetailRecordEntity detailRecordEntity,
            bool checkS3Exists,
            IAthenaRepository athenaRepository,
            IAWSHelper awsHelper
        ) :base()
        {
            _sysAthenaTaskRecordEntity = sysAthenaTaskRecordEntity;
            _athenaRepository = athenaRepository;
            _awsHelper = awsHelper;
            _detailRecordEntity = detailRecordEntity;
            _athenaTaskQueryDto = new AthenaTaskQueryDto(_sysAthenaTaskRecordEntity, _detailRecordEntity);

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
            base.Composition();
            
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
