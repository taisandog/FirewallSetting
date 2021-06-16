using Buffalo.ArgCommon;
using Buffalo.Kernel;
using Renci.SshNet;
using SettingLib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace FWSettingClientCore
{
    class Program
    {
        private static List<FWUser> _curUser;
        private static ThreadEventInfo _doThreadInfo;

        static void Main(string[] args)
        {
            var ssh = new SshClient("192.168.130.198", "root", "1012"); //创建ssh连接对象

            FWUser.XmlPath = CommonMethods.GetBaseRoot("App_Data\\accont.xml");
            FWUser.IsServer = false;

            _curUser = FWUser.LoadConfig();

            if (args!=null && args.Length > 0 && string.Equals(args[0],"-o")) 
            {
                RunToOnce();
                return;
            }
            RunToRoll();
            
        }
        /// <summary>
        /// 执行一次
        /// </summary>
        private static void RunToOnce()
        {
            DoUpdateIP();
        }
        /// <summary>
        /// 循环执行
        /// </summary>
        private static void RunToRoll() 
        {
            Thread doThread = new Thread(new ThreadStart(UpdateIP));
            doThread.Start();
            _doThreadInfo = new ThreadEventInfo(doThread);
            _doThreadInfo.Running = true;
            string line = null;
            try
            {
                while (true)
                {
                    Console.WriteLine("please input command");
                    line = Console.ReadLine();
                    if (string.Equals(line, "exit"))
                    {
                        _doThreadInfo.Running = false;

                        Console.WriteLine("app is closing");
                        _doThreadInfo.Wait(10000);
                        Console.WriteLine("app is closed!");
                        break;
                    }

                }
            }
            finally
            {

                Thread.Sleep(200);
            }
        }

        private static void DoUpdateIP() 
        {
            long tick = (long)CommonMethods.ConvertDateTimeInt(DateTime.Now, true, true);

            foreach (FWUser user in _curUser)
            {
                APIResault res = user.Handle.UpdateAddress(user.UserName, tick, user.GetSign(tick));
                if (!res.IsSuccess)
                {
                    Console.WriteLine(res.Message);
                }
            }
        }

        /// <summary>
        /// 更新IP
        /// </summary>
        private static void UpdateIP()
        {
            int sleep = 5 * 60 * 1000;

            int pertime = 200;
            int runticks = sleep/pertime;

            int curCount = runticks;

            while (_doThreadInfo.Running)
            {
                try
                {
                    if (curCount >= runticks)
                    {
                        DoUpdateIP();
                        curCount = 1;
                    }
                    else
                    {
                        curCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Thread.Sleep(pertime);
                }
            }

        }
    }
}
