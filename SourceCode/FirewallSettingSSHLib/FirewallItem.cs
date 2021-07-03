
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    public class FirewallItem
    {
        /// <summary>
        /// 端口
        /// </summary>
        private int _port;
        /// <summary>
        /// 端口
        /// </summary>
        public int Port 
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        private string _name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private string _protocol;
        /// <summary>
        /// 协议
        /// </summary>
        public string Protocol
        {
            get
            {
                return _protocol;
            }
            set
            {
                _protocol = value;
            }
        }

    }
}
