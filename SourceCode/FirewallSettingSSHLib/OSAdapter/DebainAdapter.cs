using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib.OSAdapter
{
    /// <summary>
    /// Debian系统的适配
    /// </summary>
    public class DebainAdapter : OSAdapterBase
    {
        public override string Name 
        {
            get 
            {
                return "Debian/Ubuntu";
            }
        }

        /// <summary>
        /// 获取安装命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public override string GetInstallCommand(string packetName)
        {
            return "apt-get -y install "+packetName;
        }

        /// <summary>
        /// 获取iptable恢复命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public override string GetIPtablesRestoreCommand(bool isV6)
        {
            //if (isV6) 
            //{
            //    return "ip6tables-restore /etc/ip6tables.rules";
            //}
            //return "iptables-restore /etc/iptables.rules";
            return null;
        }
        /// <summary>
        /// 获取iptable恢复命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public override string GetIPtablesSaveCommand(bool isV6)
        {
            if (isV6)
            {
                return "ip6tables-save > /etc/ip6tables.rules";
            }
            return "iptables-save > /etc/iptables.rules";
        }
    }
}
