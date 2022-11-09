using Buffalo.Kernel;
using FirewallSettingSSHLib.OSAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirewallSettingSSHLib
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class AppConfig
    {
        
        /// <summary>
        /// 日志路径
        /// </summary>
        public static readonly string AppLog = AppSetting.Default["App.Log"];

        /// <summary>
        /// 日是否使用IPv6
        /// </summary>
        public static readonly bool UseIPv6 = AppSetting.Default["App.AllowIPv6"] == null ? true : AppSetting.Default["App.AllowIPv6"] == "1";

        /// <summary>
        /// ip集名字，默认buffirewallipset
        /// </summary>
        public static string IPSetName = AppSetting.Default["App.IPSet"] == null ? "buffirewallipset" : AppSetting.Default["App.IPSet"];

        /// <summary>
        /// 当前系统(centos、debian)，默认centos
        /// </summary>
        public static readonly string OSString = AppSetting.Default["App.OS"];

        /// <summary>
        /// 是否使用sudo执行命令
        /// </summary>
        public static readonly bool UseSudo = AppSetting.Default["App.UseSudo"] == "1";

        /// <summary>
        /// 默认加到白名单的IP
        /// </summary>
        public static string LanIPs = AppSetting.Default["Server.AllowIP"];

        /// <summary>
        /// 使用NTTables时候指定用来操作的表名
        /// </summary>
        public static string NFTableName = AppSetting.Default["App.NFTableName"];

        /// <summary>
        /// 使用NTTables时候指定用来操作的Chain
        /// </summary>
        public static string NFChain = AppSetting.Default["App.NFChain"];

        /// <summary>
        /// 本服务器名字
        /// </summary>
        public static readonly string ServerName = AppSetting.Default["Server.Name"];
        /// <summary>
        /// 本服务的外网访问地址
        /// </summary>
        public static readonly string ServerUrl = AppSetting.Default["Server.URL"];
        /// <summary>
        /// 使用几段guid来生成效验Key(默认是3)
        /// </summary>
        public static readonly int ServerKey = AppSetting.Default["Server.ServerKey"].ConvertTo<int>(3);
        /// <summary>
        /// 使用缓存的类型
        /// </summary>
        public static readonly string AppCacheType = AppSetting.Default["App.CacheType"];
        /// <summary>
        /// 缓存链接字符串
        /// </summary>
        public static readonly string AppCache = AppSetting.Default["App.Cache"];
        /// <summary>
        /// 强行用v2模式效验
        /// </summary>
        public static readonly bool ForceV2 = AppSetting.Default["App.ForceV2"] == "1";
        /// <summary>
        /// 防火墙类型（可选参数：iptables、firewalld、ufw、nftable）
        /// </summary>
        public static readonly string FirewallType = AppSetting.Default["Server.FirewallType"];

        /// <summary>
        /// 当前操作系统
        /// </summary>
        public static readonly OSAdapterBase OS = LoadOS();

        /// <summary>
        /// 加载OS类型
        /// </summary>
        /// <returns></returns>
        public static OSAdapterBase LoadOS()
        {
            string os = AppConfig.OSString;
            if (string.Equals("centos", os, StringComparison.CurrentCultureIgnoreCase))
            {
                return new CentosAdapter();
            }
            if (string.Equals("debian", os, StringComparison.CurrentCultureIgnoreCase))
            {
                return new DebainAdapter();
            }
            if (string.Equals("ubuntu", os, StringComparison.CurrentCultureIgnoreCase))
            {
                return new DebainAdapter();
            }
            return new CentosAdapter();
        }

    }
}
