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
    public class IPtableIPSetAdapter: FWAdapterBase
    {
        public override string Name
        {
            get
            {
                return "iptables-ipset";
            }
        }
        /// <summary>
        /// 检查是否运行中
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh)
        {
            SshCommand cmd = RunCommand(ssh ,"service iptables status");//查看当前规则
            string res = cmd.Result;

            bool ret = false;
            if (!string.IsNullOrWhiteSpace(res))
            {
                string line = null;
                using (StringReader sr = new StringReader(res))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Replace(" ", "");
                        if (line.Contains("Active:active"))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            
            
            return ret;
        }
        public override bool InitSetting(SshClient ssh)
        {
            CheckIPSet(ssh, IPSetName, false);
            CheckIPSet(ssh, IPSetNameV6, true);
            return true;
        }
        /// <summary>
        /// 加载现存规则
        /// </summary>
        /// <param name="ssh">ssh</param>
        /// <param name="repeatListNumber">重复的规则号</param>
        /// <returns></returns>
        private Dictionary<string, FirewallRule> LoadExistsRule(SshClient ssh, List<FirewallRule> repeatListNumber)
        {
            Dictionary<int, bool> dicPort = LoadRulePort();
            int numHeadIndex = -1;
            int protHeadIndex = -1;
            string ipHead = null;
            string line = null;
            Dictionary<string, FirewallRule> dicExists = new Dictionary<string, FirewallRule>();


            SshCommand cmd = RunCommand(ssh,"iptables -nvL INPUT --line-number");//查看当前规则
            string res = cmd.Result;
            ipHead = "! match-set " + IPSetName;
            
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if(numHeadIndex<0) 
                    {
                        numHeadIndex = line.IndexOf("num ", StringComparison.CurrentCultureIgnoreCase);
                        protHeadIndex= line.IndexOf("prot ", StringComparison.CurrentCultureIgnoreCase);
                        
                        continue;
                    }
                    if (line.IndexOf(ipHead) <= 0)
                    {
                        continue;
                    }
                    FillRule(line, IPSetName, numHeadIndex, protHeadIndex, dicExists, repeatListNumber);
                }
            }

            cmd = RunCommand(ssh,"ip6tables -nvL INPUT --line-number");//查看当前规则
            res = cmd.Result;
            ipHead = "! match-set " + IPSetNameV6;
            numHeadIndex = -1;
            protHeadIndex = -1;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (numHeadIndex < 0)
                    {
                        numHeadIndex = line.IndexOf("num ", StringComparison.CurrentCultureIgnoreCase);
                        protHeadIndex = line.IndexOf("prot ", StringComparison.CurrentCultureIgnoreCase);
                        
                        continue;
                    }
                    if (line.IndexOf(ipHead) <= 0)
                    {
                        continue;
                    }
                    FillRule(line, IPSetNameV6, numHeadIndex, protHeadIndex, dicExists, repeatListNumber);
                }
            }


            return dicExists;
        }
        /// <summary>
        /// 填充规则
        /// </summary>
        /// <param name="line">当前数据</param>
        /// <param name="ipHead">判定</param>
        /// <param name="ipSet">当前ip集</param>
        /// <param name="numHeadIndex">num列的起始索引</param>
        /// <param name="protHeadIndex">prot列的起始索引</param>
        /// <param name="dicExists">当前集合</param>
        private void FillRule(string line,string ipSet, int numHeadIndex, int protHeadIndex,
            Dictionary<string, FirewallRule> dicExists, List<FirewallRule> repeatListNumber) 
        {
            //string head = "! match-set "+ ipSet;
            
            int port = GetPort(line);
            if (port <= 0) 
            {
                return;
            }
            string protocol = GetProtocol(line, protHeadIndex);
            if (string.IsNullOrWhiteSpace(protocol)) 
            {
                return;
            }
            int num = GetLineNum(line, numHeadIndex);
            if (port <= 0)
            {
                return;
            }
            string key = GetKey(ipSet, port, protocol);
            FirewallRule urle=new FirewallRule(ipSet, port, protocol);
            urle.LineNum = num;
            if (dicExists.ContainsKey(key))
            {
                repeatListNumber.Add(urle);
            }
            else
            {
                dicExists[key] = urle;
            }
        }
        /// <summary>
        /// 截取端口号
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int GetLineNum(string line, int numHeadIndex)
        {
            try
            {
                int end = line.IndexOf(" ", numHeadIndex);
                string ret = line.Substring(numHeadIndex, end - numHeadIndex);
                return ret.ConvertTo<int>();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 截取协议
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string GetProtocol(string line, int protHeadIndex)
        {
            try
            {
                int end = line.IndexOf(" ", protHeadIndex);
                string ret = line.Substring(protHeadIndex, end - protHeadIndex);
                return ret;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 截取端口号
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private int GetPort(string line) 
        {
            try
            {
                int index = line.IndexOf("dpt:");
                if (index <= 0)
                {
                    return 0;
                }
                index += 4;
                int end = line.IndexOf(" ", index);

                string sport = line.Substring(index, end - index);
                return sport.ConvertTo<int>();
            }
            catch 
            {
                return 0;
            }
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
            res = RunCommand(ssh,"service iptables save");
            res = RunCommand(ssh,"service ip6tables save");
            ApplicationLog.LogCmdError(res);
        }

        /// <summary>
        /// 加载现存IP
        /// </summary>
        /// <param name="ssh"></param>
        /// <param name="ipsetName">ip集名</param>
        /// <param name="existsIP">已存在IP集合</param>
        private void FillExistsIP(SshClient ssh, string ipsetName, Dictionary<string, bool> existsIP)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("ipset list ");
            sbCmd.Append(ipsetName);
            SshCommand cmd = RunCommand(ssh, sbCmd.ToString());
            string res = cmd.Result;
            string line = null;
            int state = 0;
            using (StringReader reader = new StringReader(res))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    if (line.StartsWith("Members:", StringComparison.CurrentCultureIgnoreCase)) 
                    {
                        state = 1;//开始读取IP
                        continue;
                    }
                    switch (state) 
                    {
                        case 1:
                            existsIP[line.Trim()]=true;
                            break;
                        default:
                            break;
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
            List<FirewallRule> willDelete = new List<FirewallRule>();

            List<FirewallRule> lstCreateItem = new List<FirewallRule>();//需要创建的列表
           

            UpdateIPset(ssh, cmd);

            UpdateRule(ssh, willDelete, lstCreateItem);


            FirewallRule tmp = null;
            for (int i=0;i< willDelete.Count-1; i++) 
            {
                for(int j = i + 1; j < willDelete.Count; j++) 
                {
                    if(willDelete[i].LineNum< willDelete[j].LineNum) 
                    {
                        tmp = willDelete[i];
                        willDelete[i] = willDelete[j];
                        willDelete[j] = tmp;
                    }
                }
            }

            Dictionary<string, bool> dicExistsLineNum = new Dictionary<string, bool>();//已存在的行数
            StringBuilder sbKey = null; ;
            foreach(FirewallRule rule in willDelete) 
            {
                sbKey = new StringBuilder();
                sbKey.Append(rule.LineNum.ToString());
                sbKey.Append(".");
                sbKey.Append(IsIPv6(rule.IP)?"v6":"v4");
                string key = sbKey.ToString();
                if (dicExistsLineNum.ContainsKey(key)) 
                {
                    continue;
                }
                dicExistsLineNum[key] = true;
                cmd.Add(CreateDeleteCommand(rule));
            }

            foreach(FirewallRule rule in lstCreateItem) 
            {
                cmd.Add(CreateAddCommand(rule));
            }

            return cmd;
        }

        /// <summary>
        /// 获取更新规则的指令
        /// </summary>
        /// <param name="ssh">ssh</param>
        /// <param name="willDelete">要删除的规则</param>
        /// <param name="lstCreateItem">要创建的规则</param>
        private void UpdateRule(SshClient ssh, List<FirewallRule> willDelete, List<FirewallRule> lstCreateItem)
        {
            Dictionary<string, FirewallRule> dicExistsRule = LoadExistsRule(ssh, willDelete);

            foreach (FirewallItem fwItem in _firewallRule)
            {
                string key = GetKey(IPSetName, fwItem.Port, fwItem.Protocol);
                if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                {
                    dicExistsRule.Remove(key);

                }
                else
                {
                    lstCreateItem.Add(new FirewallRule(IPSetName, fwItem.Port, fwItem.Protocol));
                }


                key = GetKey(IPSetNameV6, fwItem.Port, fwItem.Protocol);
                if (dicExistsRule.ContainsKey(key)) //已存在规则，在存在列表删除并跳过
                {
                    dicExistsRule.Remove(key);
                }
                else
                {
                    lstCreateItem.Add(new FirewallRule(IPSetNameV6, fwItem.Port, fwItem.Protocol));
                }
            }
            FirewallRule rule = null;
            foreach (KeyValuePair<string, FirewallRule> kvpRule in dicExistsRule)
            {
                rule = kvpRule.Value;
                willDelete.Add(rule); //删除已经不存在的白名单规则
            }
        }

        /// <summary>
        /// 获取更新ip集合的指令
        /// </summary>
        private void UpdateIPset(SshClient ssh,List<string> cmd) 
        {
            List<string> lstIP = LoadUserIP();
            Dictionary<string, bool> existsIP = new Dictionary<string, bool>();
            FillExistsIP(ssh, IPSetName, existsIP);
            FillExistsIP(ssh, IPSetNameV6, existsIP);
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
            sbCmd.Append("ipset add ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" ");
            sbCmd.Append(ip);

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
            sbCmd.Append("ipset del ");
            sbCmd.Append(ipSet);
            sbCmd.Append(" ");
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
            bool isIPv6= IsIPv6(ipSet);
            StringBuilder sbCmd = new StringBuilder();
            if (isIPv6)
            {
                sbCmd.Append("ip6tables");
            }
            else 
            {
                sbCmd.Append("iptables");
            }
            sbCmd.Append(" -I INPUT -m set ! --match-set \"");
            sbCmd.Append(ipSet);
            sbCmd.Append("\" src -p ");
            sbCmd.Append(rule.Protocol.ToString());
            sbCmd.Append(" --destination-port \"");
            sbCmd.Append(rule.Port.ToString());
            sbCmd.Append("\" -j REJECT");

            return sbCmd.ToString();
        }

        /// <summary>
        /// 判断是否ipv6
        /// </summary>
        /// <param name="ipset"></param>
        /// <returns></returns>
        private bool IsIPv6(string ipset) 
        {
            return string.Equals(ipset, IPSetNameV6, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        private string CreateDeleteCommand(FirewallRule rule)
        {
            string ipSet = rule.IP;
            bool isIPv6 = IsIPv6(ipSet);
            
            StringBuilder sbCmd = new StringBuilder();

            if (isIPv6)
            {
                sbCmd.Append("ip6tables");
            }
            else
            {
                sbCmd.Append("iptables");
            }
            //sbCmd.Append(" -D INPUT ");
            //sbCmd.Append(rule.LineNum.ToString());
            sbCmd.Append(" -D INPUT -m set ! --match-set \"");
            sbCmd.Append(ipSet);
            sbCmd.Append("\" src -p ");
            sbCmd.Append(rule.Protocol.ToString());
            sbCmd.Append(" --destination-port \"");
            sbCmd.Append(rule.Port.ToString());
            sbCmd.Append("\" -j REJECT");
            return sbCmd.ToString();
        }


        private void CheckIPSet(SshClient ssh, string setName, bool isV6)
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("ipset list ");
            sbCmd.Append(setName);

            SshCommand cmd = RunCommand(ssh, sbCmd.ToString());//查看当前规则
            string res = cmd.Result;
            string line = null;
            if (!string.IsNullOrWhiteSpace(res)) //已存在则退出
            {
                using (StringReader sr = new StringReader(res)) 
                {
                    while ((line = sr.ReadLine()) != null) 
                    {
                        line = line.Replace(" ", "");
                        if(string.Equals(line, "Name:" + setName, StringComparison.CurrentCultureIgnoreCase)) 
                        {
                            return;
                        }
                    }
                }
            }

            StringBuilder sbComd = new StringBuilder();

            sbCmd = new StringBuilder();
            sbCmd.Append("ipset create ");
            sbCmd.Append(setName);
            sbCmd.Append(" hash:net");
            if (isV6)
            {
                sbCmd.Append(" family inet6");
            }
            else 
            {
                sbCmd.Append(" family inet");
            }
            sbCmd.Append(" maxelem 1000000");
            
            cmd = RunCommand(ssh, sbCmd.ToString());//创建IP集
            ApplicationLog.LogCmdError(cmd);

        }
    }
}
