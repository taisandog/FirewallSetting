using Buffalo.Kernel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Buffalo.Kernel
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class AppSetting: IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private Dictionary<string, string> _dicSetting = null;
        private AppSetting()
        {
            _dicSetting = LoadSetting();
        }

        public static readonly AppSetting Default = new AppSetting();

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _dicSetting.GetEnumerator();
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            string ret = null;
            if (_dicSetting.TryGetValue(key, out ret))
            {
                return ret;
            }
            return ConfigurationManager.AppSettings[key];
        }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
        }
        /// <summary>
         /// 获取所有键
         /// </summary>
         /// <returns></returns>
        public IEnumerable<string> Keys
        {
            get
            {
                return _dicSetting.Keys;
            }
        }
        /// <summary>
        /// 加载配置
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> LoadSetting()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            string[] keys = ConfigurationManager.AppSettings.AllKeys;
            foreach (string obj in keys)
            {
                if (string.IsNullOrWhiteSpace(obj) || string.Equals(obj, "App.Setting"))
                {
                    continue;
                }
                
                dic[obj] = ConfigurationManager.AppSettings[obj];
            }

            string path = ConfigurationManager.AppSettings["App.Setting"];
            if (string.IsNullOrWhiteSpace(path))
            {
                return dic;
            }
            path = CommonMethods.GetBaseRoot(path);
            if (!File.Exists(path))
            {
                return dic;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlAttribute attr = null;
            string key = null;
            string value = null;

            foreach (XmlNode basenode in doc.ChildNodes)
            {
                XmlNodeList lstNode = basenode.ChildNodes;

                foreach (XmlNode node in lstNode)
                {
                    if (node.Attributes == null)
                    {
                        continue;
                    }
                    attr = node.Attributes["key"];
                    if (attr == null)
                    {
                        continue;
                    }
                    key = attr.InnerText;
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }
                    if (dic.ContainsKey(key))
                    {
                        continue;
                    }
                    attr = node.Attributes["value"];
                    if (attr == null)
                    {
                        continue;
                    }
                    value = attr.InnerText;
                    
                    dic.Add(key, value);
                }
            }
            


            return dic;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dicSetting.GetEnumerator();
        }
    }
}
