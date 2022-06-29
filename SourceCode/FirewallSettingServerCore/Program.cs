using Buffalo.ArgCommon;
using Buffalo.Kernel;
using Buffalo.Kernel.TreadPoolManager;
using FirewallSettingSSHLib;
using FirewallSettingSSHLib.FWAdapter;
using Library;
using Renci.SshNet;
using SettingLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using WebServerLib;

namespace FirewallSettingServerCore
{
    public class Program: IShowMessage, IFormUpdate
    {
        private static UserManager _userMan;
        private static WebServer _server;
        private static readonly string FirewallRule = System.Configuration.ConfigurationManager.AppSettings["Firewall.Rule"];
        
        public bool ShowLog
        {
            get
            {
                return true;
            }
        }

        public bool ShowError
        {
            get
            {
                return true;
            }
        }

        public bool ShowWarning 
        {
            get 
            {
                return true;
            }
        }
        /// <summary>
        /// 自动刷新线程
        /// </summary>
        private static BlockThread _thdRefreash;

        static void Main(string[] args)
        {
            FWUser.XmlPath = CommonMethods.GetBaseRoot("App_Data/userInfo.xml") ;
            _userMan = new UserManager();
            APIResault res=_userMan.LoadInfo();
            if (!res.IsSuccess) 
            {
                Console.WriteLine(res.Message);
                return;
            }
            Console.WriteLine("FirewallType:"+ _userMan.FWHandle.Name);
            Console.WriteLine("Log:" + ApplicationLog.BaseRoot);  
            FWUser.IsServer = true;
            try
            {
                
                List<FirewallItem> rule = GetRule();
                _userMan.FWHandle.FirewallRule = rule;
                if (!StartApiServices(new Program()))
                {
                    return;
                }
                _thdRefreash = BlockThread.Create(RefreashHandle);
                _thdRefreash.StartThread(null);
                RunToRoll();
            }
            
            finally
            {
                CloseServer();
            }
        }

        /// <summary>
        /// 自动刷新线程
        /// </summary>
        private static void RefreashHandle() 
        {
            DateTime _lastRefreash = DateTime.MinValue;

            while (_server!=null && _server.IsListener) 
            {
                if (DateTime.Now.Subtract(_lastRefreash).TotalMinutes >= 5) 
                {
                    _userMan.RefreashFirewall();
                    _lastRefreash = DateTime.Now;
                }
                Thread.Sleep(1000);
            }

        }

        /// <summary>
        /// 循环执行
        /// </summary>
        private static void RunToRoll()
        {
            string line = null;
            List<string> commands = null;
            try
            {
                while (true)
                {
                    
                    Console.WriteLine("命令:");
                    line = Console.ReadLine();
                    try
                    {
                        commands = LoadCommand(line);
                        if (commands.Count <= 0)
                        {
                            continue;
                        }

                        if (string.Equals(commands[0], "exit", StringComparison.CurrentCultureIgnoreCase))//退出
                        {
                            Console.WriteLine("关闭中");

                            break;
                        }
                        else if (string.Equals(commands[0], "refreash", StringComparison.CurrentCultureIgnoreCase))//刷新防火墙
                        {
                            _userMan.RefreashFirewall();
                            Console.WriteLine("刷新完毕");
                        }
                        else if (string.Equals(commands[0], "clear", StringComparison.CurrentCultureIgnoreCase))//清屏
                        {
                            Console.Clear();
                        }
                        else if (string.Equals(commands[0], "user", StringComparison.CurrentCultureIgnoreCase))//新增用户
                        {
                            DoAddUser(commands);
                        }
                    }catch(Exception ex) 
                    {
                        ApplicationLog.LogException("FirewallSetting", ex);
                    }
                }
            }
            finally
            {

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// 载入命令
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static List<string> LoadCommand(string line) 
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return new List<string>();
            }
            string[] commands = line.Split(' ');
            List<string> cmds = new List<string>(commands.Length + 1);
            foreach(string cmd in commands) 
            {
                if (string.IsNullOrWhiteSpace(cmd)) 
                {
                    continue;
                }
                cmds.Add(cmd);
            }
            return cmds;
        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="name"></param>
        private static void DoAddUser(List<string> command) 
        {
            if (command.Count < 2 ) 
            {
                Console.WriteLine("请输入动作:");
                Console.WriteLine("show=显示用户的json(user show name)");
                Console.WriteLine("add=新增用户，是否多IP(user add name 1)");
                Console.WriteLine("renew=刷新用户安全码,重新设置是否多IP(user renew name 1)");
                return;
            }
            string action = command[1];

            if (command.Count < 3 )
            {
                Console.WriteLine("请输入用户名");
                return;
            }
            string name= command[2];
            FWUser user = null;

            if (string.Equals(action, "show", StringComparison.CurrentCultureIgnoreCase))
            {
                user = _userMan.GetUser(name);
                if (user == null)
                {
                    Console.WriteLine("找不到用户:" + name);
                    return;
                }
            }
            else if (string.Equals(action, "add", StringComparison.CurrentCultureIgnoreCase))
            {
                
                user = new FWUser();
                user.UserName = name;
                user.Secret = FWUser.CreateSecret();
                user.MultipleIP = LoadIsMultiple(command);
                _userMan.AddUser(user);
                _userMan.SaveConfig();
            }
            else if (string.Equals(action, "renew", StringComparison.CurrentCultureIgnoreCase))
            {
                user = _userMan.GetUser(name);
                if (user == null)
                {
                    Console.WriteLine("找不到用户:" + name);
                    return;
                }

                user.Secret = FWUser.CreateSecret();
                user.MultipleIP = LoadIsMultiple(command);
                _userMan.SaveConfig();
            }
            else 
            {
                Console.WriteLine("不存在指令:" + action);
                return;
            }
            string json = user.ToJson();
            Console.WriteLine("用户配置:" + json);
        }

        /// <summary>
        /// 加载是否多IP
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static bool LoadIsMultiple(List<string> command) 
        {
            string isMultiple = "0";
            if (command.Count > 3)
            {
                isMultiple = command[3];
            }
            return isMultiple=="1";
        }
        private static bool StartApiServices(Program my)
        {
            
            _server = new WebServer();
            _server.Messager = my;
            string conUrl = System.Configuration.ConfigurationManager.AppSettings["Server.Listen"];
            if (string.IsNullOrWhiteSpace(conUrl))
            {

                ApplicationLog.LogError("Server.Listen不能为空");
                return false;
            }
            string[] urls = conUrl.Split(';');
            _server.ListeneAddress = urls;
            SettingHandle handle = new SettingHandle();
            handle.UserMan = _userMan;
            handle.Form = my;
            handle.Message = my;
            _server.OnException += _authServices_OnException;
            _server.UrlMap["Setting"] = handle;

            _server.StartServer();
       
            Console.WriteLine("服务启动成功，监听地址为:" + conUrl);

            return true;
        }

        private static void CloseServer() 
        {
            
            if (_server != null)
            {
                
                try
                {
                    _server.Stop();
                   

                    
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    
                        //Console.WriteLine(ex.ToString());
                    ApplicationLog.LogException("FirewallSetting", ex);
                }
            }
            if (_thdRefreash != null)
            {

                try
                {
                    _thdRefreash.StopThread();
                    


                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    ApplicationLog.LogException("FirewallSetting", ex);
                }
            }
            _server = null;
            _thdRefreash = null;
        }

        static void _authServices_OnException(Exception ex)
        {
            ApplicationLog.LogException("FirewallSetting", ex);
            
        }

        /// <summary>
        /// 获取规则
        /// </summary>
        /// <returns></returns>
        public static List<FirewallItem> GetRule()
        {
            string xml = CommonMethods.GetBaseRoot("App_Data/firewallRule.xml") ;
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList lstRule = doc.GetElementsByTagName("rule");
            List<FirewallItem> lstRet = new List<FirewallItem>();
            string name = null;
            //string ruleName = null;
            string port = null;
            string protocol = null;
            string[] arrProtocol = null;
            foreach (XmlNode node in lstRule)
            {
                XmlAttribute att = node.Attributes["name"];
                name = att.InnerText;


                att = node.Attributes["port"];
                port = att.InnerText;

                att = node.Attributes["protocol"];
                protocol = att.InnerText;
                if (string.IsNullOrWhiteSpace(protocol)) 
                {
                    protocol = "tcp";
                }
                arrProtocol = protocol.Split(',');

                foreach(string sProtocol in arrProtocol) 
                {
                    if (string.IsNullOrWhiteSpace(sProtocol)) 
                    {
                        continue;
                    }
                    FirewallItem item = new FirewallItem();
                    item.Name = name;
                    item.Port = port.ConvertTo<int>();
                    item.Protocol = sProtocol;
                    lstRet.Add(item);
                }
                
            }


            return lstRet;
        }

        public void Log(string message)
        {
            //Console.WriteLine(message);
            ApplicationLog.LogMessage(message);
        }

        public void LogError(string message)
        {
            //Console.WriteLine(message);
            ApplicationLog.LogError(message);
        }

        public void LogWarning(string message)
        {
            //Console.WriteLine(message);
            ApplicationLog.LogWarning(message);
        }

        public bool OnUserUpdate()
        {
            return true;
        }
    }
}
