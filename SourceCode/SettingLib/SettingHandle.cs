using Buffalo.ArgCommon;
using Buffalo.DB.CacheManager;
using Buffalo.Kernel;
using Library;
using SettingLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebServerLib;

namespace SettingLib
{
    public class SettingHandle: INetHandle
    {
        private QueryCache _cache = LoadCache();

        private static QueryCache LoadCache()
        {
            string type = AppSetting.Default["App.CacheType"];
            string cache = AppSetting.Default["App.Cache"];
            if (string.IsNullOrWhiteSpace(type))
            {
                type = BuffaloCacheTypes.System;
            }
            return CacheUnit.CreateCache(type, cache);
        }

        private static APIWebMethod _handle = APIWebMethod.CreateWebMethod(typeof(SettingHandle));
        /// <summary>
        /// 消息
        /// </summary>
        private IShowMessage _message;



        /// <summary>
        /// 消息
        /// </summary>
        public IShowMessage Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
            }
        }
        /// <summary>
        /// 消息
        /// </summary>
        private IFormUpdate _form;
        /// <summary>
        /// 消息
        /// </summary>
        public IFormUpdate Form
        {
            get
            {
                return _form;
            }
            set
            {
                _form = value;
            }
        }
        
        private UserManager _userMan;
        /// <summary>
        /// 玩家管理
        /// </summary>
        public UserManager UserMan
        {
            get
            {
                return _userMan;
            }
            set
            {
                _userMan = value;
            }
        }

        public APIResault InvokeMethod(string methodName, string arg, HttpListenerRequest request)
        {
            Buffalo.Kernel.FastReflection.FastInvokeHandler han = _handle.GetMethod(methodName);
            if (han == null)
            {
                return ApiCommon.GetFault("找不到函数:" + methodName);
            }

            APIResault con = han.Invoke(this, new object[] { arg, request }) as APIResault;
            return con;


        }

        private static readonly string KeyHead = "App.BkIP_";
        private static readonly string KeyCntHead = "App.CntIP_";
        private const int BlockSecond = 60 * 5;
        private const int BlockTimes = 5;

        [System.Web.Services.WebMethod]
        public APIResault UpdateAddress(string args, HttpListenerRequest request)
        {
            string remoteIP = GetIP(request);
            string key = KeyHead + remoteIP;
            
            long curTick = (long)CommonMethods.ConvertDateTimeInt(DateTime.Now);
            long bTick = _cache.GetValue<long>(key);
            if (curTick - bTick < BlockSecond)
            {
                return ApiCommon.GetFault(remoteIP+"，被写入黑名单");
            }
            if (bTick > 0)
            {
                _cache.DeleteValue(key);
            }

            

            ArgValues arg = ApiCommon.GetArgs(args);
            long tick = arg.GetDataValue<long>("Tick");
            string name = arg.GetDataValue<string>("Name");
            string sign= arg.GetDataValue<string>("Sign");

            
            long left = Math.Abs(curTick - tick);
            if(left > 30)
            {
                return ApiCommon.GetFault("请求已过期" );
            }
            FWUser user = _userMan.GetUser(name);
            if (user == null)
            {
                return ApiCommon.GetFault("找不到用户:"+name); 
            }

            string cntkey = KeyCntHead + remoteIP;

            string cursign = user.GetSign(tick);
            if (!string.Equals(cursign ,sign,StringComparison.CurrentCultureIgnoreCase))
            {

                int times = _cache.GetValue<int>(cntkey);
                times++;
                string err = null;
                if (times >= BlockTimes)
                {
                    _cache.SetValue<long>(key, curTick);
                    _cache.DeleteValue(cntkey);
                    err = "效验错误,IP被屏蔽:" + remoteIP;
                }
                else
                {
                    _cache.SetValue<int>(cntkey, times);
                    err = "效验错误,错误次数:" + times;
                }
                return ApiCommon.GetFault(err);
            }
            _cache.DeleteValue(cntkey);
            if (user.IP == remoteIP)
            {
                return ApiCommon.GetSuccess();
            }
            user.IP = remoteIP;
            _userMan.RefreashFirewall();
            _userMan.SaveConfig();
            _form.OnUserUpdate();
            if (_message!=null && _message.ShowLog)
            {
                _message.Log("用户:" + name + "，的IP更新为:" + remoteIP);
            }
            return ApiCommon.GetSuccess();
        }
        public string GetIP(HttpListenerRequest request)
        {
            
            return request.RemoteEndPoint.Address.ToString();
        }
    }
}
