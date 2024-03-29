﻿using Buffalo.ArgCommon;
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
        private Thread _thd;
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
            _thd = new Thread(new ThreadStart(DoListen));
            _thd.Start();
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
        public APIResault RunMethod(string url, string methodName, string args, HttpListenerRequest request, ref string textHtml)
        {
            INetHandle handle = null;

            if (!_dicUrlMap.TryGetValue(url.Trim(' ', '\\', '/'), out handle))
            {
                return ApiCommon.GetFault("找不到位置:" + url);
            }
            return handle.InvokeMethod(methodName, args,request,ref textHtml);
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
        private APIResault DoMethods(HttpListenerRequest request, ref string textHtml)
        {
            ServiceRequest req = new ServiceRequest();
            req.Load(request);
            string arg = req.GetArgString();
            string method = req.MethodName;
            string url = req.Page;
           

            if (string.IsNullOrWhiteSpace(method))
            {
                return ApiCommon.GetFault("函数MethodName不能为空");
            }
            try
            {

                APIResault con = RunMethod(url, method, arg, request, ref textHtml);
                if (_message != null && _message.ShowLog)
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
                string textHtml = null;
                HttpListenerRequest request = context.Request;
                APIResault res = DoMethods(request,ref textHtml);
                
                //取得响应对象
                HttpListenerResponse response = context.Response;
                string responseBody = null;
                if (res == null)
                {
                    res = ApiCommon.GetFault("请求错误");
                }
                if (res.IsSuccess && !string.IsNullOrWhiteSpace(textHtml))
                {
                    response.ContentType = "text/html; Charset=UTF-8";
                    responseBody = textHtml;
                }
                else
                {
                    responseBody = res.ToJson();
                    response.ContentLength64 = System.Text.Encoding.UTF8.GetByteCount(responseBody);
                    response.ContentType = "application/json; Charset=UTF-8";

                }
                //设置响应头部内容，长度及编码
                
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
                    _thd.Abort();
                }
                catch { }
            }
            _thd = null;

            
            _server = null;
            Thread.Sleep(200);
        }
    }
}
