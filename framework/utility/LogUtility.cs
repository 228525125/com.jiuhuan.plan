using System;
using System.Collections.Generic;
using System.Text;
using com.jiuhuan.plan.domain;
using QFramework;

namespace com.jiuhuan.plan.tools
{
    public interface ILogUtility : IUtility
    {
        /// <summary>
        /// 设置服务器IP地址
        /// </summary>
        /// <param name="ip">服务器IP</param>
        void SetServerIp(string ip);

        /// <summary>
        /// 本地记录日志到文件
        /// </summary>
        /// <param name="content">日志内容</param>
        void Log(string content);

        /// <summary>
        /// 异步上传日志队列中的日志到服务器
        /// </summary>
        void UploadLog();

        /// <summary>
        /// 获取指定日志文件的内容
        /// </summary>
        /// <param name="fileName">日志文件名（不包含扩展名）</param>
        /// <returns>日志文件内容字符串</returns>
        string GetLog(string fileName);

        Exception GetException();
    }

    public class LogUtility : ILogUtility
    {
        private readonly Queue<Log> Logs = new Queue<Log>();
        private Exception exception = null;
        private string serverIp = string.Empty;

        public void Log(string content)
        {
            string datetime = DateTime.Now.ToString("yyyy-MM-dd");
            var text = "当前时间：" + datetime + "\r\n";
            text += content;
            text += "\r\n";
            text += "--------------------------------\r\n";

            var log_dir = Utils.GetAppDirectory() + "/log";
            var file_path = datetime + ".txt";
            
            if (FileHelpler.isExists(log_dir, file_path))
            {
                FileHelpler.append(log_dir, file_path, text, Encoding.UTF8);
            }
            else
            {
                FileHelpler.create(log_dir, file_path, text, Encoding.UTF8);
            }

            if (isUploadLog()) appendUploadLog(content);
        }

        public async void UploadLog()
        {
            exception = null;
            while (Logs.Count > 0)
            {
                var log = Logs.Peek();
                try
                {
                    LogService.serverIp = string.IsNullOrEmpty(serverIp) ? LogService.Remote_IP : serverIp;
                    var response = await LogService.Save(log);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Logs.Dequeue();
                    }
                }
                catch (Exception e)
                {
                    exception = e;
                    return;
                }
            }
        }

        public string GetLog(string fileName)
        {
            string text = "";
            var log_dir = Utils.GetAppDirectory() + "/log";
            var file_path = fileName + ".txt";
            text = FileHelpler.reader(log_dir, file_path);
            return text;
        }

        // 设置服务器 IP
        public void SetServerIp(string ip)
        {
            serverIp = ip;
        }

        private bool isUploadLog()
        {
            return bool.Parse(ConfigHelper.GetConfigKey(ConfigHelper.log_upload_cloud));
        }

        private void appendUploadLog(string content)
        {
            var log = new Log
            {
                level = "debug",
                owner = Utils.GetMachineName(),
                ip = Utils.GetLocalIPAddress()?.ToString(),
                content = content.Replace("\r\n", "<br>"),
                date = DateTime.Now
            };
            Logs.Enqueue(log);
        }

        public Exception GetException()
        {
            return exception;
        }
    }
}