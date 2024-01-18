using AthenaLog.Athena.Domain.Base;
using AthenaLog.Athena.Domain.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Interface
{
    /// <summary>
    /// 主要用途為將來源資料處理成符合Athena認得的Json格式上傳至S3, 當Athena建立對應table並指向該路徑, 即可查詢
    /// </summary>
    public interface IAthenaFactory
    {
        /// <summary>
        /// 設定目前應執行任務, 目前主要藉由sys_athena_task
        /// </summary>
        Dictionary<string, List<BaseAthenaTask>> CreateTasks();
    }
}
