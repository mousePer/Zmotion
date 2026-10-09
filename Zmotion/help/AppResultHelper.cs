using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zmotion.help
{
    public class AppResultHelper<T>
    {
        public bool isSuccessful { get; set; }

        public string message { get; set; }

        public T data { get; set; }
        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="appResultHelper"></param>
        /// <param name="data"></param>
        public static AppResultHelper<T> Success( T data)
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = true,
                message = "成功",
                data = data
            };
        }


        /// <summary>
        /// 成功
        /// </summary>
        /// <returns></returns>
        public static AppResultHelper<T> Success()
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = true,
                message = "成功"
            };
        }
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="appResultHelper"></param>
        /// <param name="message"></param>
        public static AppResultHelper<T> Fail(string msg)
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = false,
                message = msg
            };
        }

        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="appResultHelper"></param>
        /// <param name="message"></param>
        public static AppResultHelper<T> Fail(string msg,T data)
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = false,
                message = msg,
                data = data
            };
        }
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="errorCode">状态码</param>
        /// <returns></returns>
        public static AppResultHelper<T> Fail(int errorCode)
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = false,
                message = ErrorCodeHelper.ErrorCodeDic[errorCode]+"。错误码:"+ errorCode
            };
        }
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="errorCode">状态码</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static AppResultHelper<T> Fail(int errorCode,T data)
        {
            return new AppResultHelper<T>()
            {
                isSuccessful = false,
                message = ErrorCodeHelper.ErrorCodeDic[errorCode]+"。错误码:" + errorCode,
                data = data
            };
        }
        /// <summary>   
        /// 结果验证
        /// </summary>
        /// <param name="result">结果码 </param>
        /// <returns></returns>
        public static AppResultHelper<T> ResultValidation(int result)
        {
            if (result != 0)
            {
                return Fail(result);
            }
            return Success();
        }
        /// <summary>   
        /// 结果验证
        /// </summary>
        /// <param name="result">结果码 </param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static AppResultHelper<T> ResultValidation(int result, T data)
        {
            if (result != 0)
            {
                return Fail(result,data);
            }
            return Success(data);
        }
        /// <summary>   
        /// 结果验证
        /// </summary>
        /// <param name="result">结果码 </param>
        /// <returns></returns>
        public static AppResultHelper<T> ResultValidation(List<int> resultList)
        {
            foreach(var result in resultList)
            {
                if (result != 0)
                {
                    return Fail(result);
                }
            }
            return Success();
        }
        /// <summary>   
        /// 结果验证
        /// </summary>
        /// <param name="result">结果码 </param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static AppResultHelper<T> ResultValidation(List<int> resultList, T data)
        {
            foreach (var result in resultList)
            {
                if (result != 0)
                {
                    return Fail(result, data);
                }
            }
            return Success(data);
        }
    }
}
