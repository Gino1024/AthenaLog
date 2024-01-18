using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Infrastructure
{
    public static class Hardcode
    {
        public static string tempFolderPath => Path.Combine(
                                    AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                    "temp"
                                    );
    }
}
