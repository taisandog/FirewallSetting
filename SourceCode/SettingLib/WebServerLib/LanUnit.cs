using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// 局域网判断类
/// </summary>
public class LanUnit
{
    //
    //private static Dictionary<string,bool> Localhost = { "localhost", "::1", "127.0.0.1" };
    public static readonly LanUnit Default = new LanUnit("LanUnit.AllowIP");
    private ConcurrentDictionary<string, bool> _dicAllowIP ;

    /// <summary>
    /// 局域网判断
    /// </summary>
    /// <param name="configName">appSetting配置名</param>
    public LanUnit(string configName)
    {
        _dicAllowIP = LoadAllowIP(configName);
    }
    public LanUnit() { }
    /// <summary>
    /// 是否全部允许
    /// </summary>
    private bool _isAllAllow = false;
    /// <summary>
    /// 是否全部允许
    /// </summary>
    public bool AllAllow
    {
        get { return _isAllAllow; }
    }
    /// <summary>
    /// 加载允许的IP
    /// </summary>
    /// <returns></returns>
    protected ConcurrentDictionary<string, bool> LoadAllowIP(string configName)
    {
        ConcurrentDictionary<string, bool> _dicAllowIP = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        //本地
        _dicAllowIP["localhost"] = true;
        _dicAllowIP["::1"] = true;
        _dicAllowIP["127.0.0.1"] = true;


        string config = System.Configuration.ConfigurationManager.AppSettings[configName];
        if (string.IsNullOrEmpty(config))
        {
            return _dicAllowIP;
        }
        string[] ret = config.Split(';');
        List<string> lstRet = new List<string>();
        string curIP = null;
        foreach (string item in ret)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                continue;
            }
            curIP = item.Trim();
            if (string.Equals(curIP, "0.0.0.0", StringComparison.CurrentCultureIgnoreCase))
            {
                _isAllAllow = true;//全部允许
                return _dicAllowIP;
            }
            _dicAllowIP[curIP] = true;
        }
        return _dicAllowIP;
    }
    /// <summary>
    /// 获取客户端IP地址（无视代理）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetHostAddress(HttpRequest request)
    {
        string userHostAddress = request.UserHostAddress;

        if (string.IsNullOrWhiteSpace(userHostAddress))
        {
            return userHostAddress;

        }
        userHostAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
        if (!string.IsNullOrWhiteSpace(userHostAddress))
        {
            return userHostAddress;
        }
        return "";
    }
    /// <summary>
    /// 获取客户端IP地址（无视代理）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static string GetHostAddress(HttpListenerRequest request)
    {
        string userHostAddress = request.RemoteEndPoint.Address.ToString();

        if (!string.IsNullOrWhiteSpace(userHostAddress))
        {
            return userHostAddress;

        }

        return "";
    }
    /// <summary>
    /// 判断是否允许的IP
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public bool IsAllowIP(string ip)
    {
        if (_isAllAllow)
        {
            return true;
        }
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }
        return _dicAllowIP.ContainsKey(ip.Trim());
    }
}

