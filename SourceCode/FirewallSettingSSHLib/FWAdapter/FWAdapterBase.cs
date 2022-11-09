using Buffalo.Kernel;
using FirewallSettingSSHLib.OSAdapter;
using Renci.SshNet;
using SettingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib.FWAdapter
{
    /// <summary>
    /// 防火墙操作适配器基类
    /// </summary>
    public abstract class FWAdapterBase
    {
        


       



        private string _ipsetNameV6;
        /// <summary>
        /// 防火墙ip集名
        /// </summary>
        public string IPSetNameV6
        {
            get
            {
                if (_ipsetNameV6 == null) 
                {
                    _ipsetNameV6 = AppConfig.IPSetName + ".v6";
                }
                return _ipsetNameV6;
            }

        }

        /// <summary>
        /// 判断是否执行成功
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool IsSuccess(SshCommand cmd) 
        {
            return cmd.ExitStatus == 0;
        }
        
        protected List<FirewallItem> _firewallRule;
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
                _firewallRule = value;
            }
        }

        protected List<FWUser> _allUser;

        /// <summary>
        /// 所有用户
        /// </summary>
        public List<FWUser> AllUser
        {
            get
            {
                return _allUser;
            }
            set 
            {
                _allUser = value;
            }
        }
        
        /// <summary>
        /// 运行命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public SshCommand RunCommand(SshClient ssh,string cmd) 
        {
            SshCommand ret = null;
            if (AppConfig.UseSudo)
            {
                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(FirewallUnit.UserPassword))
                {
                    sb.Append("echo -e '");
                    sb.Append(FirewallUnit.UserPassword);
                    sb.Append("' | sudo -S ");
                }
                else
                {
                    sb.Append("sudo ");
                }
                sb.Append(cmd);
                ret = ssh.RunCommand(sb.ToString());
            }
            else
            {
                ret = ssh.RunCommand(cmd);
            }
            
            return ret;
        }

        public abstract string Name { get; }

        /// <summary>
        /// 检查是否开启
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public abstract bool CheckEnable(SshClient ssh);
        /// <summary>
        /// 初始化Setting
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public abstract bool InitSetting(SshClient ssh);
        /// <summary>
        /// 更新防火墙
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public abstract void UpdateFirewall(SshClient ssh);
        /// <summary>
        /// 加载规则端口
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<int, bool> LoadRulePort()
        {
            //加载接管的端口
            Dictionary<int, bool> dicPort = new Dictionary<int, bool>();
            foreach (FirewallItem item in _firewallRule)
            {
                dicPort[item.Port] = true;
            }
            return dicPort;
        }

        /// <summary>
        /// 加载用户IP
        /// </summary>
        /// <returns></returns>
        public virtual List<string> LoadUserIP()
        {
            Dictionary<string, bool> dicExists = new Dictionary<string, bool>();
            List<string> lstIP = new List<string>(_allUser.Count);
            List<string> cur = null;
            string curIP = null;
            if (!string.IsNullOrWhiteSpace(AppConfig.LanIPs))
            {
                string[] lanIPArr = AppConfig.LanIPs.Split(',');
                foreach (string lanIP in lanIPArr)
                {
                    if (string.IsNullOrWhiteSpace(lanIP))
                    {
                        continue;
                    }
                    curIP = lanIP.Trim();
                    if (dicExists.ContainsKey(curIP))
                    {
                        continue;
                    }

                    
                    lstIP.Add(curIP);
                    dicExists[curIP] = true;
                }
            }
            foreach (FWUser user in _allUser)
            {
                cur = user.GetIP();
                foreach (string ip in cur)
                {
                    if (string.IsNullOrWhiteSpace(ip))
                    {
                        continue;
                    }
                    curIP = ip.Trim();
                    if (dicExists.ContainsKey(curIP))
                    {
                        continue;
                    }
                    lstIP.Add(curIP);
                    dicExists[curIP] = true;
                }
            }
            return lstIP;
        }

        /// <summary>
        /// 获取键
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="sport"></param>
        /// <returns></returns>
        protected virtual string GetKey(string ip, int port, string protocol)
        {
            StringBuilder sbRet = new StringBuilder();
            sbRet.Append(ip);
            sbRet.Append("_");
            sbRet.Append(port.ToString());
            sbRet.Append("_");
            sbRet.Append(protocol);
            return sbRet.ToString();
        }

        /// <summary>
        /// 判断地址是否ipv6
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        protected bool IsIPV6(string ip) 
        {
            return ip.Contains(":");
        }

        /// <summary>
        /// 截取内容
        /// </summary>
        /// <param name="tag">标记</param>
        /// <param name="line">内容</param>
        /// <returns></returns>
        protected virtual string SubValue(string tag, string line)
        {
            int startindex = line.IndexOf(tag, StringComparison.CurrentCultureIgnoreCase);
            if (startindex < 0)
            {
                return null;
            }
            startindex = line.IndexOf("\"", startindex + 1, StringComparison.CurrentCultureIgnoreCase) + 1;
            if (startindex < 0)
            {
                return null;
            }

            int endindex = line.IndexOf("\"", startindex + 1, StringComparison.CurrentCultureIgnoreCase);
            if (endindex < 0)
            {
                return null;
            }
            return line.Substring(startindex, endindex - startindex);
        }

    }
}
