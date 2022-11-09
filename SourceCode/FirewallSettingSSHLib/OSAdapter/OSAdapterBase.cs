using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib.OSAdapter
{
    /// <summary>
    /// 操作系统适配基类
    /// </summary>
    public abstract class OSAdapterBase
    {

        


        /// <summary>
        /// OS名
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 获取安装命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public abstract string GetInstallCommand(string packetName);

        /// <summary>
        /// 获取iptable恢复命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public abstract string GetIPtablesRestoreCommand(bool isV6);
        /// <summary>
        /// 获取iptable恢复命令
        /// </summary>
        /// <param name="packetName"></param>
        /// <returns></returns>
        public abstract string GetIPtablesSaveCommand(bool isV6);
    }
}
