using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
namespace FWSettingClient
{
	/// <summary>
	/// RegConfig 的摘要说明。
	/// </summary>
	public class RegConfig
	{
		public RegConfig()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}

		public static string AutoRoot=Path.Combine(Application.StartupPath, "FWSettingClient.exe");
		public static string KeyName= "FWSettingClient";//注册表键名

		

		/// <summary>
		/// 设置是否自启动(管理员)
		/// </summary>
		public static bool IsAutoRun
		{
			get
			{
                
				RegistryKey autoKey=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true);
				return FindValue(autoKey, KeyName);
			}
			set
			{
				RegistryKey autoKey=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true);
				if(value)
				{
					autoKey.SetValue(KeyName, AutoRoot);
				}
				else
				{
					autoKey.DeleteValue(KeyName, false);
				}
			}
		}
		

		/// <summary>
		/// 设置是否自启动(本用户)
		/// </summary>
		public static bool IsUserAutoRun
		{
			get
			{

				RegistryKey autoKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
				return FindValue(autoKey, KeyName);
			}
			set
			{
				RegistryKey autoKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
				if (value)
				{
					autoKey.SetValue(KeyName, AutoRoot);
				}
				else
				{
					autoKey.DeleteValue(KeyName, false);
				}
			}
		}


		/// <summary>
		/// 注册表键中查找指定的项
		/// </summary>
		/// <param name="key">注册表键</param>
		/// <param name="val">项名</param>
		/// <returns></returns>
		private static bool FindValue(RegistryKey key,string val)
		{
			string[] subkeys=key.GetValueNames();
			for(int i=0;i<subkeys.Length;i++)
			{
				if(val==subkeys[i])
				{
					return true;
				}
				
			}
			return false;
		}
	}
}
