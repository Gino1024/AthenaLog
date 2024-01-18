using AthenaLog.Athena.Domain.Dto;
using AthenaLog.Athena.Domain.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Interface
{
    public interface IAthenaTaskValidator
    {
        ResultDto Verify(AthenaTask instance);
    }
}
