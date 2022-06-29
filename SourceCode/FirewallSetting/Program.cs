using Buffalo.Kernel;
using FWSettingClient;
using SettingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FirewallSetting
{
    static class Program
    {
        public static bool IsAuto = false;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FWUser.IsServer = true;
            FWUser.XmlPath = Path.Combine(UserManager.BasePath , "userInfo.xml");
            RegConfig.AutoRoot = "\"" + Path.Combine(CommonMethods.GetBaseRoot(), "FirewallSetting.exe")+ "\" --auto";
            RegConfig.KeyName = "FirewallSetting";
            if(args!=null && args.Length > 0) 
            {
                IsAuto = (args[0] == "--auto");
            }


            Application.Run(new FrmMain());
        }
    }
}
