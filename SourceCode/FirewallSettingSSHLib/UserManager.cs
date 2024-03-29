﻿using Buffalo.ArgCommon;
using Buffalo.Kernel;
using FirewallSettingSSHLib;
using FirewallSettingSSHLib.FWAdapter;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingLib
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserManager
    {

        public static readonly string BasePath = CommonMethods.GetBaseRoot("App_Data\\") ;
        /// <summary>
        /// 防火墙接口
        /// </summary>
        private FWAdapterBase _fwHandle;

        /// <summary>
        /// 防火墙接口
        /// </summary>
        public FWAdapterBase FWHandle 
        {
            get
            {
                return _fwHandle;
            }
        }

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;
        /// <summary>
        /// 加载信息
        /// </summary>
        public APIResault LoadInfo()
        {

            _fwHandle = LoadAdapter();

            if (_fwHandle==null) 
            {
                return ApiCommon.GetFault("没有可用的防火墙进程");
            }
            _fwHandle.AllUser= FWUser.LoadConfig();

            return ApiCommon.GetSuccess();
        }

        private FWAdapterBase LoadAdapter()
        {
            
            using (SshClient ssh = FirewallUnit.CreateSsh())
            {
                ssh.Connect();

                if (string.Equals(AppConfig.FirewallType, "iptables", StringComparison.CurrentCultureIgnoreCase))
                {
                    IPtableIPSetAdapter iptAdp = new IPtableIPSetAdapter();

                    if (iptAdp.InitSetting(ssh))
                    {
                        return iptAdp;
                    }

                }
                if (string.Equals(AppConfig.FirewallType, "ufw", StringComparison.CurrentCultureIgnoreCase))
                {
                    UFWiptablesAdapter ufwAdp = new UFWiptablesAdapter();

                    if (ufwAdp.InitSetting(ssh))
                    {
                        return ufwAdp;
                    }

                }
                if (string.Equals(AppConfig.FirewallType, "firewalld", StringComparison.CurrentCultureIgnoreCase))
                {
                    FirewalldIPSetAdapter fwAdp = new FirewalldIPSetAdapter();

                    if (fwAdp.InitSetting(ssh))
                    {
                        return fwAdp;
                    }

                }
                if (string.Equals(AppConfig.FirewallType, "nftable", StringComparison.CurrentCultureIgnoreCase))
                {
                    NFTableAdapter nftAdp = new NFTableAdapter();
                    if (nftAdp.InitSetting(ssh))
                    {
                        return nftAdp;
                    }

                }



                return FindFirewalld(ssh);
            }

        }
        /// <summary>
        /// 查找可用的防火墙
        /// </summary>
        /// <returns></returns>
        private FWAdapterBase FindFirewalld(SshClient ssh) 
        {
            
            FirewalldIPSetAdapter fwAdp = new FirewalldIPSetAdapter();
            if (fwAdp.CheckEnable(ssh))
            {
                if (fwAdp.InitSetting(ssh))
                {
                    return fwAdp;
                }
            }
            UFWiptablesAdapter ufwAdp = new UFWiptablesAdapter();
            if (ufwAdp.CheckEnable(ssh))
            {
                if (ufwAdp.InitSetting(ssh))
                {
                    return ufwAdp;
                }
            }


            IPtableIPSetAdapter iptAdp = new IPtableIPSetAdapter();
            if (iptAdp.CheckEnable(ssh))
            {
                if (iptAdp.InitSetting(ssh))
                {
                    return iptAdp;
                }
            }

            NFTableAdapter nftAdp = new NFTableAdapter();
            if (nftAdp.CheckEnable(ssh))
            {
                if (nftAdp.InitSetting(ssh))
                {
                    return nftAdp;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据名字获取用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public FWUser GetUser(string name)
        {
            foreach (FWUser u in _fwHandle.AllUser)
            {
                if (u.UserName == name)
                {
                    return u;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public APIResault AddUser(FWUser user)
        {
            foreach(FWUser u in _fwHandle.AllUser)
            {
                if (u.UserName == user.UserName)
                {
                    return ApiCommon.GetFault("已存在用户:" + user.UserName);
                }
            }
            _fwHandle.AllUser.Add(user);

            return ApiCommon.GetSuccess();
        }
        /// <summary>
        /// 保存信息
        /// </summary>
        public void SaveConfig()
        {
            lock (this)
            {
                string path = BasePath + "userInfo.xml";
                FWUser.SaveConfig(_fwHandle.AllUser);
            }
        }
        /// <summary>
        /// 刷新到防火墙信息
        /// </summary>
        public void RefreashFirewall()
        {
            lock (this)
            {
                List<string> lstIP = _fwHandle.LoadUserIP();

                
                using (SshClient ssh = FirewallUnit.CreateSsh())
                {
                    ssh.Connect();
                    //对别哪些需要执行

                    _fwHandle.UpdateFirewall(ssh);
                }
            }
        }
       
    }
}
