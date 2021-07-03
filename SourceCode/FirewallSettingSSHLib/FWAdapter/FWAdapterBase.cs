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

        public abstract string Name { get; }

        /// <summary>
        /// 检查是否开启
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public abstract bool CheckEnable(SshClient ssh);
        /// <summary>
        /// 生成要执行的指令
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public abstract List<string> CreateCommand(SshClient ssh);
        /// <summary>
        /// 重新加载防火墙
        /// </summary>
        /// <param name="ssh"></param>
        public abstract void ReLoad(SshClient ssh);
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
            string lanIP = System.Configuration.ConfigurationManager.AppSettings["Server.AllowIP"];
            Dictionary<string, bool> dicExists = new Dictionary<string, bool>();
            List<string> lstIP = new List<string>(_allUser.Count);
            List<string> cur = null;
            if (!string.IsNullOrWhiteSpace(lanIP))
            {
                lstIP.Add(lanIP);
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
