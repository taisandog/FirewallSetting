using Buffalo.Kernel;
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
    ///nftable适配器
    /// </summary>
    public class NFTableAdapter : FWAdapterBase
    {
        public override string Name 
        {
            get 
            {
                return "nftable";
            }
        }
        private const string Firewalld = "firewalld";
        /// <summary>
        /// NFTable名字
        /// </summary>
        private static readonly string NFTableName =GetNFTableName();

        private static string GetNFTableName()
        {
            string name = AppSetting.Default["App.NFTableName"];
            if (string.IsNullOrWhiteSpace(name))
            {
                return Firewalld;
            }
            return name;
        }


        /// <summary>
        /// NFTable名字
        /// </summary>
        private static readonly string NFTableChain= GetNFTableChain();

        private static string GetNFTableChain() 
        {
            string chain = AppSetting.Default["App.NFChain"];
            if (string.IsNullOrWhiteSpace(chain) )
            {
                return "filter_IN_public_allow";
            }
            return chain;
        }

        /// <summary>
        /// 检查是否运行中
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh) 
        {
            SshCommand cmd = RunCommand(ssh, "nft list ruleset");//查看当前规则
            if (!IsSuccess(cmd)) 
            {
                return false;
            }
            string res = cmd.Result;
            if (string.IsNullOrWhiteSpace(res)) 
            {
                return true;
            }

            bool ret = res.IndexOf("table")>=0;
            
            return ret;
        }

        public override bool InitSetting(SshClient ssh)
        {
            //CheckTable(ssh);
            CheckIPSet(ssh, IPSetName,false);
            CheckIPSet(ssh, IPSetNameV6, true);
            SshCommand cmd = RunCommand(ssh, "nft list ruleset > /etc/nftables.conf");
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
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft --handle list chain inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);
            
           SshCommand cmd = RunCommand(ssh, sbCmd.ToString());//查看当前规则
            string res = cmd.Result;
            
            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();
            string line = null;
            string sport = null;
            string ipset = null;
            string protocol = null;
            int port = 0;
            int dportIndex = 0;
            int endIndex = 0;
            int startIndex = 0;
            string sportTag = " dport ";
            string handle = null;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    
                    dportIndex = line.IndexOf(sportTag);
                    if (dportIndex < 0)
                    {
                        continue;
                    }
                    //截取协议
                    protocol = LoadProtocol(line, dportIndex);
                    

                    //截取端口
                    startIndex = dportIndex + sportTag.Length ;
                    endIndex = line.IndexOf(' ', dportIndex + sportTag.Length+1);
                    sport = line.Substring(startIndex, endIndex - startIndex);
                    

                    //截取handle
                    startIndex = line.LastIndexOf("#");
                    if (startIndex < 0) 
                    {
                        continue;
                    }
                    startIndex= line.IndexOf("handle",startIndex,StringComparison.CurrentCultureIgnoreCase);
                    if (startIndex < 0)
                    {
                        continue;
                    }
                    startIndex = startIndex + "handle".Length;
                    handle = line.Substring(startIndex + 1);
                    handle = handle.Trim();

                    port = sport.ConvertTo<int>();

                    //截取ipset
                    startIndex = line.IndexOf("@");
                    if (startIndex < 0)
                    {
                        continue;
                    }
                    endIndex =line.IndexOf(' ', startIndex);
                    if (endIndex < 0)
                    {
                        continue;
                    }
                    ipset = line.Substring(startIndex+1, endIndex - startIndex-1);

                    string key = GetKey(ipset, port, protocol);
                    FirewallRule rule = new FirewallRule(ipset, port, protocol);
                    rule.Handle = handle;
                    dicExists[key] = rule;

                }
            }
            return dicExists;
        }

        private static string LoadProtocol(string line,int dportIndex) 
        {
            Stack<char> stk = new Stack<char>();
            for(int i = dportIndex - 1; i >= 0; i--) 
            {
                if(line[i]==' ' || line[i] == '\t' || line[i] == '\r' || line[i] == '\n') 
                {
                    break;
                }
                stk.Push(line[i]);
            }
            StringBuilder sbRet = new StringBuilder();
            while (stk.Count > 0) 
            {
                sbRet.Append(stk.Pop());
            }
            return sbRet.ToString();
        }
        /// <summary>
        /// 更新防火墙
        /// </summary>
        /// <param name="ssh"></param>
        public override void UpdateFirewall(SshClient ssh)
        {
            SshCommand res = null;
            List<string> cmd = CreateCommand(ssh);
            foreach (string command in cmd)
            {
                res = RunCommand(ssh,command);
                ApplicationLog.LogCmdError(res);
            }
            //res = RunCommand(ssh,"firewall-cmd --reload");
            //ApplicationLog.LogCmdError(res);
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
            sbCmd.Append("nft list set inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipsetName);
            SshCommand cmd = RunCommand(ssh,sbCmd.ToString());
            string res = cmd.Result;
            string line=null;
            int eleindex = 0;
            int startIndex = 0;
            int endIndex = 0;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) 
                    {
                        continue;
                    }
                    eleindex = line.IndexOf("elements = {");
                    if (eleindex < 0) 
                    {
                        continue;
                    }
                    startIndex = line.IndexOf('{');
                    if (startIndex < 0 )
                    {
                        continue;
                    }
                    startIndex++;
                    endIndex = line.IndexOf('}', startIndex);
                    if ( endIndex < 0) 
                    {
                        continue;
                    }
                    int len = endIndex - startIndex;
                    string str = line.Substring(startIndex, len);
                    string[] ips = str.Split(',');
                    foreach (string ip in ips)
                    {
                        if (!string.IsNullOrWhiteSpace(ip))
                        {
                            existsIP[ip.Trim()] = true;
                        }
                    }
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
            FillExistsIP(ssh,IPSetName, existsIP);
            FillExistsIP(ssh, IPSetNameV6, existsIP);
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
                string key = GetKey(IPSetName, fwItem.Port, fwItem.Protocol);
                if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                {
                    dicExistsRule.Remove(key);

                }
                else
                {
                    //lstCreateItem.Add(new FirewallRule(IPSetName, fwItem.Port, fwItem.Protocol));
                    rule = new FirewallRule(IPSetName, fwItem.Port, fwItem.Protocol);
                    cmd.Add(CreateAddCommand(rule));//增加白名单命令
                }


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
            string ipSet = IPSetName;
            if (IsIPV6(ip))
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft add element inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" {");

            sbCmd.Append(ip);
            sbCmd.Append("}");
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增IP
        /// </summary>
        /// <returns></returns>
        private string CreateDeleteIPCommand(string ip)
        {
            string ipSet = IPSetName;
            if (IsIPV6(ip))
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft delete element inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" {");

            sbCmd.Append(ip);
            sbCmd.Append("}");
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        private string CreateAddCommand(FirewallRule rule)
        {

            string ipSet = rule.IP;
            bool isIPv6 = string.Equals(ipSet, IPSetNameV6, StringComparison.CurrentCultureIgnoreCase);
            

            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft insert rule inet ");


            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);

            if (isIPv6)
            {
                sbCmd.Append(" ip6");
            }
            else
            {
                sbCmd.Append(" ip");

            }
            sbCmd.Append(" saddr @");
            sbCmd.Append(rule.IP);
            sbCmd.Append(" ");
            sbCmd.Append(rule.Protocol);
            
            sbCmd.Append(" dport ");
            sbCmd.Append(rule.Port);
            
            sbCmd.Append(" accept");
            
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建删除命令
        /// </summary>
        /// <returns></returns>
        private string CreateDeleteCommand(FirewallRule rule)
        {
            StringBuilder sbCmd = new StringBuilder();

            sbCmd.Append("nft delete rule inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);

            sbCmd.Append(" handle ");
            sbCmd.Append(rule.Handle);
            
            return sbCmd.ToString();
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="ssh"></param>
        private void CheckTable(SshClient ssh) 
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft list table inet ");
            sbCmd.Append(NFTableName);

            SshCommand cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集
            
            if (!IsSuccess(cmd)) //已存在
            {
                sbCmd = new StringBuilder();
                sbCmd.Append("nft add table inet ");
                sbCmd.Append(NFTableName);
                cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集
                ApplicationLog.LogCmdError(cmd);
                if (!IsSuccess(cmd))
                {
                    ApplicationLog.LogCmdError(cmd);
                    return;
                }
            }

            sbCmd = new StringBuilder();
            sbCmd.Append("nft list chain inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);
            cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集
            if (IsSuccess(cmd))
            {
                return;
            }

            sbCmd = new StringBuilder();
            sbCmd.Append("nft add chain inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);
            //sbCmd.Append(" { type filter hook input priority -10\\; }");
            sbCmd.Append(" {type filter hook forward priority 0 \\; policy accept \\; }");
            cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集
            ApplicationLog.LogCmdError(cmd);
            if (!IsSuccess(cmd))
            {
                ApplicationLog.LogCmdError(cmd);
            }
        }

        private void CheckIPSet(SshClient ssh,string setName, bool isV6) 
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft list set inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(setName);
           

            SshCommand cmd = RunCommand(ssh,sbCmd.ToString());//查看当前规则

            
            if (IsSuccess(cmd)) //已存在则退出
            {
                return;
            }

            sbCmd = new StringBuilder();

            sbCmd.Append("nft add set inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(setName);
            sbCmd.Append(" { type ");
            if (isV6)
            {
                sbCmd.Append("ipv6_addr");
            }
            else
            {
                sbCmd.Append("ipv4_addr");
            }
            sbCmd.Append(" \\; flags interval \\;}");
            cmd = RunCommand(ssh,sbCmd.ToString()) ;//创建IP集
            
            if (!IsSuccess(cmd)) 
            {
                ApplicationLog.LogCmdError(cmd);
                return;
            }

            //sbCmd = new StringBuilder();

            //sbCmd.Append("nft add rule inet ");
            //sbCmd.Append(NFTableName);
            //sbCmd.Append(" ");
            //sbCmd.Append(NFTableChain);
            //if (isV6) 
            //{
            //    sbCmd.Append(" ip6");
            //}
            //else 
            //{
            //    sbCmd.Append(" ip");
            //}
            //sbCmd.Append(" saddr @");
            //sbCmd.Append(setName);
            //sbCmd.Append(" accept");
            //cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集

        }
    }
}
