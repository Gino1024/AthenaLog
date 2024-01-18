using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure.Utils.Interface
{
    public interface IFileHelper
    {
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        bool ByteArrayToFile(string fileName, byte[] byteArray);
    }
}
