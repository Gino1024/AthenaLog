using AthenaLog.Athena.Infrastructure.Utils.Dto;
using AthenaLog.Athena.Infrastructure.Utils.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Utils.Interface
{
    public interface IAWSHelper
    {
        ResultHelper<string> SendFileToS3(byte[] fileMs, string s3Path, S3DataType s3DataType, bool isAbsolutePath);
    }
}
