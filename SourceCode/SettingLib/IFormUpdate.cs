using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    public interface IFormUpdate
    {
        /// <summary>
        /// 用户有更改
        /// </summary>
        /// <returns></returns>
        bool OnUserUpdate();
    }
}
