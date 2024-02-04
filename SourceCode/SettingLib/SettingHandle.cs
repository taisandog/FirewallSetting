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
using System.Web.Services;
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
                type = BuffaloCacheTypes.Web;
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

        public APIResault InvokeMethod(string methodName, string arg, HttpListenerRequest request, ref string textHtml)
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
        private const int PacketTimeout = 30;
        private static readonly string PackIDHead = "pk.";
        private static readonly bool ForceV2 = AppSetting.Default["App.ForceV2"] == "1";
        [WebMethod]
        public APIResault UpdateAddress(string args, HttpListenerRequest request)
        {
            string remoteIP = GetIP(request);
            string blockkey = KeyHead + remoteIP;

            long curTick = (long)CommonMethods.ConvertDateTimeInt(DateTime.Now, true, true);

            APIResault res = CheckBlockIP(blockkey, remoteIP, curTick);
            if (!res.IsSuccess)
            {
                return res;
            }


            ArgValues arg = ApiCommon.GetArgs(args);
            long tick = arg.GetDataValue<long>("Tick");
            string name = arg.GetDataValue<string>("Name");
            string sign = arg.GetDataValue<string>("Sign");
            bool isV2 = arg.GetDataValue<string>("V2")=="1";

            res = CheckPacket(curTick, name, tick);
            if (!res.IsSuccess)
            {
                return res;
            }

            FWUser user = _userMan.GetUser(name);
            if (user == null)
            {
                return ApiCommon.GetFault("找不到用户:" + name);
            }

            string cntkey = KeyCntHead + remoteIP;
            string cursign = null;
            if (!isV2)
            {
                if (ForceV2)
                {
                    return ApiCommon.GetFault("本程序强制用V2验证，请更新客户端");
                }
                cursign = user.GetSign(tick);
            }
            else
            {
                cursign = user.GetSignV2(tick, remoteIP);
            }
            if (!string.Equals(cursign, sign, StringComparison.CurrentCultureIgnoreCase))
            {

                int times = _cache.GetValue<int>(cntkey);
                times++;
                string err = null;
                if (times >= BlockTimes)
                {
                    _cache.SetValue<long>(blockkey, curTick, SetValueType.Set, BlockSecond);
                    _cache.DeleteValue(cntkey);
                    err = "效验错误,IP被屏蔽:" + remoteIP;
                }
                else
                {
                    _cache.SetValue<int>(cntkey, times, SetValueType.Set, BlockSecond);
                    err = "效验错误,错误次数:" + times;
                }
                return ApiCommon.GetFault(err);
            }
            _cache.DeleteValue(cntkey);
            if (!user.UpdateIP(remoteIP))
            {
                return ApiCommon.GetSuccess();
            }
            _userMan.RefreashFirewall();
            _userMan.SaveConfig();
            _form.OnUserUpdate();
            if (_message != null && _message.ShowLog)
            {
                _message.Log("用户:" + name + "，的IP更新为:" + remoteIP);
            }
            return ApiCommon.GetSuccess();
        }
        [WebMethod]
        public APIResault GetIP(string args, HttpListenerRequest request)
        {
            string remoteIP = GetIP(request);
            return ApiCommon.GetSuccess(null, remoteIP);
        }
        /// <summary>
        /// 检查是否被屏蔽
        /// </summary>
        /// <param name="remoteIP"></param>
        /// <returns></returns>
        private APIResault CheckBlockIP(string blockkey,string remoteIP, long curTick) 
        {
            
            long bTick = _cache.GetValue<long>(blockkey);
            if (curTick - bTick < BlockSecond)
            {
                return ApiCommon.GetFault(remoteIP + "，被写入黑名单");
            }
            if (bTick > 0)
            {
                _cache.DeleteValue(blockkey);
            }
            return ApiCommon.GetSuccess();
        }
        /// <summary>
        /// 检查数据包ID
        /// </summary>
        /// <param name="curTick"></param>
        /// <param name="tick"></param>
        /// <returns></returns>
        private APIResault CheckPacket(long curTick, string name, long tick) 
        {
            long left = Math.Abs(curTick - tick);
            if (left > PacketTimeout)
            {
                return ApiCommon.GetFault("请求已过期");
            }
            StringBuilder sbKey = new StringBuilder();
            sbKey.Append(PackIDHead);
            sbKey.Append(name);
            sbKey.Append(".");
            sbKey.Append(curTick.ToString("X"));
            string key = sbKey.ToString();
            bool isSet = _cache.SetValue<int>(key, 1, SetValueType.AddNew,30);
            if (!isSet)
            {
                return ApiCommon.GetFault("重复请求");
            }
            return ApiCommon.GetSuccess();
        }

        public string GetIP(HttpListenerRequest request)
        {
            
            return request.RemoteEndPoint.Address.ToString();
        }
    }
}
