
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SettingLib
{
    /// <summary>
    /// 防火墙工具
    /// </summary>
    public class FirewallUnit
    {
        /// <summary>
        /// 默认添加的IP
        /// </summary>
        private readonly static string[] DefaultAllow = InitDefaultAllow();
        private static string[] InitDefaultAllow()
        {
            string defaultAllow = System.Configuration.ConfigurationManager.AppSettings["Server.AllowIP"];
            if (string.IsNullOrWhiteSpace(defaultAllow))
            {
                return new string[] { };
            }
            List<string> lst = new List<string>();
            string[] parts = defaultAllow.Split(',');
            foreach(string part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }
                lst.Add(part.Trim());
            }
            return lst.ToArray();
        }

        /// <summary>
        /// 创建SSH连接
        /// </summary>
        /// <returns></returns>
        public static SshClient CreateSsh() 
        {
            string username= System.Configuration.ConfigurationManager.AppSettings["SSH.UserName"];
            string userpassword = System.Configuration.ConfigurationManager.AppSettings["SSH.UserPassword"];
            string privateKey = System.Configuration.ConfigurationManager.AppSettings["SSH.PrivateKey"];
            string host = System.Configuration.ConfigurationManager.AppSettings["SSH.Host"];
            string sport = System.Configuration.ConfigurationManager.AppSettings["SSH.Port"];

            int port = 22;
            if (!string.IsNullOrWhiteSpace(sport)) 
            {
                port = sport.ConvertTo<int>();
            }

            
            SshClient client = null;

            if (!string.IsNullOrWhiteSpace(privateKey))
            {
                List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
                if (!string.IsNullOrWhiteSpace(privateKey))
                {
                    PrivateKeyFile keyFile = new PrivateKeyFile(privateKey);
                    methods.Add(new PrivateKeyAuthenticationMethod(username, keyFile));

                    ConnectionInfo con = new ConnectionInfo(host, port, username, methods.ToArray());
                    client = new SshClient(con);
                }
            }
            else 
            {
                client = new SshClient(host,port, username, userpassword);
            }
            return client;
        }

        /// <summary>
        /// 是否匹配
        /// </summary>
        /// <param name="source">配置的数值</param>
        /// <param name="target">目标的数值</param>
        /// <returns></returns>
        private static bool IsFit(string source,string target)
        {
            if (!string.IsNullOrWhiteSpace(source) && !string.Equals(source, target, StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }
            return true;
        }

       
    }
    /// <summary>
    /// 防火墙规则
    /// </summary>
    public class FirewallRule 
    {

        public FirewallRule(string ip,int port) 
        {
            _port = port;
            _ip = ip;
        }

        /// <summary>
        /// 端口
        /// </summary>
        private int _port;
        /// <summary>
        /// 端口
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
        }

        private string _ip;
        /// <summary>
        /// 名称
        /// </summary>
        public string IP
        {
            get
            {
                return _ip;
            }
        }

        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        public string CreateAddCommand() 
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --add-rich-rule=\"rule family=\"ipv4\" source address=\"");
            sbCmd.Append(_ip);
            sbCmd.Append("\" port protocol=\"tcp\" port=\"");
            sbCmd.Append(_port.ToString());
            sbCmd.Append("\" accept\"");
            return sbCmd.ToString();
        }
        /// <summary>
        /// 创建新增命令
        /// </summary>
        /// <returns></returns>
        public string CreateDeleteCommand()
        {
            StringBuilder sbCmd = new StringBuilder();
            sbCmd.Append("firewall-cmd --permanent --remove-rich-rule=\"rule family=\"ipv4\" source address=\"");
            sbCmd.Append(_ip);
            sbCmd.Append("\" port protocol=\"tcp\" port=\"");
            sbCmd.Append(_port.ToString());
            sbCmd.Append("\" accept\"");
            return sbCmd.ToString();
        }
    }
}
