using System;
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

		private static string autoRoot=Application.StartupPath+ "\\FWSettingClient.exe";
		private const string keyName= "FWSettingClient";//ע������
		

		/// <summary>
		/// �����Ƿ�������
		/// </summary>
		public static bool IsAutoRun
		{
			get
			{
                
				RegistryKey autoKey=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true);
				return FindValue(autoKey,keyName);
			}
			set
			{
				RegistryKey autoKey=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",true);
				if(value)
				{
					autoKey.SetValue(keyName,autoRoot);
				}
				else
				{
					autoKey.DeleteValue(keyName,false);
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
