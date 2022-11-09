using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib.OSAdapter
{
    public class CentosAdapter: OSAdapterBase
    {
        public override string Name
        {
            get
            {
                return "CentOS";
            }
        }
        /// <summary>
        /// 获取安装命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public override string GetInstallCommand(string packetName)
        {
            return "yum -y install " + packetName;
        }

        /// <summary>
        /// 获取iptable恢复命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public override string GetIPtablesRestoreCommand(bool isV6)
        {
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
                return "service ip6tables save";
            }
            return "service iptables save";
        }
    }
}
