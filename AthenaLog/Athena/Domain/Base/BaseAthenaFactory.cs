using AthenaLog.Athena.Domain.Implement;
using AthenaLog.Athena.Domain.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaLog.Athena.Domain.Base
{
    public abstract class BaseAthenaFactory
    {
        public Dictionary<string, List<BaseAthenaTask>> athenaTasks = new Dictionary<string, List<BaseAthenaTask>>();
        public abstract Dictionary<string, List<BaseAthenaTask>> CreateTasks();
    }
}
