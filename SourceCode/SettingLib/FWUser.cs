using Buffalo.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SettingLib
{
    /// <summary>
    /// 用户信息
    /// </summary>
    public class FWUser
    {
        protected SettingInvoke _handle=new SettingInvoke("");
        public static string XmlPath = null;
        /// <summary>
        /// 是否服务器
        /// </summary>
        public static bool IsServer = false;
        /// <summary>
        /// 网络请求
        /// </summary>
        [JsonIgnore]
        public SettingInvoke Handle
        {
            get
            {
                return _handle;
            }
            set
            {
                _handle = value;
            }
        }

        /// <summary>
        /// 配置名
        /// </summary>
        protected string _name;
        /// <summary>
        /// 配置名
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
       
        /// <summary>
        /// 连接
        /// </summary>
        public string Url
        {
            get
            {
                return _handle.BaseLoaction;
            }
            set
            {
                _handle.BaseLoaction = value;
            }
        }
        /// <summary>
        /// 用户名
        /// </summary>
        protected string _userName;
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
            }
        }

        /// <summary>
        /// 密码
        /// </summary>
        protected string _secret;
        /// <summary>
        /// 密码
        /// </summary>
        public string Secret
        {
            get
            {
                return _secret;
            }
            set
            {
                _secret = value;
            }
        }

        /// <summary>
        /// IP
        /// </summary>
        protected string _ip;
        /// <summary>
        /// IP
        /// </summary>
        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;
            }
        }

        private static readonly string ServerName = System.Configuration.ConfigurationManager.AppSettings["Server.Name"];

        private static readonly string ServerUrl = System.Configuration.ConfigurationManager.AppSettings["Server.URL"];
        /// <summary>
        /// 创建新的密钥
        /// </summary>
        /// <returns></returns>
        public static string CreateSecret()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                Guid id = Guid.NewGuid();
                byte[] arr = id.ToByteArray();
                sb.Append(CommonMethods.BytesToHexString(arr, false));
                sb.Append("-");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString() ;
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="tick">时间戳</param>
        /// <returns></returns>
        public string GetSign(long tick)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("name=");
            sb.Append(System.Web.HttpUtility.UrlEncode(_userName));
            sb.Append("&secret=");
            sb.Append(System.Web.HttpUtility.UrlEncode(_secret));
            sb.Append("&tick=");
            sb.Append(tick.ToString());
            string ret = sb.ToString();
            ret = PasswordHash.ToSHA1String(ret,false);
            return ret;
        }
        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="tick"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public bool VerifySign(long tick,string sign)
        {
            string curSign = GetSign(tick);
            return string.Equals(curSign, sign, StringComparison.CurrentCultureIgnoreCase);
        }



        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="path"></param>
        /// <param name="lstUser"></param>
        public static void SaveConfig( List<FWUser> lstUser)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);
            XmlNode rootNode = doc.CreateElement("root");
            doc.AppendChild(rootNode);
            
            foreach (FWUser user in lstUser)
            {
                XmlNode item = doc.CreateElement("account");

                XmlAttribute att = doc.CreateAttribute("name");
                if (IsServer)
                {
                    att.InnerText = ServerName;
                }
                else
                {
                    att.InnerText = user.Name;
                    
                }
                item.Attributes.Append(att);


                att = doc.CreateAttribute("url");
                if (IsServer)
                {
                    att.InnerText = ServerUrl;
                }
                else
                {
                    att.InnerText = user.Url;

                }
                item.Attributes.Append(att);
                
                att = doc.CreateAttribute("username");
                att.InnerText = user.UserName;
                item.Attributes.Append(att);

                att = doc.CreateAttribute("secretkey");
                att.InnerText = user.Secret;
                item.Attributes.Append(att);

                att = doc.CreateAttribute("ip");
                att.InnerText = user.IP;
                item.Attributes.Append(att);

                rootNode.AppendChild(item);
            }
            doc.Save(XmlPath);
        }
        /// <summary>
        /// 转换成Json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            
            

            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (IsServer)
            {
                
                dic["Name"] = ServerName;
            }
            else
            {
                dic["Name"] = _name;
            }
            if (IsServer)
            {
               
                dic["Url"] = ServerUrl;
            }
            else
            {
                dic["Url"] = Url;
            }
            dic["UserName"] = _userName;
            dic["Secret"] = _secret;

            return JsonConvert.SerializeObject(dic);
        }
        /// <summary>
        /// 转换成Json
        /// </summary>
        /// <returns></returns>
        public static FWUser LoadJson(string json)
        {
            return JsonConvert.DeserializeObject<FWUser>(json);
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static List<FWUser> LoadConfig()
        {
            //string xml = UserManager.BasePath + "\\accont.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(XmlPath);
            XmlNodeList lstRule = doc.GetElementsByTagName("account");
            List<FWUser> lstRet = new List<FWUser>();
            FWUser user = null;
            foreach (XmlNode node in lstRule)
            {
                user = new FWUser();
                XmlAttribute att = node.Attributes["name"];
                if (att != null)
                {
                    user.Name = att.InnerText;
                }

                att = node.Attributes["url"];
                if (att != null)
                {
                    user.Url = att.InnerText;
                }
                att = node.Attributes["username"];
                if (att != null)
                {
                    user.UserName = att.InnerText;
                }
                att = node.Attributes["secretkey"];
                if (att != null)
                {
                    user.Secret = att.InnerText;
                }
                att = node.Attributes["ip"];
                if (att != null)
                {
                    user.IP = att.InnerText;
                }
                lstRet.Add(user);
            }
            return lstRet;
        }
    }
}
