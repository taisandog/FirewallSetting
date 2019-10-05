using Buffalo.ArgCommon;
using Buffalo.Kernel;
using NetFwTypeLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserManager
    {
        public static readonly string BasePath = CommonMethods.GetBaseRoot() + "\\App_Data\\";

        private List<FirewallItem> _firewallRule;
        /// <summary>
        /// 防火墙规则名
        /// </summary>
        public List<FirewallItem> FirewallRule
        {
            get
            {
                return _firewallRule;
            }
            set
            {
                _firewallRule=value;
            }
        }

        private List<FWUser> _lstUser;

        /// <summary>
        /// 所有用户
        /// </summary>
        public List<FWUser> AllUser
        {
            get
            {
                return _lstUser;
            }
        }

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        /// <summary>
        /// 加载用户信息
        /// </summary>
        public void LoadUser()
        {

            

            _lstUser = FWUser.LoadConfig();
        }
        /// <summary>
        /// 根据名字获取用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FWUser GetUser(string name)
        {
            foreach (FWUser u in _lstUser)
            {
                if (u.UserName == name)
                {
                    return u;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public APIResault AddUser(FWUser user)
        {
            foreach(FWUser u in _lstUser)
            {
                if (u.UserName == user.UserName)
                {
                    return ApiCommon.GetFault("已存在用户:" + user.UserName);
                }
            }
            _lstUser.Add(user);
            return ApiCommon.GetSuccess();
        }
        /// <summary>
        /// 保存信息
        /// </summary>
        public void SaveConfig()
        {
            string path = BasePath + "userInfo.xml";
            FWUser.SaveConfig( _lstUser);
        }
        /// <summary>
        /// 刷新到防火墙信息
        /// </summary>
        public void RefreashFirewall()
        {
            Dictionary<string, bool> dicExists = new Dictionary<string, bool>();
            List<string> lstIP = new List<string>(_lstUser.Count);
            string cur = null;
            foreach(FWUser user in _lstUser)
            {
                cur = user.IP;
                if (string.IsNullOrWhiteSpace(cur))
                {
                    continue;
                }
                if (dicExists.ContainsKey(cur))
                {
                    continue;
                }
                lstIP.Add(cur);
                dicExists[cur] = true;
            }
            foreach (FirewallItem item in _firewallRule)
            {
                WinFirewallUnit.SetWhiteIP(item.Rule, lstIP);
            }
        }
    }
}
