using Buffalo.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
                _arrSecret = null;
            }
        }
        /// <summary>
        /// 密钥的分段
        /// </summary>
        private string[] _arrSecret;

        private bool _multipleIP;
        /// <summary>
        /// 多IP
        /// </summary>
        public bool MultipleIP
        {
            get
            {
                return _multipleIP;
            }

            set
            {
                _multipleIP = value;
            }
        }

        /// <summary>
        /// IP列表
        /// </summary>
        protected List<IpItem> _ipList=null;
        /// <summary>
        /// IP
        /// </summary>
        public List<IpItem> IPList
        {
            get
            {
                if (_ipList == null)
                {
                    _ipList = new List<IpItem>();
                }
                return _ipList;
            }
            
        }
        
        /// <summary>
        /// IP
        /// </summary>
        public string IPText
        {
            get
            {
                List<IpItem> lst = IPList;
                StringBuilder sbRet = new StringBuilder();
                lock (lst)
                {
                    foreach (IpItem item in lst)
                    {
                        sbRet.Append(item.IP);
                        sbRet.Append(",");
                    }
                    if (sbRet.Length > 0)
                    {
                        sbRet.Remove(sbRet.Length - 1, 1);
                    }
                }
                return sbRet.ToString();
            }
        }
        /// <summary>
        /// 更新IP
        /// </summary>
        /// <param name="ip"></param>
        public bool UpdateIP(string ip)
        {
            int change = 0;
            if (_multipleIP)
            {
                change +=DeleteOld();
                change += AddIP(ip);
            }
            else
            {
                List<IpItem> lst = IPList;
                lock (lst)
                {
                    if (lst.Count == 1)
                    {
                        if (lst[0].IP == ip)
                        {
                            UpdateLastDate(lst[0]);
                            return false;
                        }
                    }
                    lst.Clear();
                    AppendIP(ip);
                    return true;
                }
            }
            return change > 0;
        }
        public static double IPTimeOutMilliseconds = 60 * 60 * 1000;
        /// <summary>
        /// 删除过期IP
        /// </summary>
        private int DeleteOld()
        {
            List<IpItem> lst = IPList;
            int ret = 0;
            lock (lst)
            {
                DateTime now = DateTime.Now;
                IpItem item = null;
                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    item = lst[i];
                    if (now.Subtract(item.UpdateDate).TotalMilliseconds > IPTimeOutMilliseconds)
                    {
                        lst.RemoveAt(i);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取IP
        /// </summary>
        /// <returns></returns>
        public List<string> GetIP()
        {
            List<string> ret = new List<string>();
            List<IpItem> lst = IPList;
            lock (lst)
            {
                DeleteOld();
                if (_multipleIP)
                {
                    foreach (IpItem item in lst)
                    {
                        ret.Add(item.IP);
                    }
                    
                    return ret;
                }
                if (lst.Count == 1)
                {
                    ret.Add(lst[0].IP);
                }
            }
            return ret;
        }

        /// <summary>
        /// 增加到ID
        /// </summary>
        /// <param name="ip"></param>
        private int AddIP(string ip)
        {
            int ret = 0;
            List<IpItem> lst = IPList;
            lock (lst)
            {
                DateTime now = DateTime.Now;
                foreach(IpItem item in lst)
                {
                    if(item.IP== ip)
                    {
                        UpdateLastDate(item);
                        return 0;
                    }
                }
                AppendIP(ip);

            }
            return 1;
        }

        private void AppendIP(string ip)
        {
            List<IpItem> lst = IPList;
            lock (lst)
            {
                
                IpItem nitem = new IpItem();
                UpdateLastDate(nitem );
                nitem.IP = ip;
                lst.Add(nitem);
            }
        }

        private void UpdateLastDate(IpItem nitem) 
        {
            DateTime now = DateTime.Now;
            nitem.UpdateDate = now;
        }

        public static readonly string ServerName = AppSetting.Default["Server.Name"];

        public static readonly string ServerUrl = AppSetting.Default["Server.URL"];

        public static readonly int ServerKey = AppSetting.Default["Server.ServerKey"].ConvertTo<int>(3);
        /// <summary>
        /// 创建新的密钥
        /// </summary>
        /// <returns></returns>
        public static string CreateSecret()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ServerKey; i++)
            {
                Guid id = Guid.NewGuid();
                byte[] arr = id.ToByteArray();
                sb.Append(CommonMethods.BytesToHexString(arr, false));
                sb.Append("|");
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
        /// 生成签名
        /// </summary>
        /// <param name="tick">时间戳</param>
        /// <returns></returns>
        public string GetSignV2(long tick,string ip)
        {
            if (_arrSecret == null) 
            {
                _arrSecret = _secret.Split('|');
            }
            StringBuilder sbRet = new StringBuilder();
            foreach (string secret in _arrSecret)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("name=");
                sb.Append(System.Web.HttpUtility.UrlEncode(_userName));
                sb.Append("&secret=");
                sb.Append(secret);
                sb.Append("&tick=");
                sb.Append(tick.ToString());
                sb.Append("&ip=");
                sb.Append(ip);
                string hash = sb.ToString();
                hash = PasswordHash.ToSHA1String(hash, false);
                sbRet.Append(hash);
                sbRet.Append("|");
            }
            if (sbRet.Length > 0) 
            {
                sbRet.Remove(sbRet.Length-1, 1);
            }
            return sbRet.ToString() ;
        }

        

        ///// <summary>
        ///// 验证签名
        ///// </summary>
        ///// <param name="tick"></param>
        ///// <param name="sign"></param>
        ///// <returns></returns>
        //public bool VerifySign(long tick,string sign)
        //{
        //    string curSign = GetSign(tick);
        //    return string.Equals(curSign, sign, StringComparison.CurrentCultureIgnoreCase);
        //}



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

                att = doc.CreateAttribute("iplist");
                
                att.InnerText = JsonConvert.SerializeObject(user.IPList);
                item.Attributes.Append(att);

                att = doc.CreateAttribute("multipleIP");
                att.InnerText = user.MultipleIP?"1":"0";
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
            dic["V2"] = "1";
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
            List<FWUser> lstRet = new List<FWUser>();
            if (!File.Exists(XmlPath))
            {
                return lstRet;
            }
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(XmlPath);
                XmlNodeList lstRule = doc.GetElementsByTagName("account");

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
                    att = node.Attributes["iplist"];
                    if (att != null)
                    {
                        user._ipList =JsonConvert.DeserializeObject<List<IpItem>>( att.InnerText);
                    }
                    att = node.Attributes["multipleIP"];
                    if (att != null)
                    {
                        user.MultipleIP = att.InnerText=="1";
                    }
                    lstRet.Add(user);
                }
                
            }
            catch
            {

            }
            return lstRet;
        }
    }

    /// <summary>
    /// IP项信息
    /// </summary>
    public class IpItem
    {
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

        /// <summary>
        /// 更新日期
        /// </summary>
        protected DateTime _updateDate;
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime UpdateDate
        {
            get
            {
                return _updateDate;
            }
            set
            {
                _updateDate = value;
            }
        }
    }
}
