using Buffalo.ArgCommon;
using Buffalo.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
namespace WebServerLib
{
    /// <summary>
    /// 网易回调
    /// </summary>
    public class ServiceRequest
    {
        public ServiceRequest()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        private string _contentType;
       
        private string _body;

        private string _methodName;
        /// <summary>
        /// 内容
        /// </summary>
        public string Body
        {
            get
            {
                return _body;
            }
            set 
            {
                _body = value;
            }
        }
        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }
        
        /// <summary>
        /// 函数名
        /// </summary>
        public string MethodName
        {
            get
            {
                return _methodName;
            }
            set
            {
                _methodName = value;
            }
        }
        /// <summary>
        /// 获取Arg的信息
        /// </summary>
        /// <returns></returns>
        public string GetArgString() 
        {
            if (string.IsNullOrWhiteSpace(_body)) 
            {
                return "";
            }
            string args = _body;
            args = args.Trim(' ', '=');
            args = System.Web.HttpUtility.UrlDecode(args);
            return args;
        }

        
        /// <summary>
        /// 加载回调信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string Load(HttpListenerRequest request)
        {
            _contentType = request.ContentType;
            
            _methodName = request.Headers["MethodName"];
            if (string.IsNullOrWhiteSpace(_methodName)) 
            {
                _methodName = request.QueryString["MethodName"];
            }
            _body = ReadBody(request);
            return null;
        }

        /// <summary>
        /// 读取信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string ReadBody(HttpListenerRequest request)
        {
            string ret = null;
            using (StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null) 
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    line=line.Trim();
                    if (line.StartsWith("{") && line.EndsWith("}")) 
                    {
                        return line;
                    }
                    if (line.StartsWith("=")) 
                    {
                        return line;
                    }
                }
            }
            return null;
        }

    }
}