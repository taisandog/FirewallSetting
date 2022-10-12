using Buffalo.ArgCommon;
using Buffalo.DBTools;
using Buffalo.Kernel;
using Buffalo.Kernel.TreadPoolManager;
using SettingLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace FWSettingClient
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private BlockThread _thd;

        private List<FWUser> _curUser;
        private void FrmMain_Load(object sender, EventArgs e)
        {
            chkAuto.Checked = RegConfig.IsAutoRun;
            
            _curUser = FWUser.LoadConfig();
            dgUsers.AutoGenerateColumns = false;
            RefreashUser();
            StartAuto();
            SetTitle();
        }

        

        /// <summary>
        /// 设置标题
        /// </summary>
        private void SetTitle()
        {
            this.Text = ToolVersionInfo.GetToolVerInfo("防火墙白名单客户端", this.GetType().Assembly);
        }
        private void StartAuto()
        {
            _running = true;
            _thd = BlockThread.Create(DoAuto);
            _thd.StartThread();
        }
        private void StopAuto()
        {
            _running = false;
            try
            {
                _thd.StopThread();
            }
            catch { }
            _thd = null;
        }
        private bool _running = false;

        private  const int Sleep =  1000;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        DateTime _lastRun = DateTime.MinValue;
        private void DoAuto()
        {
            
            while (_running)
            {
                if (DateTime.Now.Subtract(_lastRun).TotalMinutes >= 5)
                {
                    UpdateIP();
                    _lastRun = DateTime.Now;
                }
                Thread.Sleep(Sleep);
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
        /// <summary>
        /// 更新IP
        /// </summary>
        private void UpdateIP()
        {
            
            Queue<FWUser> que = null;
            lock (_curUser)
            {
                que = new Queue<FWUser>(_curUser);
            }

            List<Task> lstTask = new List<Task>();
            foreach (FWUser user in que)
            {
                Task tsk = Task.Run(() =>
                {
                    DoUpdate(user);
                });
                lstTask.Add(tsk);
            }

            Task.WaitAll(lstTask.ToArray(), 10000);
            

        }

        private void DoUpdate(FWUser user) 
        {
            long tick = (long)CommonMethods.ConvertDateTimeInt(DateTime.Now, true, true);
            if (user == null) 
            {
                return;
            }
            APIResault res = null;
            string sign = null;
            if (user.V2 == "1")
            {
                res = user.Handle.GetIP();
                if (!res.IsSuccess)
                {
                    mess.LogError(user.Name + ":" + res.Message);
                    return;
                }
                string ip = res.GetValue<string>();
                sign = user.GetSignV2(tick, ip);
            }
            else
            {
                sign = user.GetSign(tick);
            }
            res = user.Handle.UpdateAddress(user.UserName, tick, sign);
            if (!res.IsSuccess)
            {
                mess.LogError(user.Name + ":" + res.Message);
                return ;
            }
            mess.Log(user.Name + ":" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  进行了IP同步");
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
           
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_running)
            {
                e.Cancel = true;
                this.Hide();
                SetIconVisable();
            }
            //StopAuto();
        }

        private void chkAuto_Click(object sender, EventArgs e)
        {
            RegConfig.IsAutoRun = chkAuto.Checked;
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
            SetIconVisable();
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

        private void TsSync_Click(object sender, EventArgs e)
        {
            UpdateIP();
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            FWUser user=FrmLoad.ShowLoad("请输入配置");
            if (user != null)
            {
                lock (_curUser)
                {
                    _curUser.Add(user);
                }
                FWUser.SaveConfig(_curUser);
                RefreashUser();
                UpdateIP();
            }
        }

        private void RefreashUser()
        {
            dgUsers.DataSource = null;
            dgUsers.DataSource = _curUser;
        }

        private void TsExit_Click(object sender, EventArgs e)
        {
            StopAuto();
            this.Close();
        }

        private void TsAdd_Click(object sender, EventArgs e)
        {
            BtnLoad_Click(btnLoad, e);
        }

        private void TsDelete_Click(object sender, EventArgs e)
        {
            if (dgUsers.SelectedRows.Count <= 0)
            {
                return;
            }
            DataGridViewRow row = dgUsers.SelectedRows[0];
            int index = row.Index;
            if (index < 0)
            {
                return;
            }
            lock (_curUser)
            {
                _curUser.RemoveAt(index);
            }
            FWUser.SaveConfig(_curUser);
            RefreashUser();
        }

        private void tsHide_Click(object sender, EventArgs e)
        {
            this.Hide();
            SetIconVisable();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            _lastRun = DateTime.MinValue;
        }
    }
}
