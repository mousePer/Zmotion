using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogProject
{
    [Description("日志服务")]
    ///<summary>
    /// 日志服务   (将日志请求放入队列，后台线程处理)
    /// </summary>
    public class LogServer
    {
        //定义一个队列 ，存放打印日志请求
        ConcurrentQueue<LogInfo> logQueue = new ConcurrentQueue<LogInfo>();

        public string path = @"D:\Log\";

        /// <summary>
        /// 添加日志（拼接文件路劲，并判断是否存在，不存在则创建文件）
        /// </summary>
        /// <param name="message">日志内容</param>
        public void WriteLog(string message)
        {
            string fileName = $" {path}{DateTime.Now.ToString("yyyy-MM")}\\{DateTime.Now.ToString("yyyy-MM-dd")}_LogInfo.txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(fileName.Substring(0, fileName.LastIndexOf("\\"))))
            {
                Directory.CreateDirectory(fileName.Substring(0, fileName.LastIndexOf("\\")));
            }
            //拼接日志信息
            string logInfo = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}==========> {message}";
            //将拼接好的日志信息写入日志文件
            WriteLog(fileName, logInfo);

        }
        /// <summary>
        /// 将日志信息写入日志文件
        /// </summary>
        /// <param name="fileName">日志文件路径</param>
        /// <param name="logInfo">日志内容</param>
        private void WriteLog(string fileName, string logInfo)
        {
            lock (this)
            {
                //创建日志信息对象
                LogInfo log = new LogInfo()
                {
                    Path = fileName,
                    Message = logInfo
                };
                //将日志信息加入队列
                logQueue.Enqueue(log);
            }
        }

        //使用单例模式，保证日志服务器只有一个实例
        public static LogServer Instance { get; private set; } = new LogServer();
        //构造函数私有化,防止外部实例化，更改已存在实例
        private LogServer()
        {
            //创建一个后台线程，处理日志请求
            Task.Run(() => Dowork());
        }
        /// <summary>
        /// 处理日志请求
        /// </summary>
        private void Dowork()
        {
            //循环处理队列中的日志请求
            while (true)
            {
                //失败次数
                int failCount = 0;
                if (logQueue.Count <= 0)
                {
                    continue;
                }
                //获取队列中的第一个日志请求
                logQueue.TryDequeue(out LogInfo logInfo);
                LogStart:try
                {
                    using (StreamWriter sw = new StreamWriter(logInfo.Path, true))
                    {
                        sw.WriteLine(logInfo.Message);   // 写一行日志，自动加换行
                        sw.Flush();                       // 把缓冲区里的内容立即刷到磁盘，防止断电/崩溃丢日志
                        // 关闭流  在using中默认会调用Dispose方法
                        //sw.Close();
                    }
                }
                catch (Exception e)
                {
                    failCount++;
                    if (failCount <=30)
                    {
                        goto LogStart;
                    }
                    else
                    {
                        //报警
                        //三色灯闪烁
                        //蜂鸣器响
                        //邮件发送
                    }
                }

            }
        }



    }
}
