using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Buffalo.DBTools
{
    /// <summary>
    /// 版本信息
    /// </summary>
    public class ToolVersionInfo
    {

        
        /// <summary>
        /// 软件标题版本信息
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ass"></param>
        /// <returns></returns>
        public static string GetToolVerInfo(string title, Assembly ass)
        {

            string ver = ass.GetName().Version.ToString();
            StringBuilder sbInfo = new StringBuilder();
            sbInfo.Append(title);
            sbInfo.Append("   [v");
            sbInfo.Append(ver);
            sbInfo.Append("]");
            return sbInfo.ToString();
        }
        
    }
}
