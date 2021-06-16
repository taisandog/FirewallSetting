
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
        public int port 
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
    }
}
