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
    public class UFWiptablesAdapter: IPtableIPSetAdapter
    {
        public override string Name 
        {
            get { return "UFW.iptables.ipset"; }
        }
        /// <summary>
        /// 检查是否运行中
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        public override bool CheckEnable(SshClient ssh)
        {
            if (CheckUFWStatus(ssh))
            {
                return true;
            }

            return false;
        }
        protected override List<string> CreateAddCommand(FirewallRule rule)
        {
            List<string> lst= base.CreateAddCommand(rule);

            bool isIPv6 = IsIPv6(rule.IP);
            
            if (!isIPv6)
            {
                StringBuilder sbCmd = new StringBuilder();
                sbCmd.Append("ufw allow ");
                sbCmd.Append(rule.Port.ToString());
                lst.Add(sbCmd.ToString());
            }
            return lst;
        }

        /// <summary>
        /// 检查ufw
        /// </summary>
        /// <param name="ssh"></param>
        /// <returns></returns>
        private bool CheckUFWStatus(SshClient ssh)
        {
            SshCommand cmd = RunCommand(ssh, "ufw status");//查看当前规则
            string res = cmd.Result;

            if (!string.IsNullOrWhiteSpace(res))
            {
                string line = null;
                using (StringReader sr = new StringReader(res))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains("active"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
