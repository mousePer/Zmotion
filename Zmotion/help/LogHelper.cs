using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.help
{
    public static class LogHelper
    {
        public static void WriteLog(string message)
        {
            LogProject.LogServer.Instance.WriteLog(message);
        }
    }
}
