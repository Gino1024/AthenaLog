using AthenaLog.ApplicationService.Interface;
using AthenaLog.Athena.Domain.Implement;
using AthenaLog.Athena.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.ApplicationService.Implement
{
    public class AthenaLogService : IAthenaLogService
    {
        private IAthenaFactory _athenaFectory;
        public AthenaLogService(IAthenaFactory athenaFectory)
        {
            _athenaFectory = athenaFectory;
        }
        public void AthenaJobRunner()
        {
            try
            {
                var tasks = _athenaFectory.CreateTasks();

                foreach (var task in tasks)
                {
                    foreach (var item in task.Value)
                    {
                        item.Run();
                    }
                }
                //平行處理每個target
                //Parallel.ForEach(tasks, new ParallelOptions() { MaxDegreeOfParallelism = 5 }, (task) =>
                //{
                //    //目前設計會先處理target 下的 retry任務, 再處理例行任務
                //    foreach (var item in task.Value)
                //    {
                //        item.Run();
                //    }
                //});

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
