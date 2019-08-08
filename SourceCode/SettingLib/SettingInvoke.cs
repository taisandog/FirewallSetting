using Buffalo.ArgCommon;
using SettingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    public class SettingInvoke
    {
        private string _baseLocation = null;
        /// <summary>
        /// 请求远程服务器
        /// </summary>
        /// <param name="baseLocation"></param>
        public SettingInvoke(string baseLocation)
        {
            BaseLoaction = baseLocation;
        }
        private string _url = null;
        /// <summary>
        /// 基础地址
        /// </summary>
        public string BaseLoaction
        {
            get
            {
                return _baseLocation;
            }
            set
            {
                _baseLocation = value;
                if (string.IsNullOrWhiteSpace(_baseLocation))
                {
                    _url = "";
                    return;
                }
                _url= _baseLocation.TrimEnd('/','\\',' ') + "/Setting";
            }
        }
        /// <summary>
        /// 获取数据处理类
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="hasToken">包含Token</param>
        /// <returns></returns>
        private RequestHelper GetHelper(string methodName)
        {
            StringBuilder sbUrl = new StringBuilder();
            sbUrl.Append(_url);
            sbUrl.Append("?MethodName=");
            sbUrl.Append(methodName);
            RequestHelper rh = new RequestHelper(sbUrl.ToString());
           
            return rh;
        }

        /// <summary>
        /// 更新地址
        /// </summary>
        /// <param name="name">游戏</param>
        /// <returns></returns>
        public APIResault UpdateAddress(string name,long tick,string sign)
        {
            RequestHelper rh = GetHelper("UpdateAddress");
            rh["Tick"] = tick;
            rh["Name"] = name;
            rh["Sign"] = sign;
            APIResault res = rh.DoPost();

            return res;
        }
    }
}
