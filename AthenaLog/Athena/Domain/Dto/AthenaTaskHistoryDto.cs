using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Dto
{
    public class AthenaTaskHistoryListDto
    {
        public List<AthenaTaskHistoryDto> data = new List<AthenaTaskHistoryDto>();
        public void Add(string msg)
        {
            AthenaTaskHistoryDto tempMsg = new AthenaTaskHistoryDto();
            tempMsg.create_time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            tempMsg.message = msg;
            data.Add(tempMsg);
        }
    }
    public class AthenaTaskHistoryDto
    {
        public string create_time { get; set; }
        public string message { get; set; }
    }
}
