using AthenaLog.ApplicationService.Interface;
using AthenaLog.Athena.Domain.Implement;
using AthenaLog.Athena.Domain.Interface;
using AthenaLog.Athena.Infrastructure.Repository.Implement;
using AthenaLog.Athena.Infrastructure.Repository.Interface;
using AthenaLog.Athena.Infrastructure.Utils.Implement;
using AthenaLog.Athena.Infrastructure.Utils.Interface;
using AthenaLog.Common.DBConnectionHelper;
using AthenaLog.ApplicationService.Implement;

namespace AthenaLog
{
    class Program
    {
        static void Main(string[] args)
        {
            IDBConnectionHelper dbConnectionHelper = new DBConnectionHelper();
            IAthenaRepository athenaRepository = new AthenaRepository(dbConnectionHelper);
            IFileHelper fileHelper = new FileHelper();
            IAWSHelper awsHelper = new AWSHelper(fileHelper);
            IAthenaTaskValidator athenaTaskValidator = new AthenaTaskValidator();
            IAthenaFactory athenaFectory = new AthenaFactory(athenaRepository, awsHelper, athenaTaskValidator);
            IAthenaLogService AthenaLogService = new AthenaLogService(athenaFectory);

            AthenaLogService.AthenaJobRunner();
        }
    }
}
