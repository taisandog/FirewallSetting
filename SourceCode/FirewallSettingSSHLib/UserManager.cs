using Buffalo.ArgCommon;
using Buffalo.Kernel;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserManager
    {

        public static readonly string BasePath = CommonMethods.GetBaseRoot("App_Data\\") ;
        //private static SshClient _ssh;
        ///// <summary>
        ///// SSH连接
        ///// </summary>
        //public SshClient SSHClient
        //{
        //    get
        //    {
        //        return _ssh;
        //    }
        //    set
        //    {
        //        _ssh = value;
        //    }
        //}

        private List<FirewallItem> _firewallRule;
        /// <summary>
        /// 防火墙规则名
        /// </summary>
        public List<FirewallItem> FirewallRule
        {
            get
            {
                return _firewallRule;
            }
            set
            {
                _firewallRule=value;
            }
        }

        private List<FWUser> _lstUser;

        /// <summary>
        /// 所有用户
        /// </summary>
        public List<FWUser> AllUser
        {
            get
            {
                return _lstUser;
            }
        }

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        /// <summary>
        /// 加载用户信息
        /// </summary>
        public void LoadUser()
        {

            

            _lstUser = FWUser.LoadConfig();
        }
        /// <summary>
        /// 根据名字获取用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FWUser GetUser(string name)
        {
            foreach (FWUser u in _lstUser)
            {
                if (u.UserName == name)
                {
                    return u;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public APIResault AddUser(FWUser user)
        {
            foreach(FWUser u in _lstUser)
            {
                if (u.UserName == user.UserName)
                {
                    return ApiCommon.GetFault("已存在用户:" + user.UserName);
                }
            }
            _lstUser.Add(user);

            return ApiCommon.GetSuccess();
        }
        /// <summary>
        /// 保存信息
        /// </summary>
        public void SaveConfig()
        {
            string path = BasePath + "userInfo.xml";
            FWUser.SaveConfig( _lstUser);
        }
        /// <summary>
        /// 刷新到防火墙信息
        /// </summary>
        public void RefreashFirewall()
        {
            List<string> lstIP = LoadUserIP();

            Dictionary<int, bool> dicPort = LoadRulePort();

           

           
            using (SshClient ssh = FirewallUnit.CreateSsh())
            {
                ssh.Connect();
                //对别哪些需要执行
                Dictionary<string, FirewallRule> dicExistsRule = LoadExists(dicPort, ssh);
                List<string> cmd = CreateCMD(lstIP, dicPort, dicExistsRule);
                foreach (string command in cmd)
                {
                    SshCommand res = ssh.RunCommand(command);
                    if (!string.IsNullOrWhiteSpace(res.Error))
                    {
                        Console.WriteLine(res.Error);
                    }
                }
                ssh.RunCommand("firewall-cmd --reload");
            }
        }
        /// <summary>
        /// 创建指令
        /// </summary>
        /// <returns></returns>
        private List<string> CreateCMD(List<string> lstIP, Dictionary<int, bool> dicPort, Dictionary<string, FirewallRule> dicExistsRule)
        {
            List<FirewallRule> lstCreateItem = new List<FirewallRule>();//需要创建的列表
            foreach (string ip in lstIP)
            {
                foreach (KeyValuePair<int, bool> kvpPort in dicPort)
                {
                    int port = kvpPort.Key;
                    string key = GetKey(ip, port);
                    if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                    {
                        dicExistsRule.Remove(key);
                        continue;
                    }
                    lstCreateItem.Add(new FirewallRule(ip, port));
                }
            }

            List<string> cmd = new List<string>();

            foreach (FirewallRule rule in lstCreateItem)
            {
                cmd.Add(rule.CreateAddCommand());//增加白名单命令
            }
            foreach (KeyValuePair<string, FirewallRule> kvpRule in dicExistsRule)
            {
                FirewallRule rule = kvpRule.Value;
                cmd.Add(rule.CreateDeleteCommand());//删除白名单命令
            }


            return cmd;
        }

        /// <summary>
        /// 加载规则端口
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, bool> LoadRulePort() 
        {
            //加载接管的端口
            Dictionary<int, bool> dicPort = new Dictionary<int, bool>();
            foreach (FirewallItem item in _firewallRule)
            {
                dicPort[item.port] = true;
            }
            return dicPort;
        }

        /// <summary>
        /// 加载用户IP
        /// </summary>
        /// <returns></returns>
        private List<string> LoadUserIP() 
        {
            string lanIP = System.Configuration.ConfigurationManager.AppSettings["Server.AllowIP"];
            Dictionary<string, bool> dicExists = new Dictionary<string, bool>();
            List<string> lstIP = new List<string>(_lstUser.Count);
            List<string> cur = null;
            if (!string.IsNullOrWhiteSpace(lanIP))
            {
                lstIP.Add(lanIP);
            }
            foreach (FWUser user in _lstUser)
            {
                cur = user.GetIP();
                foreach (string ip in cur)
                {
                    if (string.IsNullOrWhiteSpace(ip))
                    {
                        continue;
                    }
                    if (dicExists.ContainsKey(ip))
                    {
                        continue;
                    }
                    lstIP.Add(ip);
                    dicExists[ip] = true;
                }
            }
            return lstIP;
        }

        /// <summary>
        /// 加载现存规则
        /// </summary>
        /// <param name="dicPort"></param>
        /// <returns></returns>
        private Dictionary<string, FirewallRule> LoadExists(Dictionary<int, bool> dicPort, SshClient ssh)
        {
            SshCommand cmd = ssh.RunCommand("firewall-cmd --list-rich-rules");//查看当前规则
            string res = cmd.Result;
            StringReader reader = new StringReader(res);
            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();
            string line = null;
            string sport = null;
            string ip = null;
            int port = 0;
            while ((line = reader.ReadLine()) != null)
            {
                ip = SubValue("source address=", line);
                sport= SubValue("port port=", line);
                

                if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(sport)) 
                {
                    continue;
                }
                port = sport.ConvertTo<int>();
                if (!dicPort.ContainsKey(port)) //跳过非接管的端口
                {
                    continue;
                }
                string key = GetKey(ip, port);
                dicExists[key] = new FirewallRule(ip, port);
            }
            return dicExists;
        }
        /// <summary>
        /// 获取键
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="sport"></param>
        /// <returns></returns>
        private string GetKey(string ip, int port) 
        {
            return ip + "_" + port.ToString();
        }


        /// <summary>
        /// 截取内容
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="line">内容</param>
        /// <returns></returns>
        private string SubValue(string tag,string line) 
        {
            int startindex = line.IndexOf(tag, StringComparison.CurrentCultureIgnoreCase);
            if (startindex < 0) 
            {
                return null;
            }
            startindex = line.IndexOf("\"", startindex + 1,StringComparison.CurrentCultureIgnoreCase)+1;
            if (startindex < 0)
            {
                return null;
            }

            int endindex= line.IndexOf("\"", startindex + 1, StringComparison.CurrentCultureIgnoreCase);
            if (endindex < 0)
            {
                return null;
            }
            return line.Substring(startindex, endindex - startindex);
        }
    }
}
