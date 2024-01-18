using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Utils.Enum
{
    public enum S3DataType
    {
        /// <summary>
        /// 報表
        /// </summary>
        Report = 0,

        /// <summary>
        /// 備份檔
        /// </summary>
        Bak = 1,

        /// <summary>
        /// 各模組與週邊的檔案
        /// </summary>
        Data = 2,

        /// <summary>
        /// 圖片
        /// </summary>
        Image = 3,

    }

    public enum Enviroment
    {
        SIT,
        UAT,
        PRD
    }
}
