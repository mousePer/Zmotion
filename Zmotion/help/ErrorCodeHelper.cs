using Sunny.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.help
{
    public static class ErrorCodeHelper
    {
        public static Dictionary<int, string> ErrorCodeDic;
        
        public static Dictionary<int,string> GetErrorCodeDicWithFile()
        {
            //读取Debug中的错误码文件
            ErrorCodeDic = new Dictionary<int, string>();
            string path = AppDomain.CurrentDomain.BaseDirectory + "ErrorCode.txt";
            IEnumerable<string> enumerable = File.ReadLines(path);
            List<string> ErrorCodeLineList = enumerable
                .Select(line => line.Trim())                 // 去除每一行的前后空格
                .Where(line => !string.IsNullOrEmpty(line))  // 去掉空行
                .ToList();
            int index = 0;
            //解析读取到所有行数据
            foreach (string line in ErrorCodeLineList)
            {
                //切割出Key与Value
                string CodeStr = line.Substring(0,line.IndexOf(" "));
                string message = line.Substring(line.IndexOf(" ")+1);
                //若相应的错误码已存在，则在错误信息末尾叠加新的错误信息   
                if (ErrorCodeDic.ContainsKey(int.Parse(CodeStr)))
                {
                    ErrorCodeDic.TryGetValue(int.Parse(CodeStr), out string value);
                    value = value + ",或" + message;
                    ErrorCodeDic[int.Parse(CodeStr)] = value;
                    continue;
                }
                ErrorCodeDic.Add(int.Parse(CodeStr), message);
                index++;
            }
            return ErrorCodeDic;
        }
       
    }
}
