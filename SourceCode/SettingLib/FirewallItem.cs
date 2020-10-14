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
        private IList<INetFwRule2> _rule;
        /// <summary>
        /// 规则
        /// </summary>
        public IList<INetFwRule2> Rule
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
        /// <summary>
        /// 规则个数
        /// </summary>
        public int RuleCount 
        {
            get 
            {
                if (_rule == null) 
                {
                    return 0;
                }
                return _rule.Count;
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
