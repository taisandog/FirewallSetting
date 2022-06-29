using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
namespace FWSettingClient
{
	/// <summary>
	/// RegConfig ��ժҪ˵����
	/// </summary>
	public class RegConfig
	{
		public RegConfig()
		{
			//
			// TODO: �ڴ˴���ӹ��캯���߼�
			//
		}

		public static string AutoRoot=Path.Combine(Application.StartupPath, "FWSettingClient.exe");
		public static string KeyName= "FWSettingClient";//ע������

		

		/// <summary>
		/// �����Ƿ�������(����Ա)
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
		/// �����Ƿ�������(���û�)
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
		/// ע�����в���ָ������
		/// </summary>
		/// <param name="key">ע����</param>
		/// <param name="val">����</param>
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
