using Buffalo.ArgCommon;
using Buffalo.Kernel.TreadPoolManager;
using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebServerLib
{
    public delegate APIResault RequestHandle(HttpListenerRequest request);
    public delegate void ErrorHandle(Exception ex);
    /// <summary>
    /// 网页服务器
    /// </summary>
    public class WebServer
    {
        private HttpListener _server;
        
        /// <summary>
        /// 异常
        /// </summary>
        public event ErrorHandle OnException;
        private Dictionary<string, INetHandle> _dicUrlMap = new Dictionary<string, INetHandle>(StringComparer.CurrentCultureIgnoreCase);
        private string[] _lisAddress;
        /// <summary>
        /// IP限制器
        /// </summary>
        private LanUnit _lanUnit;
        /// <summary>
        /// IP限制器
        /// </summary>
        public LanUnit RequestLanUnit
        {
            get
            {
                return _lanUnit;
            }
            set
            {
                _lanUnit = value;
            }
        }

        private IShowMessage _message;
        /// <summary>
        /// IP限制器
        /// </summary>
        public IShowMessage Messager
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
        /// 网页服务器
        /// </summary>
        /// <param name="lisaddress">监听地址</param>
        public WebServer()
        {
            
        }

        
        /// <summary>
        /// 服务器是否监听中
        /// </summary>
        public bool IsListener
        {
            get
            {
                if (_server == null)
                {
                    return false;
                }
                return _server.IsListening;
            }
        }

        
        /// <summary>
        /// 监听地址
        /// </summary>
        public string[] ListeneAddress
        {
            get
            {
                return _lisAddress;
            }
            set
            {
                _lisAddress = value;
            }
        }
        /// <summary>
        /// 监听线程 
        /// </summary>
        private BlockThread _thd;
        public APIResault StartServer()
        {
            

            if (_lisAddress == null || _lisAddress.Length<=0)
            {
                return ApiCommon.GetFault("请先设置监听地址");
            }
            _server = new HttpListener();
            _server.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            foreach (string strUrl in _lisAddress)
            {
                _server.Prefixes.Add(strUrl);
            }
            
            _server.Start();
            _thd = BlockThread.Create(DoListen);
            _thd.StartThread(null);
            return ApiCommon.GetSuccess();
        }
        /// <summary>
        /// URL映射
        /// </summary>
        public Dictionary<string, INetHandle> UrlMap
        {
            get { return _dicUrlMap; }
        }
        /// <summary>
        /// 运行函数
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public APIResault RunMethod(string url, string methodName, string args, HttpListenerRequest request)
        {
            INetHandle handle = null;

            if (!_dicUrlMap.TryGetValue(url.Trim(' ', '\\', '/'), out handle))
            {
                return ApiCommon.GetFault("找不到位置:" + url);
            }
            return handle.InvokeMethod(methodName, args,request);
        }
        private void DoListen()
        {
            while (IsListener)
            {
                try
                {
                    HttpListenerContext httpListenerContext = _server.GetContext();
                    Thread thd = new Thread(new ParameterizedThreadStart(DoRequest));
                    thd.Start(httpListenerContext);
                }
                catch (Exception ex)
                {
                    if (OnException != null)
                    {
                        OnException(ex);
                    }
                }
            }
        }
        /// <summary>
        /// 执行函数
        /// </summary>
        private APIResault DoMethods(HttpListenerRequest request)
        {
            ServiceRequest req = new ServiceRequest();
            req.Load(request);
            string arg = req.GetArgString();
            string method = req.MethodName;
            string url = request.Url.AbsolutePath;
            string ip = LanUnit.GetHostAddress(request);
            
                
                if (_lanUnit != null && _lanUnit.IsAllowIP(ip))
                {
                    return ApiCommon.GetException(new System.Net.WebException("调用IP:" + ip + "不在白名单内"));
                }
            
            if (string.IsNullOrWhiteSpace(method))
            {
                return ApiCommon.GetFault("函数MethodName不能为空");
            }
            try
            {

                APIResault con = RunMethod(url, method, arg, request);
                if (_message!=null && _message.ShowLog)
                {
                    string mess = con.Message;
                    if (!string.IsNullOrWhiteSpace(mess))
                    {
                        _message.Log(mess);
                    }

                }
                return con;
            }
            catch (Exception ex)
            {
                if (OnException != null)
                {
                    OnException(ex);
                }
                return ApiCommon.GetException(ex);
            }

        }

        private void DoRequest(object objhttpListenerContext)
        {
            try
            {
                HttpListenerContext context = objhttpListenerContext as HttpListenerContext;
                if (context == null)
                {
                    return;
                }
                HttpListenerRequest request = context.Request;
                APIResault res = DoMethods(request);
                
                //取得响应对象
                HttpListenerResponse response = context.Response;
                string responseBody = null;
                if (res == null)
                {
                    res = ApiCommon.GetFault("请求错误");
                }
                responseBody = res.ToJson();
                //设置响应头部内容，长度及编码
                response.ContentLength64 = System.Text.Encoding.UTF8.GetByteCount(responseBody);
                response.ContentType = "application/json; Charset=UTF-8";
                response.Headers["Access-Control-Allow-Origin"] = "*";
                response.Headers["Access-Control-Allow-Methods"] = "POST,GET,PUT,DELETE";
                response.Headers["Access-Control-Max-Age"] = "3600";
                response.Headers["Access-Control-Allow-Headers"] = "*";
                response.Headers["Access-Control-Allow-Credentials"] = "true";
                using (StreamWriter sw = new StreamWriter(response.OutputStream))
                {
                    sw.Write(responseBody);
                }
            }
            catch (Exception ex)
            {
                if (OnException != null)
                {
                    OnException(ex);
                }
            }
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            if (_server != null)
            {
                try
                {
                    _server.Stop();
                }
                catch { }
            }

            if (_thd != null)
            {
                try
                {
                    _thd.StopThread();
                }
                catch { }
            }
            _thd = null;

            
            _server = null;
            Thread.Sleep(200);
        }
    }
}
