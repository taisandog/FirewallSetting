using Buffalo.ArgCommon;
using Buffalo.Kernel;
using FirewallSettingSSHLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServerLib;

namespace SettingLib
{
    public class WebHandle : INetHandle
    {
        private Dictionary<string, string> _webContent;

        private Dictionary<string, string> InitWebContent() 
        {
            Dictionary<string, string> ret = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            string path = Path.Combine(CommonMethods.GetBaseRoot(),"App_Data/web/");
            DirectoryInfo dir = new DirectoryInfo(path);
            if(!dir.Exists ) 
            {
                return ret;
            }
            FileInfo[] files = dir.GetFiles();
            string key = null;
            string value=null;
            string title = String.Format("[{0}]白名单客户端", AppConfig.ServerName);
            foreach(FileInfo finfo in files) 
            {
                key = finfo.Name;
                value=File.ReadAllText(finfo.FullName);
                if (string.Equals(key, "index.html", StringComparison.CurrentCultureIgnoreCase)) 
                {
                    value = value.Replace("{{Web.Title}}", title);
                }
                ret[key] = value;
            }
            return ret;
        }

        public WebHandle() 
        {
            _webContent = InitWebContent();
        }


        public APIResault InvokeMethod(string methodName, string arg, HttpListenerRequest request,ref string textHtml)
        {
            bool ret=_webContent.TryGetValue(methodName, out textHtml);
            if(!ret) 
            {
                return ApiCommon.GetFault("找不到页面:" + methodName);
            }
            return ApiCommon.GetSuccess();
        }
    }
}
