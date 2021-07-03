
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

        private static string UserName = System.Configuration.ConfigurationManager.AppSettings["SSH.UserName"];
        private static string UserPassword = System.Configuration.ConfigurationManager.AppSettings["SSH.UserPassword"];
        private static string PrivateKey = System.Configuration.ConfigurationManager.AppSettings["SSH.PrivateKey"];
        private static string Host = System.Configuration.ConfigurationManager.AppSettings["SSH.Host"];
        private static string Sport = System.Configuration.ConfigurationManager.AppSettings["SSH.Port"];

        /// <summary>
        /// 创建SSH连接
        /// </summary>
        /// <returns></returns>
        public static SshClient CreateSsh() 
        {
            

            int port = 22;
            if (!string.IsNullOrWhiteSpace(Sport)) 
            {
                port = Sport.ConvertTo<int>();
            }

            
            SshClient client = null;

            if (!string.IsNullOrWhiteSpace(PrivateKey))
            {
                List<AuthenticationMethod> methods = new List<AuthenticationMethod>();
                if (!string.IsNullOrWhiteSpace(PrivateKey))
                {
                    PrivateKeyFile keyFile = new PrivateKeyFile(PrivateKey);
                    methods.Add(new PrivateKeyAuthenticationMethod(UserName, keyFile));

                    ConnectionInfo con = new ConnectionInfo(Host, port, UserName, methods.ToArray());
                    client = new SshClient(con);
                }
            }
            else 
            {
                client = new SshClient(Host,port, UserName, UserPassword);
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

        public FirewallRule(string ip,int port,string protocol) 
        {
            _port = port;
            _ip = ip;
            _protocol = protocol;
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
        private string _protocol;
        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol
        {
            get
            {
                return _protocol;
            }
        }
        
    }
}
