using Buffalo.Kernel.FastReflection;
using FirewallSettingSSHLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 函数加载器
/// </summary>
public class APIWebMethod
{
    private Dictionary<string, FastInvokeHandler> _dicMethods = null;
    /// <summary>
    /// 获取函数的键
    /// </summary>
    /// <param name="methodInfo">函数反射</param>
    /// <param name="objectType">类型</param>
    /// <returns></returns>
    private string GetMethodInfoKey(MemberInfo methodInfo)
    {
        StringBuilder sbRet = new StringBuilder();

        sbRet.Append(methodInfo.Name.ToString());
        return sbRet.ToString();
    }
    /// <summary>
    /// 获取函数加载器
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static APIWebMethod CreateWebMethod(Type type)
    {
        APIWebMethod handle = new APIWebMethod();
        handle.Load(type);
        return handle;
    }
    /// <summary>
    /// 获取web函数
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public void Load(Type type)
    {
        Dictionary<string, FastInvokeHandler> dic = new Dictionary<string, FastInvokeHandler>(StringComparer.OrdinalIgnoreCase);
        MethodInfo[] methods = type.GetMethods(FastValueGetSet.AllBindingFlags);
        foreach (MethodInfo info in methods)
        {
            string key = GetMethodInfoKey(info);
            WebMethod att = info.GetCustomAttribute(typeof(WebMethod)) as WebMethod;
            if (att != null)
            {
                FastInvokeHandler handle = FastInvoke.GetMethodInvoker(info);
                dic[key] = handle;
            }
        }
        _dicMethods = dic;
    }
    /// <summary>
    /// 获取函数
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public FastInvokeHandler GetMethod(string name)
    {
        FastInvokeHandler ret = null;
        if (_dicMethods.TryGetValue(name, out ret))
        {
            return ret;
        }
        return null;
    }

}

