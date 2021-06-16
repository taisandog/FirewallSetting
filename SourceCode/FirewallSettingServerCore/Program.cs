using Buffalo.Kernel;
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

        static void Main(string[] args)
        {
            FWUser.XmlPath = CommonMethods.GetBaseRoot("App_Data/userInfo.xml") ;
            _userMan = new UserManager();
            _userMan.LoadUser();
            FWUser.IsServer = true;
            try
            {
                
                List<FirewallItem> rule = GetRule();
                _userMan.FirewallRule = rule;
                if (!StartApiServices(new Program()))
                {
                    return;
                }
                _userMan.RefreashFirewall();
                RunToRoll();
            }
            
            finally
            {
                CloseServer();
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
                        Console.WriteLine(ex.Message);
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
                Console.WriteLine("show=显示用户的json");
                Console.WriteLine("add=新增用户");
                Console.WriteLine("renew=刷新用户安全码");
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

        private static bool StartApiServices(Program my)
        {
            
            _server = new WebServer();
            _server.Messager = my;
            string conUrl = System.Configuration.ConfigurationManager.AppSettings["Server.Listen"];
            if (string.IsNullOrWhiteSpace(conUrl))
            {

                Console.WriteLine("Server.Listen不能为空");

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
                    _server = null;

                    
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    
                        Console.WriteLine(ex.ToString());
                    
                }
            }
        }

        static void _authServices_OnException(Exception ex)
        {
            
                Console.WriteLine(ex.ToString());
            
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
            string ruleName = null;
            string remotePorts = null;
            foreach (XmlNode node in lstRule)
            {
                XmlAttribute att = node.Attributes["name"];
                name = att.InnerText;

                att = node.Attributes["ruleName"];
                ruleName = att.InnerText;

                att = node.Attributes["localPorts"];
                remotePorts = att.InnerText;

                FirewallItem item = new FirewallItem();
                item.Name = name;
                item.port = remotePorts.ConvertTo<int>();
                lstRet.Add(item);
            }


            return lstRet;
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.WriteLine(message);
        }

        public bool OnUserUpdate()
        {
            return true;
        }
    }
}
