using Buffalo.ArgCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServerLib
{
    /// <summary>
    /// 网络函数接口
    /// </summary>
    public interface INetHandle
    {
        APIResault InvokeMethod(string methodName, string arg, HttpListenerRequest request, ref string textHtml);
    }
}
