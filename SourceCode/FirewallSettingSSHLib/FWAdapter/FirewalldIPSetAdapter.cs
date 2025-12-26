using Buffalo.ArgCommon;
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
    public class FirewalldIPSetAdapter : FWAdapterBase
    {
        public override string Name 
        {
            get 
            {
                return "Firewalld-ipset";
            }
        }
        /// <summary>
        /// 检查是否运行中
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh) 
        {
            CommandResault cmd = RunCommand(ssh,"firewall-cmd --state");//查看当前规则
            if (cmd.IsSuccess) 
            {
                return false;
            }
            string res = cmd.Result;

            if (!string.IsNullOrWhiteSpace(res)) 
            {
                res = res.Trim(' ', '\r', '\n');
            }
            bool ret= string.Equals(res, "running", StringComparison.CurrentCultureIgnoreCase);
            
            return ret;
        }

        public override bool InitSetting(SshClient ssh)
        {
            CheckIPSet(ssh, AppConfig.IPSetName, false);
            if (AppConfig.UseIPv6)
            {
                CheckIPSet(ssh, IPSetNameV6, true);
            }
            CommandResault cmd = RunCommand(ssh,"firewall-cmd --reload");
            ApplicationLog.LogCmdError(cmd);
            return true;
        }

        /// <summary>
        /// 加载现存规则
        /// </summary>
        /// <param name="dicPort"></param>
        /// <returns></returns>
        private Dictionary<string, FirewallRule> LoadExistsRule(SshClient ssh)
        {
            Dictionary<int, bool> dicPort = LoadRulePort();
            CommandResault cmd = RunCommand(ssh,"firewall-cmd --list-rich-rules");//查看当前规则
            string res = cmd.Result;
            
            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();
            string line = null;
            string sport = null;
            string ipset = null;
            string protocol = null;
            int port = 0;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    ipset = SubValue("source ipset=", line);
                    sport = SubValue("port port=", line);
                    protocol = SubValue("protocol=", line);

                    if (string.IsNullOrWhiteSpace(ipset) || string.IsNullOrWhiteSpace(sport))
                    {
                        continue;
                    }
                    port = sport.ConvertTo<int>();
                    
                    if (!string.Equals(ipset, AppConfig.IPSetName, StringComparison.CurrentCultureIgnoreCase) &&
                        !string.Equals(ipset, IPSetNameV6, StringComparison.CurrentCultureIgnoreCase)) 
                    {
                        continue;
                    }
                    string key = GetKey(ipset, port, protocol);
                    dicExists[key] = new FirewallRule(ipset, port, protocol);
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
            CommandResault res = null;
            List<string> cmd = CreateCommand(ssh);
            foreach (string command in cmd)
            {
                res = RunCommand(ssh,command);
                ApplicationLog.LogCmdError(res);
            }
            res = RunCommand(ssh,"firewall-cmd --reload");
            ApplicationLog.LogCmdError(res);
        }

        /// <summary>
        /// 加载现存IP
        /// </summary>
        /// <param name="ssh"></param>
        /// <param name="ipsetName">ip集名</param>
        /// <param name="existsIP">已存在IP集合</param>
        private void FillExistsIP(SshClient ssh,string ipsetName, Dictionary<string, bool> existsIP)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --ipset=");
            sbCmd.Append(ipsetName);
            sbCmd.Append(" --get-entries");
            CommandResault cmd = RunCommand(ssh,sbCmd.ToString());
            string res = cmd.Result;
            string line=null;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) 
                    {
                        continue;
                    }
                    existsIP[line] = true;
                }
            }
            
        }

        /// <summary>
        /// 创建要执行的指令
        /// </summary>
        /// <param name="lstIP"></param>
        /// <param name="dicExistsRule"></param>
        private List<string> CreateCommand(SshClient ssh)
        {
            List<string> cmd = new List<string>();
            Dictionary<string, FirewallRule> dicExistsRule = LoadExistsRule(ssh);
            List<string> lstIP = LoadUserIP();
            Dictionary<string, bool> existsIP = new Dictionary<string, bool>();
            FillExistsIP(ssh, AppConfig.IPSetName, existsIP);
            if (AppConfig.UseIPv6)
            {
                FillExistsIP(ssh, IPSetNameV6, existsIP);
            }
            List<FirewallRule> lstCreateItem = new List<FirewallRule>();//需要创建的列表
            FirewallRule rule = null;

            //更新IP
            foreach (string ip in lstIP)
            {
                if (existsIP.ContainsKey(ip)) 
                {
                    existsIP.Remove(ip);
                    continue;
                }
                
                cmd.Add(CreateAddIPCommand(ip));
            }
            string exIP = null;
            foreach (KeyValuePair<string, bool> kvpExip in existsIP)
            {
                exIP = kvpExip.Key;
                cmd.Add(CreateDeleteIPCommand(exIP));//删除集合IP
            }
            //更新规则
            foreach (FirewallItem fwItem in _firewallRule)
            {
                //int port = kvpPort.Key;
                string key = GetKey(AppConfig.IPSetName, fwItem.Port, fwItem.Protocol);
                if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                {
                    dicExistsRule.Remove(key);

                }
                else
                {
                    //lstCreateItem.Add(new FirewallRule(IPSetName, fwItem.Port, fwItem.Protocol));
                    rule = new FirewallRule(AppConfig.IPSetName, fwItem.Port, fwItem.Protocol);
                    cmd.Add(CreateAddCommand(rule));//增加白名单命令
                }

                if (AppConfig.UseIPv6)
                {
                    key = GetKey(IPSetNameV6, fwItem.Port, fwItem.Protocol);
                    if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                    {
                        dicExistsRule.Remove(key);
                    }
                    else
                    {
                        //lstCreateItem.Add(new FirewallRule(IPSetName, fwItem.Port, fwItem.Protocol));
                        rule = new FirewallRule(IPSetNameV6, fwItem.Port, fwItem.Protocol);
                        cmd.Add(CreateAddCommand(rule));//增加白名单命令
                    }
                }

            }

            foreach (KeyValuePair<string, FirewallRule> kvpRule in dicExistsRule)
            {
                rule = kvpRule.Value;
                cmd.Add(CreateDeleteCommand(rule));//删除白名单命令
            }
            return cmd;
        }
        /// <summary>
        /// 创建新增IP
        /// </summary>
        /// <returns></returns>
        private string CreateAddIPCommand(string ip)
        {
            string ipSet = AppConfig.IPSetName;
            if (IsIPV6(ip)) 
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --ipset=");
            sbCmd.Append(ipSet);
            sbCmd.Append(" --add-entry=");
            sbCmd.Append(ip);
            
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增IP
        /// </summary>
        /// <returns></returns>
        private string CreateDeleteIPCommand(string ip)
        {
            string ipSet = AppConfig.IPSetName;
            if (IsIPV6(ip))
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --ipset=");
            sbCmd.Append(ipSet);
            sbCmd.Append(" --remove-entry=");
            sbCmd.Append(ip);

            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        private string CreateAddCommand(FirewallRule rule)
        {
            string ipSet = rule.IP;
            bool isIPv6 = string.Equals(ipSet, IPSetNameV6,StringComparison.CurrentCultureIgnoreCase);
            string type = isIPv6 ? "ipv6" : "ipv4";
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --add-rich-rule=\"rule family=\"");
            sbCmd.Append(type);
            sbCmd.Append("\" source ipset=\"");
            sbCmd.Append(ipSet);
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
            string ipSet = rule.IP;
            bool isIPv6 = string.Equals(ipSet, IPSetNameV6, StringComparison.CurrentCultureIgnoreCase);
            string type = isIPv6 ? "ipv6" : "ipv4";
            StringBuilder sbCmd = new StringBuilder();

            sbCmd.Append("firewall-cmd --permanent --remove-rich-rule=\"rule family=\"");
            sbCmd.Append(type);
            sbCmd.Append("\" source ipset=\"");
            sbCmd.Append(ipSet);

            sbCmd.Append("\" port protocol=\"");
            sbCmd.Append(rule.Protocol);
            sbCmd.Append("\" port=\""); ;
            sbCmd.Append(rule.Port.ToString());
            sbCmd.Append("\" accept\"");
            return sbCmd.ToString();
        }


        private void CheckIPSet(SshClient ssh,string setName,bool isV6) 
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --ipset=");
            sbCmd.Append(setName);
            sbCmd.Append(" --get-entries");

            CommandResault cmd = RunCommand(ssh,sbCmd.ToString());//查看当前规则
            string res = cmd.Error;
            if (string.IsNullOrWhiteSpace(res)) //已存在则退出
            {
                return;
            }

            StringBuilder sbComd = new StringBuilder();

            sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --new-ipset=");
            sbCmd.Append(setName);
            sbCmd.Append(" --type=hash:ip");
            if (isV6) 
            {
                sbCmd.Append(" --option=family=inet6");
            }
            cmd = RunCommand(ssh,sbCmd.ToString()) ;//创建IP集
            ApplicationLog.LogCmdError(cmd);

        }
    }
}
