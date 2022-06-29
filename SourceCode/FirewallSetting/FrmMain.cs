using Buffalo.DB.CacheManager;
using Buffalo.DBTools;
using Buffalo.Kernel.TreadPoolManager;
using FWSettingClient;
using Library;
using NetFwTypeLib;
using SettingLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Windows.Forms;
using System.Xml;
using WebServerLib;

namespace FirewallSetting
{
    public partial class FrmMain : Form, IShowMessage, IFormUpdate
    {
        /// <summary>
        /// 自动刷新线程
        /// </summary>
        private static BlockThread _thdRefreash;

        private UserManager _userMan;
        private WebServer _server;
        private static readonly string FirewallRule = System.Configuration.ConfigurationManager.AppSettings["Firewall.Rule"];
        public FrmMain()
        {
            InitializeComponent();
        }
        public bool ShowLog
        {
            get
            {
                return mbDisplay.ShowLog;
            }
        }

        public bool ShowError
        {
            get
            {
                return mbDisplay.ShowError;
            }
        }

        public bool ShowWarning
        {
            get
            {
                return mbDisplay.ShowWarning;
            }
        }
        public void Log(string message)
        {

            mbDisplay.Log(message);
        }

        public void LogError(string message)
        {
            mbDisplay.LogError(message);
        }
        public void LogWarning(string message)
        {
            mbDisplay.LogWarning(message);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            _userMan = new UserManager();
            _userMan.LoadUser();
            dgMembers.AutoGenerateColumns = false;
            dgRules.AutoGenerateColumns = false;
            RefreashUser();
            List<FirewallItem> rule = GetRule();
            dgRules.DataSource = rule;
            _userMan.FirewallRule = rule;
            chkAutoRun.Checked = RegConfig.IsUserAutoRun;
            SetTitle();
            if (Program.IsAuto) 
            {
                Btn_Connect_Click(Btn_Connect,new EventArgs());
            }
        }


        /// <summary>
        /// 设置标题
        /// </summary>
        private void SetTitle()
        {
            this.Text = ToolVersionInfo.GetToolVerInfo("防火墙白名单更新服务", this.GetType().Assembly);
        }
        private void RefreashUser()
        {
            if (_userMan == null)
            {
                return;
            }
            dgMembers.DataSource = null;
            dgMembers.DataSource = _userMan.AllUser;
        }

        private bool StartApiServices()
        {
            _server = new WebServer();
            _server.Messager = this;
            string conUrl = System.Configuration.ConfigurationManager.AppSettings["Server.Listen"];
            if (string.IsNullOrWhiteSpace(conUrl))
            {
                if (ShowError)
                {
                    LogError("Server.Listen不能为空");
                }
                return false;
            }


            
            
            string[] urls = conUrl.Split(';');
            _server.ListeneAddress = urls;
            SettingHandle handle = new SettingHandle();
            handle.UserMan = _userMan;
            handle.Form = this;
            handle.Message = this;
            _server.OnException += _authServices_OnException;
            _server.UrlMap["Setting"] = handle;
            _server.StartServer();
            if (ShowLog)
            {
                Log("服务启动成功，监听地址为:"+ conUrl);
            }
            return true;
        }

       

        void _authServices_OnException(Exception ex)
        {
            if (ShowError)
            {
                LogError(ex.ToString());
            }
        }

        /// <summary>
        /// 获取规则
        /// </summary>
        /// <returns></returns>
        public List<FirewallItem> GetRule()
        {
            string xml = UserManager.BasePath + "\\firewallRule.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(xml);
            XmlNodeList lstRule = doc.GetElementsByTagName("rule");
            List<FirewallItem> lstRet = new List<FirewallItem>();
            string name = null;
            string ruleName = null;
            string rulePath = null;
            string remotePorts = null;
            string localPorts = null;
            string direction = null;
            foreach (XmlNode node in lstRule)
            {
                XmlAttribute att = node.Attributes["name"];
                name = att.InnerText;

                att = node.Attributes["ruleName"];
                ruleName = att.InnerText;

                att = node.Attributes["rulePath"];
                rulePath = att.InnerText;

                att = node.Attributes["remotePorts"];
                remotePorts = att.InnerText;

                att = node.Attributes["localPorts"];
                localPorts = att.InnerText;

                att = node.Attributes["direction"];
                direction = att.InnerText;
                IList<INetFwRule2> netrule = WinFirewallUnit.FindRule(ruleName, rulePath, remotePorts, localPorts, direction);
                if (netrule == null || netrule.Count<=0)
                {
                    if (ShowError)
                    {
                        LogError("找不到规则:" + name);
                    }
                }
                FirewallItem item = new FirewallItem();
                item.Name = name;
                item.Rule = netrule;
                lstRet.Add(item);
            }

            
            return lstRet;
        }
        private void Btn_Connect_Click(object sender, EventArgs e)
        {
            

            RefreashUser();
            if (!StartApiServices())
            {
                return;
            }
            _thdRefreash = BlockThread.Create(RefreashHandle);
            _thdRefreash.StartThread(null);

            LogWarning("用户连接监听启动成功");
            Btn_Connect.Enabled = false;
            Btn_Disconnect.Enabled = true;
        }
        /// <summary>
        /// 自动刷新线程
        /// </summary>
        private void RefreashHandle()
        {
            DateTime _lastRefreash = DateTime.MinValue;

            while (_server != null && _server.IsListener)
            {
                if (DateTime.Now.Subtract(_lastRefreash).TotalMinutes >= 5)
                {
                    _userMan.RefreashFirewall();
                    _lastRefreash = DateTime.Now;
                }
                Thread.Sleep(1000);
            }

        }
        private void Btn_Disconnect_Click(object sender, EventArgs e)
        {
            Btn_Connect.Enabled = true;
            Btn_Disconnect.Enabled = false;
            
            if (_server != null)
            {
                try
                {
                    _server.Stop();
                    
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    if (ShowError)
                    {
                        LogError(ex.ToString());
                    }
                }
            }
            if (_thdRefreash != null)
            {

                try
                {
                    _thdRefreash.StopThread();
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    LogError( ex.ToString());
                }
            }
            _server = null;
            _thdRefreash = null;
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            
            
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _userMan.RefreashFirewall();
            Log("更新到防火墙");
        }

        public bool OnUserUpdate()
        {
            _userMan.RefreashFirewall();
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_server!=null && _server.IsListener)
            {
                e.Cancel = true;
            }
        }

        FormWindowState _lastWindowState = FormWindowState.Normal;
        private void iconMenu_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = _lastWindowState;
            this.TopMost = true;
            this.TopMost = false;
            SetIconVisable();
        }
        /// <summary>
        /// 设置图标是否显示
        /// </summary>
        private void SetIconVisable()
        {
            iconMenu.Visible = !this.Visible;
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            
        }

        private void FrmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                SetIconVisable();
                return;
            }
            _lastWindowState = this.WindowState;
        }

        private void tsRePwd_Click(object sender, EventArgs e)
        {
            if (dgMembers.SelectedRows.Count <= 0)
            {
                return;
            }
            FWUser user = dgMembers.SelectedRows[0].DataBoundItem as FWUser;
            if (user == null)
            {
                return;
            }
            user.Secret = FWUser.CreateSecret();
            _userMan.SaveConfig();
            System.Windows.Forms.MessageBox.Show("重置完毕", "提示");
        }

        private void dgMembers_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tsNewUser_Click(object sender, EventArgs e)
        {
            using (FrmNewUser frm = new FrmNewUser())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    FWUser user = new FWUser();
                    user.UserName = frm.InputText;
                    user.Secret = FWUser.CreateSecret();
                    _userMan.AddUser(user);
                    _userMan.SaveConfig();
                    RefreashUser();
                }
            }
        }

        private void tsDelete_Click(object sender, EventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("是否删除此用户?", "问题", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            if (dgMembers.SelectedRows.Count <= 0)
            {
                return;
            }
            FWUser user = dgMembers.SelectedRows[0].DataBoundItem as FWUser;
            if (user == null)
            {
                return;
            }
            _userMan.AllUser.Remove(user);

            _userMan.SaveConfig();
            RefreashUser();
        }

        private void tsOpen_Click(object sender, EventArgs e)
        {
            string path = UserManager.BasePath ;
            Process.Start(path);
        }

        private void TsCopyUser_Click(object sender, EventArgs e)
        {
            if (dgMembers.SelectedRows.Count <= 0)
            {
                return;
            }
            FWUser user = dgMembers.SelectedRows[0].DataBoundItem as FWUser;
            if (user == null)
            {
                return;
            }
            FrmText.ShowText("用户:" + user.UserName + "的配置", user.ToJson());
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _userMan.SaveConfig();
        }

        private void chkAutoRun_Click(object sender, EventArgs e)
        {
            RegConfig.IsUserAutoRun = chkAutoRun.Checked;
        }
    }
    
}
