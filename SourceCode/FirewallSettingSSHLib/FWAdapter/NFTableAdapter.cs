using Renci.SshNet;
using SettingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FirewallSettingSSHLib.FWAdapter
{
    /// <summary>
    /// nftable适配器
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
        private static readonly string NFTableName = GetNFTableName();

        private static string GetNFTableName()
        {
            if (string.IsNullOrWhiteSpace(AppConfig.NFTableName))
            {
                return Firewalld;
            }
            return AppConfig.NFTableName;
        }

        /// <summary>
        /// NFTable链名
        /// </summary>
        private static readonly string NFTableChain = GetNFTableChain();

        private static string GetNFTableChain()
        {
            string chain = AppConfig.NFChain;
            if (string.IsNullOrWhiteSpace(chain))
            {
                return "filter_IN_public_allow";
            }
            return chain;
        }

        /// <summary>
        /// 检查是否运行中（对应 iptables 版本的 CheckIPTablesStatus）
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh)
        {
            CommandResault cmd = RunCommand(ssh, "service nftables status");
            string res = cmd.Result;

            if (string.IsNullOrWhiteSpace(res))
            {
                return false;
            }

            using (StringReader sr = new StringReader(res))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace(" ", "");
                    if (line.Contains("Active:active"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 初始化设置（对应 iptables 版本的 InitSetting + IPtablesRestore）
        /// </summary>
        public override bool InitSetting(SshClient ssh)
        {
            CheckIPSet(ssh, AppConfig.IPSetName, false);
            if (AppConfig.UseIPv6)
            {
                CheckIPSet(ssh, IPSetNameV6, true);
            }
            // 恢复持久化规则，对应 iptables 版的 IPtablesRestore
            NFTablesRestore(ssh);
            return true;
        }

        /// <summary>
        /// 恢复之前保存的 nftables 规则（对应 IPtablesRestore）
        /// </summary>
        private void NFTablesRestore(SshClient ssh)
        {
            CommandResault cmd = RunCommand(ssh, "nft -f /etc/nftables.conf");
            if (!IsSuccess(cmd))
            {
                Console.WriteLine(cmd.Error);
            }
        }

        /// <summary>
        /// 加载现存规则（对应 iptables 版本的 LoadExistsRule）
        /// </summary>
        private Dictionary<string, FirewallRule> LoadExistsRule(SshClient ssh, List<FirewallRule> repeatListNumber)
        {
            Dictionary<int, bool> dicPort = LoadRulePort();

            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft --handle list chain inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);

            CommandResault cmd = RunCommand(ssh, sbCmd.ToString());
            string res = cmd.Result;

            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();
            string line = null;
            string sportTag = " dport ";

            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    int dportIndex = line.IndexOf(sportTag);
                    if (dportIndex < 0)
                    {
                        continue;
                    }

                    // 截取协议（dport 前面紧邻的单词，如 tcp/udp）
                    string protocol = LoadProtocol(line, dportIndex);
                    if (string.IsNullOrWhiteSpace(protocol))
                    {
                        continue;
                    }

                    // 截取端口号：dport 后面的数字
                    int startIndex = dportIndex + sportTag.Length;
                    int endIndex = line.IndexOf(' ', startIndex);
                    if (endIndex < 0)
                    {
                        endIndex = line.Length;
                    }
                    string sport = line.Substring(startIndex, endIndex - startIndex);
                    int port = sport.ConvertTo<int>();
                    if (port <= 0)
                    {
                        continue;
                    }

                    // 截取 handle 值
                    int hashIndex = line.LastIndexOf('#');
                    if (hashIndex < 0)
                    {
                        continue;
                    }
                    int handleIndex = line.IndexOf("handle", hashIndex, StringComparison.CurrentCultureIgnoreCase);
                    if (handleIndex < 0)
                    {
                        continue;
                    }
                    string handle = line.Substring(handleIndex + "handle".Length).Trim();

                    // 截取 ipset 名（@ 后面到下一个空格）
                    int atIndex = line.IndexOf('@');
                    if (atIndex < 0)
                    {
                        continue;
                    }
                    int ipsetEnd = line.IndexOf(' ', atIndex);
                    if (ipsetEnd < 0)
                    {
                        ipsetEnd = line.Length;
                    }
                    string ipset = line.Substring(atIndex + 1, ipsetEnd - atIndex - 1);

                    string key = GetKey(ipset, port, protocol);
                    FirewallRule rule = new FirewallRule(ipset, port, protocol);
                    rule.Handle = handle;

                    // 对应 iptables 版本的重复规则检测
                    if (dicExists.ContainsKey(key))
                    {
                        repeatListNumber.Add(rule);
                    }
                    else
                    {
                        dicExists[key] = rule;
                    }
                }
            }
            return dicExists;
        }

        /// <summary>
        /// 从 dport 位置往前截取协议名（tcp/udp 等）
        /// </summary>
        private static string LoadProtocol(string line, int dportIndex)
        {
            Stack<char> stk = new Stack<char>();
            for (int i = dportIndex - 1; i >= 0; i--)
            {
                char c = line[i];
                if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                {
                    break;
                }
                stk.Push(c);
            }
            StringBuilder sbRet = new StringBuilder();
            while (stk.Count > 0)
            {
                sbRet.Append(stk.Pop());
            }
            return sbRet.ToString();
        }

        /// <summary>
        /// 更新防火墙（对应 iptables 版本的 UpdateFirewall）
        /// </summary>
        public override void UpdateFirewall(SshClient ssh)
        {
            CommandResault res = null;
            List<string> cmd = CreateCommand(ssh);
            foreach (string command in cmd)
            {
                res = RunCommand(ssh, command);
                ApplicationLog.LogCmdError(res);
            }
            // 持久化保存，对应 iptables 版本的 GetIPtablesSaveCommand
            CommandResault saveCmd = RunCommand(ssh, "nft list ruleset > /etc/nftables.conf");
            ApplicationLog.LogCmdError(saveCmd);
        }

        /// <summary>
        /// 加载现存 IP（对应 iptables 版本的 FillExistsIP）
        /// nftables 的 elements 可能跨多行，需要多行拼接后再解析
        /// </summary>
        private void FillExistsIP(SshClient ssh, string ipsetName, Dictionary<string, bool> existsIP)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft list set inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipsetName);

            CommandResault cmd = RunCommand(ssh, sbCmd.ToString());
            string res = cmd.Result;

            // 找到 elements = { ... } 区块（可能跨多行）
            int eleStart = res.IndexOf("elements = {");
            if (eleStart < 0)
            {
                return;
            }
            int braceStart = res.IndexOf('{', eleStart);
            if (braceStart < 0)
            {
                return;
            }
            int braceEnd = res.IndexOf('}', braceStart);
            if (braceEnd < 0)
            {
                return;
            }

            string eleContent = res.Substring(braceStart + 1, braceEnd - braceStart - 1);
            string[] ips = eleContent.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string ip in ips)
            {
                string trimmed = ip.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    existsIP[trimmed] = true;
                }
            }
        }

        /// <summary>
        /// 创建要执行的指令（对应 iptables 版本的 CreateCommand）
        /// 结构与 iptables 版对齐：先更新 IP 集，再更新规则
        /// </summary>
        private List<string> CreateCommand(SshClient ssh)
        {
            List<string> cmd = new List<string>();
            List<FirewallRule> repeatListNumber = new List<FirewallRule>();

            // 先更新 IP 集（对应 UpdateIPset）
            UpdateIPset(ssh, cmd);

            // 再更新规则（对应 UpdateRule）
            List<FirewallRule> willDelete = new List<FirewallRule>();
            List<FirewallRule> lstCreateItem = new List<FirewallRule>();
            UpdateRule(ssh, repeatListNumber, willDelete, lstCreateItem);

            // 删除多余规则
            foreach (FirewallRule rule in willDelete)
            {
                cmd.Add(CreateDeleteCommand(rule));
            }
            // 同时删除重复规则
            foreach (FirewallRule rule in repeatListNumber)
            {
                cmd.Add(CreateDeleteCommand(rule));
            }

            // 新增规则
            foreach (FirewallRule rule in lstCreateItem)
            {
                cmd.Add(CreateAddCommand(rule));
            }

            return cmd;
        }

        /// <summary>
        /// 获取更新 IP 集合的指令（对应 iptables 版本的 UpdateIPset）
        /// </summary>
        private void UpdateIPset(SshClient ssh, List<string> cmd)
        {
            List<string> lstIP = LoadUserIP();
            Dictionary<string, bool> existsIP = new Dictionary<string, bool>();
            FillExistsIP(ssh, AppConfig.IPSetName, existsIP);
            if (AppConfig.UseIPv6)
            {
                FillExistsIP(ssh, IPSetNameV6, existsIP);
            }

            foreach (string ip in lstIP)
            {
                if (existsIP.ContainsKey(ip))
                {
                    existsIP.Remove(ip);
                    continue;
                }
                cmd.Add(CreateAddIPCommand(ip));
            }
            foreach (KeyValuePair<string, bool> kvpExip in existsIP)
            {
                cmd.Add(CreateDeleteIPCommand(kvpExip.Key));
            }
        }

        /// <summary>
        /// 获取更新规则的指令（对应 iptables 版本的 UpdateRule）
        /// </summary>
        private void UpdateRule(SshClient ssh, List<FirewallRule> repeatListNumber,
            List<FirewallRule> willDelete, List<FirewallRule> lstCreateItem)
        {
            Dictionary<string, FirewallRule> dicExistsRule = LoadExistsRule(ssh, repeatListNumber);

            foreach (FirewallItem fwItem in _firewallRule)
            {
                string key = GetKey(AppConfig.IPSetName, fwItem.Port, fwItem.Protocol);
                if (dicExistsRule.ContainsKey(key))
                {
                    dicExistsRule.Remove(key);
                }
                else
                {
                    lstCreateItem.Add(new FirewallRule(AppConfig.IPSetName, fwItem.Port, fwItem.Protocol));
                }

                if (AppConfig.UseIPv6)
                {
                    key = GetKey(IPSetNameV6, fwItem.Port, fwItem.Protocol);
                    if (dicExistsRule.ContainsKey(key))
                    {
                        dicExistsRule.Remove(key);
                    }
                    else
                    {
                        lstCreateItem.Add(new FirewallRule(IPSetNameV6, fwItem.Port, fwItem.Protocol));
                    }
                }
            }

            foreach (KeyValuePair<string, FirewallRule> kvpRule in dicExistsRule)
            {
                willDelete.Add(kvpRule.Value);
            }
        }

        /// <summary>
        /// 创建新增 IP 命令（对应 iptables 版本的 CreateAddIPCommand）
        /// </summary>
        private string CreateAddIPCommand(string ip)
        {
            string ipSet = AppConfig.IPSetName;
            if (IsIPV6(ip))
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft add element inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" { ");
            sbCmd.Append(ip);
            sbCmd.Append(" }");
            return sbCmd.ToString();
        }

        /// <summary>
        /// 创建删除 IP 命令（对应 iptables 版本的 CreateDeleteIPCommand）
        /// </summary>
        private string CreateDeleteIPCommand(string ip)
        {
            string ipSet = AppConfig.IPSetName;
            if (IsIPV6(ip))
            {
                ipSet = IPSetNameV6;
            }
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft delete element inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" { ");
            sbCmd.Append(ip);
            sbCmd.Append(" }");
            return sbCmd.ToString();
        }

        /// <summary>
        /// 创建新增规则命令（对应 iptables 版本的 CreateAddCommand）
        /// 白名单逻辑：不在 IP 集合里的来源地址，拒绝访问指定端口
        /// 对应 iptables: -m set ! --match-set <set> src -p <proto> --dport <port> -j REJECT
        /// nftables:  <proto> saddr != @<set> <proto> dport <port> drop
        /// </summary>
        private string CreateAddCommand(FirewallRule rule)
        {
            string ipSet = rule.IP;
            bool isIPv6 = string.Equals(ipSet, IPSetNameV6, StringComparison.CurrentCultureIgnoreCase);

            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft insert rule inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(NFTableChain);
            sbCmd.Append(" ");

            // ip/ip6 层地址匹配（取反，不在白名单里则拒绝）
            if (isIPv6)
            {
                sbCmd.Append("ip6 saddr != @");
            }
            else
            {
                sbCmd.Append("ip saddr != @");
            }
            sbCmd.Append(ipSet);
            sbCmd.Append(" ");

            // 协议 + 目标端口
            sbCmd.Append(rule.Protocol);
            sbCmd.Append(" dport ");
            sbCmd.Append(rule.Port);

            // 动作：丢弃（对应 iptables 的 REJECT）
            sbCmd.Append(" drop");

            return sbCmd.ToString();
        }

        /// <summary>
        /// 创建删除规则命令（对应 iptables 版本的 CreateDeleteCommand）
        /// nftables 需要用 handle 号删除
        /// </summary>
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
        /// 检查 IP 集合是否存在，不存在则创建（对应 iptables 版本的 CheckIPSet）
        /// </summary>
        private void CheckIPSet(SshClient ssh, string setName, bool isV6)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("nft list set inet ");
            sbCmd.Append(NFTableName);
            sbCmd.Append(" ");
            sbCmd.Append(setName);

            CommandResault cmd = RunCommand(ssh, sbCmd.ToString());
            string res = cmd.Result;

            // 对应 iptables 版本：检查输出中是否包含集合名来判断是否已存在
            if (!string.IsNullOrWhiteSpace(res))
            {
                using (StringReader sr = new StringReader(res))
                {
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(setName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return; // 已存在，退出
                        }
                    }
                }
            }

            // 创建 IP 集合
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
            // flags interval 支持 CIDR 段，对应 iptables 版本的 hash:net
            sbCmd.Append(" \\; flags interval \\; }");

            cmd = RunCommand(ssh, sbCmd.ToString());
            ApplicationLog.LogCmdError(cmd);
        }
    }
}