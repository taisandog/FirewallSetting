using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SettingLib
{
    /// <summary>
    /// 防火墙工具
    /// </summary>
    public class WinFirewallUnit
    {
        /// <summary>
        /// 默认添加的IP
        /// </summary>
        private readonly static string[] DefaultAllow = InitDefaultAllow();
        private static string[] InitDefaultAllow()
        {
            string defaultAllow = System.Configuration.ConfigurationManager.AppSettings["Server.AllowIP"];
            if (string.IsNullOrWhiteSpace(defaultAllow))
            {
                return new string[] { };
            }
            List<string> lst = new List<string>();
            string[] parts = defaultAllow.Split(',');
            foreach(string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }
                lst.Add(part.Trim());
            }
            return lst.ToArray();
        }

        /// <summary>
        /// 查找规则
        /// </summary>
        /// <param name="name">规则名</param>
        /// <param name="applicationPath">应用程序名</param>
        /// <returns></returns>
        public static INetFwRule2 FindRule(string ruleName, string applicationPath,string remotePorts,string localPorts, 
            string direction)
        {
            NET_FW_RULE_DIRECTION_ dir = GetDirect(direction);
               INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            foreach (INetFwRule2 firewallRule in firewallPolicy.Rules)
            {

                if(!IsFit(ruleName,firewallRule.Name))
                {
                    continue;
                }
                if (!IsFit(applicationPath, firewallRule.ApplicationName))
                {
                    continue;
                }
                if (!IsFit(remotePorts, firewallRule.RemotePorts))
                {
                    continue;
                }
                if (!IsFit(localPorts, firewallRule.LocalPorts))
                {
                    continue;
                }
                if(dir != firewallRule.Direction)
                {
                    continue;
                }
                return firewallRule;
            }
            return null;
        }
        /// <summary>
        /// 获取规则类型
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private static NET_FW_RULE_DIRECTION_ GetDirect(string dir)
        {
            if (string.Equals(dir, "IN", StringComparison.CurrentCultureIgnoreCase))
            {
                return NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            }
            if (string.Equals(dir, "OUT", StringComparison.CurrentCultureIgnoreCase))
            {
                return NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            }
            return NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_MAX;
        }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="source">配置的数值</param>
        /// <param name="target">目标的数值</param>
        /// <returns></returns>
        private static bool IsFit(string source,string target)
        {
            if (!string.IsNullOrWhiteSpace(source) && !string.Equals(source, target, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 用端口查找规则
        /// </summary>
        /// <param name="name">规则名</param>
        /// <param name="localPorts">端口</param>
        /// <returns></returns>
        public static INetFwRule2 FindRuleByPort(string name, string localPorts, NET_FW_RULE_DIRECTION_ direction)
        {
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            foreach (INetFwRule2 firewallRule in firewallPolicy.Rules)
            {
                string cname = firewallRule.Name;
                string port = firewallRule.LocalPorts;
                if (string.Equals(cname, name) && string.Equals(port, localPorts, StringComparison.CurrentCultureIgnoreCase)&& direction == firewallRule.Direction)
                {
                    return firewallRule;
                }
            }
            return null;
        }
        /// <summary>
        /// 设置白名单IP
        /// </summary>
        /// <param name="ipArr"></param>
        /// <returns></returns>
        public static string SetWhiteIP(INetFwRule2 firewallRule, IEnumerable<string> whiteips)
        {
            //firewallRule.Enabled = true;
            StringBuilder sbIP = new StringBuilder();
            foreach (string str in whiteips)
            {
                sbIP.Append(str);
                sbIP.Append(",");
            }

            foreach (string part in DefaultAllow)
            {
                sbIP.Append(part);
                sbIP.Append(",");
            }

            if (sbIP.Length > 0)
            {
                sbIP.Remove(sbIP.Length - 1, 1);
            }

            firewallRule.RemoteAddresses = sbIP.ToString();

            return null;
        }
    }
}
