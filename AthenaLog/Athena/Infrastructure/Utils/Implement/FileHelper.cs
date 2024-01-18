using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AthenaLog.Athena.Infrastructure.Utils.Interface;

namespace AthenaLog.Athena.Infrastructure.Utils.Implement
{
    public class FileHelper : IFileHelper
    {
        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            //if (Directory.Exists(path))
            //Directory.Delete(path, true);
        }

        public bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
