using Renci.SshNet;
using SettingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib.FWAdapter
{
    /// <summary>
    /// firewalld适配器
    /// </summary>
    public class FirewalldAdapter : FWAdapterBase
    {
        public override string Name 
        {
            get 
            {
                return "Firewalld";
            }
        }
        /// <summary>
        /// 检查是否运行中
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh) 
        {
            SshCommand cmd = RunCommand(ssh,"firewall-cmd --state");//查看当前规则
            string res = cmd.Result;
            if (!string.IsNullOrWhiteSpace(res)) 
            {
                res = res.Trim(' ', '\r', '\n');
            }
            return string.Equals(res, "running", StringComparison.CurrentCultureIgnoreCase);
        }
        public override bool InitSetting(SshClient ssh)
        {
            return true;
        }
        /// <summary>
        /// 加载现存规则
        /// </summary>
        /// <param name="dicPort"></param>
        /// <returns></returns>
        private  Dictionary<string, FirewallRule> LoadExists(SshClient ssh)
        {
            Dictionary<int, bool> dicPort = LoadRulePort();
            SshCommand cmd = RunCommand(ssh,"firewall-cmd --list-rich-rules");//查看当前规则
            string res = cmd.Result;
            
            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();
            string line = null;
            string sport = null;
            string ip = null;
            string protocol = null;
            int port = 0;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    ip = SubValue("source address=", line);
                    sport = SubValue("port port=", line);
                    protocol = SubValue("protocol=", line);

                    if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(sport))
                    {
                        continue;
                    }
                    port = sport.ConvertTo<int>();
                    if (!dicPort.ContainsKey(port)) //跳过非接管的端口
                    {
                        continue;
                    }
                    string key = GetKey(ip, port, protocol);
                    dicExists[key] = new FirewallRule(ip, port, protocol);
                }
            }
            return dicExists;
        }
        /// <summary>
        /// 更新防火墙
        /// </summary>
        /// <param name="ssh"></param>
        public override void UpdateFirewall(SshClient ssh) 
        {
            List<string> cmd = CreateCommand(ssh);
            SshCommand res = null;
            foreach (string command in cmd)
            {
                res = RunCommand(ssh,command);
                ApplicationLog.LogCmdError(res);
            }
            res = RunCommand(ssh,"firewall-cmd --reload");
            ApplicationLog.LogCmdError(res);
        }
        /// <summary>
        /// 创建要执行的指令
        /// </summary>
        /// <param name="lstIP"></param>
        /// <param name="dicExistsRule"></param>
        private List<string> CreateCommand(SshClient ssh)
        {
            Dictionary<string, FirewallRule> dicExistsRule = LoadExists(ssh);
            List<string> lstIP = LoadUserIP();
            List<FirewallRule> lstCreateItem = new List<FirewallRule>();//需要创建的列表
            foreach (string ip in lstIP)
            {
                foreach (FirewallItem fwItem in _firewallRule)
                {
                    //int port = kvpPort.Key;
                    string key = GetKey(ip, fwItem.Port, fwItem.Protocol);
                    if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                    {
                        dicExistsRule.Remove(key);
                        continue;
                    }
                    lstCreateItem.Add(new FirewallRule(ip, fwItem.Port, fwItem.Protocol));
                }
            }

            List<string> cmd = new List<string>();

            foreach (FirewallRule rule in lstCreateItem)
            {
                cmd.Add(CreateAddCommand(rule));//增加白名单命令
            }
            foreach (KeyValuePair<string, FirewallRule> kvpRule in dicExistsRule)
            {
                FirewallRule rule = kvpRule.Value;
                cmd.Add(CreateDeleteCommand(rule));//删除白名单命令
            }
            return cmd;
        }

        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        private string CreateAddCommand(FirewallRule rule)
        {
            StringBuilder sbCmd = new StringBuilder();
            string ipType = IsIPV6(rule.IP) ? "ipv6" : "ipv4";
            sbCmd.Append("firewall-cmd --permanent --add-rich-rule=\"rule family=\"");
            sbCmd.Append(ipType);
            sbCmd.Append("\" source address=\"");

            sbCmd.Append(rule.IP);
            sbCmd.Append("\" port protocol=\"");
            sbCmd.Append(rule.Protocol);
            sbCmd.Append("\" port=\"");
            sbCmd.Append(rule.Port.ToString());
            sbCmd.Append("\" accept\"");
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        private string CreateDeleteCommand(FirewallRule rule)
        {
            StringBuilder sbCmd = new StringBuilder();
            string ipType = IsIPV6(rule.IP) ? "ipv6" : "ipv4";
            //sbCmd.Append("firewall-cmd --permanent --remove-rich-rule=\"rule family=\"ipv4\" source address=\"");
            sbCmd.Append("firewall-cmd --permanent --remove-rich-rule=\"rule family=\"");
            sbCmd.Append(ipType);
            sbCmd.Append("\" source address=\"");

            sbCmd.Append(rule.IP);
            sbCmd.Append("\" port protocol=\"");
            sbCmd.Append(rule.Protocol);
            sbCmd.Append("\" port=\""); ;
            sbCmd.Append(rule.Port.ToString());
            sbCmd.Append("\" accept\"");
            return sbCmd.ToString();
        }

        
    }
}
