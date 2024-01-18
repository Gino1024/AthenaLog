using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Dto
{
    public class AthenaTaskDetailLogDto
    {
        public string prefix { get; set; }
        public ResultDto result { get; set; } = new ResultDto();
        public List<object> dataList { get; set; } = new List<object>();
        public int total { get; set; }
        public string FormatMessage()
        {
            if (!dataList.Any())
                return "本次無需要上傳資料";

            string msg = string.Format("prefix:{0}, 上傳:{1}, 筆數:{2}{3}"
                , prefix
                , result.isSuccess ? "成功" : "失敗"
                , dataList.Count()
                , (result.isSuccess) ? "": $", 原因: {result.msg}"); 

            return msg;
        }
    }
}
