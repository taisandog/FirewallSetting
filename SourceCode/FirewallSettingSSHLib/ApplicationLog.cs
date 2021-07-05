using Buffalo.Kernel;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib
{
    public class ApplicationLog
    {
        public readonly static string BaseRoot = GetBaseRoot();
        private static Encoding _defaultEncoding = Encoding.UTF8;
        /// <summary>
        /// 获取基础路径
        /// </summary>
        /// <returns></returns>
        private static string GetBaseRoot()
        {
            string ret = AppSetting.Default["App.Log"];
            if (string.IsNullOrWhiteSpace(ret)) 
            {
                ret = CommonMethods.GetBaseRoot("log");
            }
            if (!Directory.Exists(ret))
            {
                Directory.CreateDirectory(ret);
            }
            return ret;
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="cmd"></param>
        public static void LogCmdError(SshCommand cmd) 
        {
            if (!string.IsNullOrWhiteSpace(cmd.Error))
            {
                LogError(cmd.Error);
            }
        }
        

        private static object _autoLock = false;
        /// <summary>
        /// 记录自动日志日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogAuto(string message)
        {
            
            string fileName = Path.Combine(BaseRoot, "auto" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            lock (_autoLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(message);
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }
        }
        private static object _messLock = false;
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogMessage(string message)
        {
            
            string name = "log." + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string fileName = Path.Combine(BaseRoot, name);
            lock (_messLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(message);
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }

        }

        private static object _warningLock = false;
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            
            string name = "warning." + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string fileName = Path.Combine(BaseRoot, name);
            lock (_warningLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(message);
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }

        }


        private static object _errorLock = false;
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogError(string message)
        {
           
            string name = "error." + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string fileName = Path.Combine(BaseRoot, name);
            lock (_errorLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(message);
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }

        }
        private static object _exceptionLock = false;
        /// <summary>
        /// 记录自动日志日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogException(string modelname, Exception ex)
        {
            string name = "exception." + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            
            string fileName = Path.Combine(BaseRoot, name);
            lock (_exceptionLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====model:" + modelname + ",date:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }
        }


        private static object _debugLock = false;
        /// <summary>
        /// 记录测试日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogDebug(string message)
        {
            string name = "debug." + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            string fileName = Path.Combine(BaseRoot, name);
            lock (_debugLock)
            {
                using (StreamWriter sw = new StreamWriter(fileName, true, _defaultEncoding))
                {
                    sw.WriteLine("====" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=====");
                    sw.WriteLine(message);
                    sw.WriteLine("===========");
                    sw.WriteLine("");
                    sw.WriteLine("");
                    sw.WriteLine("");
                }
            }
        }
    }
}
