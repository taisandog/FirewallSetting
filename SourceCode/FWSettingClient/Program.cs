using SettingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FWSettingClient
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool canRun = false;
            FWUser.XmlPath = UserManager.BasePath + "\\accont.xml";
            FWUser.IsServer = false;
            using (System.Threading.Mutex mutex = new System.Threading.Mutex(true, "FWSettingClient.Mutex", out canRun))
            {
                if (!canRun)
                {
                    System.Windows.Forms.MessageBox.Show("已经启动了一个“自动更新SVN白名单”的实例");
                    return;
                }

                Application.Run(new FrmMain());
            }
        }
    }
}
