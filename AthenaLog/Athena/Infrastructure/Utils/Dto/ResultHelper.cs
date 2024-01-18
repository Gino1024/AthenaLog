using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Utils.Dto
{
    public class ResultHelper<T> 
    {
        /// <summary>
        /// 執行狀態
        /// </summary>
        public bool Status { set; get; } = false;

        /// <summary>
        /// 回傳訊息
        /// </summary>
        public string ResultMessage { set; get; }

        public T Data { get; set; }
    }
}
