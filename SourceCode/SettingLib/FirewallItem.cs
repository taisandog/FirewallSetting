using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    public class FirewallItem
    {
        private INetFwRule2 _rule;
        /// <summary>
        /// 规则
        /// </summary>
        public INetFwRule2 Rule
        {
            get
            {
                return _rule;
            }
            set
            {
                _rule = value;
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
