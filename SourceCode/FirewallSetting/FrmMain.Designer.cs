namespace FirewallSetting
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.gbMain = new System.Windows.Forms.GroupBox();
            this.dgMembers = new System.Windows.Forms.DataGridView();
            this.ColUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColMultipleIP = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.cmUser = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsNewUser = new System.Windows.Forms.ToolStripMenuItem();
            this.tsRePwd = new System.Windows.Forms.ToolStripMenuItem();
            this.tsCopyUser = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgRules = new System.Windows.Forms.DataGridView();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Btn_Disconnect = new System.Windows.Forms.Button();
            this.Btn_Connect = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.chkAutoRun = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.iconMenu = new System.Windows.Forms.NotifyIcon(this.components);
            this.mbDisplay = new Library.MessageBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtClient = new System.Windows.Forms.TextBox();
            this.gbMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgMembers)).BeginInit();
            this.cmUser.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRules)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbMain
            // 
            this.gbMain.Controls.Add(this.dgMembers);
            this.gbMain.Controls.Add(this.groupBox1);
            this.gbMain.Controls.Add(this.panel1);
            this.gbMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbMain.Location = new System.Drawing.Point(0, 0);
            this.gbMain.Name = "gbMain";
            this.gbMain.Size = new System.Drawing.Size(956, 300);
            this.gbMain.TabIndex = 1;
            this.gbMain.TabStop = false;
            this.gbMain.Text = "信息";
            // 
            // dgMembers
            // 
            this.dgMembers.AllowUserToAddRows = false;
            this.dgMembers.AllowUserToResizeRows = false;
            this.dgMembers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgMembers.BackgroundColor = System.Drawing.Color.White;
            this.dgMembers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgMembers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColUserName,
            this.ColIP,
            this.ColMultipleIP});
            this.dgMembers.ContextMenuStrip = this.cmUser;
            this.dgMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgMembers.Location = new System.Drawing.Point(3, 25);
            this.dgMembers.MultiSelect = false;
            this.dgMembers.Name = "dgMembers";
            this.dgMembers.RowHeadersVisible = false;
            this.dgMembers.RowTemplate.Height = 23;
            this.dgMembers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgMembers.Size = new System.Drawing.Size(757, 204);
            this.dgMembers.TabIndex = 1;
            // 
            // ColUserName
            // 
            this.ColUserName.DataPropertyName = "UserName";
            this.ColUserName.FillWeight = 76.99991F;
            this.ColUserName.HeaderText = "名字";
            this.ColUserName.Name = "ColUserName";
            this.ColUserName.ReadOnly = true;
            // 
            // ColIP
            // 
            this.ColIP.DataPropertyName = "IPText";
            this.ColIP.FillWeight = 123.8578F;
            this.ColIP.HeaderText = "IP";
            this.ColIP.Name = "ColIP";
            this.ColIP.ReadOnly = true;
            // 
            // ColMultipleIP
            // 
            this.ColMultipleIP.DataPropertyName = "MultipleIP";
            this.ColMultipleIP.FillWeight = 43.14219F;
            this.ColMultipleIP.HeaderText = "多IP";
            this.ColMultipleIP.Name = "ColMultipleIP";
            this.ColMultipleIP.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // cmUser
            // 
            this.cmUser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsNewUser,
            this.tsRePwd,
            this.tsCopyUser,
            this.toolStripMenuItem1,
            this.tsDelete,
            this.tsOpen});
            this.cmUser.Name = "cmUser";
            this.cmUser.Size = new System.Drawing.Size(204, 120);
            // 
            // tsNewUser
            // 
            this.tsNewUser.Name = "tsNewUser";
            this.tsNewUser.Size = new System.Drawing.Size(203, 22);
            this.tsNewUser.Text = "新增用户";
            this.tsNewUser.Click += new System.EventHandler(this.tsNewUser_Click);
            // 
            // tsRePwd
            // 
            this.tsRePwd.Name = "tsRePwd";
            this.tsRePwd.Size = new System.Drawing.Size(203, 22);
            this.tsRePwd.Text = "重置密码";
            this.tsRePwd.Click += new System.EventHandler(this.tsRePwd_Click);
            // 
            // tsCopyUser
            // 
            this.tsCopyUser.Name = "tsCopyUser";
            this.tsCopyUser.Size = new System.Drawing.Size(203, 22);
            this.tsCopyUser.Text = "复制此用户配置";
            this.tsCopyUser.Click += new System.EventHandler(this.TsCopyUser_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(200, 6);
            // 
            // tsDelete
            // 
            this.tsDelete.Name = "tsDelete";
            this.tsDelete.Size = new System.Drawing.Size(203, 22);
            this.tsDelete.Text = "删除";
            this.tsDelete.Click += new System.EventHandler(this.tsDelete_Click);
            // 
            // tsOpen
            // 
            this.tsOpen.Name = "tsOpen";
            this.tsOpen.Size = new System.Drawing.Size(203, 22);
            this.tsOpen.Text = "配置文件(userInfo.xml)";
            this.tsOpen.Click += new System.EventHandler(this.tsOpen_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dgRules);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.groupBox1.Location = new System.Drawing.Point(760, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(193, 204);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "已加载规则";
            // 
            // dgRules
            // 
            this.dgRules.AllowUserToAddRows = false;
            this.dgRules.AllowUserToResizeRows = false;
            this.dgRules.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgRules.BackgroundColor = System.Drawing.Color.White;
            this.dgRules.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRules.ColumnHeadersVisible = false;
            this.dgRules.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.ColNumber});
            this.dgRules.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgRules.Location = new System.Drawing.Point(3, 25);
            this.dgRules.MultiSelect = false;
            this.dgRules.Name = "dgRules";
            this.dgRules.ReadOnly = true;
            this.dgRules.RowHeadersVisible = false;
            this.dgRules.RowTemplate.Height = 23;
            this.dgRules.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgRules.Size = new System.Drawing.Size(187, 176);
            this.dgRules.TabIndex = 2;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Name";
            this.dataGridViewTextBoxColumn1.FillWeight = 169.5432F;
            this.dataGridViewTextBoxColumn1.HeaderText = "名字";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // ColNumber
            // 
            this.ColNumber.DataPropertyName = "RuleCount";
            this.ColNumber.FillWeight = 30.45686F;
            this.ColNumber.HeaderText = "个数";
            this.ColNumber.Name = "ColNumber";
            this.ColNumber.ReadOnly = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Btn_Disconnect);
            this.panel1.Controls.Add(this.Btn_Connect);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(3, 229);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(950, 68);
            this.panel1.TabIndex = 0;
            // 
            // Btn_Disconnect
            // 
            this.Btn_Disconnect.Enabled = false;
            this.Btn_Disconnect.Location = new System.Drawing.Point(507, 6);
            this.Btn_Disconnect.Name = "Btn_Disconnect";
            this.Btn_Disconnect.Size = new System.Drawing.Size(113, 47);
            this.Btn_Disconnect.TabIndex = 13;
            this.Btn_Disconnect.Text = "停止服务器";
            this.Btn_Disconnect.UseVisualStyleBackColor = true;
            this.Btn_Disconnect.Click += new System.EventHandler(this.Btn_Disconnect_Click);
            // 
            // Btn_Connect
            // 
            this.Btn_Connect.Location = new System.Drawing.Point(388, 6);
            this.Btn_Connect.Name = "Btn_Connect";
            this.Btn_Connect.Size = new System.Drawing.Size(113, 47);
            this.Btn_Connect.TabIndex = 12;
            this.Btn_Connect.Text = "启动服务器";
            this.Btn_Connect.UseVisualStyleBackColor = true;
            this.Btn_Connect.Click += new System.EventHandler(this.Btn_Connect_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(9, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(149, 47);
            this.button2.TabIndex = 1;
            this.button2.Text = "重新应用到防火墙";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.txtClient);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.chkAutoRun);
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(632, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(318, 68);
            this.panel2.TabIndex = 1;
            // 
            // chkAutoRun
            // 
            this.chkAutoRun.AutoSize = true;
            this.chkAutoRun.Location = new System.Drawing.Point(216, 4);
            this.chkAutoRun.Name = "chkAutoRun";
            this.chkAutoRun.Size = new System.Drawing.Size(93, 25);
            this.chkAutoRun.TabIndex = 1;
            this.chkAutoRun.Text = "开机启动";
            this.chkAutoRun.UseVisualStyleBackColor = true;
            this.chkAutoRun.Click += new System.EventHandler(this.chkAutoRun_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(71, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(97, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存配置";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // iconMenu
            // 
            this.iconMenu.Icon = ((System.Drawing.Icon)(resources.GetObject("iconMenu.Icon")));
            this.iconMenu.Text = "防火墙白名单服务端";
            this.iconMenu.Visible = true;
            this.iconMenu.DoubleClick += new System.EventHandler(this.iconMenu_DoubleClick);
            // 
            // mbDisplay
            // 
            this.mbDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mbDisplay.Font = new System.Drawing.Font("宋体", 10F);
            this.mbDisplay.Location = new System.Drawing.Point(0, 300);
            this.mbDisplay.Name = "mbDisplay";
            this.mbDisplay.ShowError = true;
            this.mbDisplay.ShowLog = true;
            this.mbDisplay.ShowWarning = true;
            this.mbDisplay.Size = new System.Drawing.Size(956, 244);
            this.mbDisplay.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 21);
            this.label1.TabIndex = 14;
            this.label1.Text = "客户端:";
            // 
            // txtClient
            // 
            this.txtClient.BackColor = System.Drawing.Color.White;
            this.txtClient.Location = new System.Drawing.Point(71, 35);
            this.txtClient.Name = "txtClient";
            this.txtClient.ReadOnly = true;
            this.txtClient.Size = new System.Drawing.Size(238, 29);
            this.txtClient.TabIndex = 15;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 544);
            this.Controls.Add(this.mbDisplay);
            this.Controls.Add(this.gbMain);
            this.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FrmMain";
            this.Text = "防火墙设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.SizeChanged += new System.EventHandler(this.FrmMain_SizeChanged);
            this.gbMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgMembers)).EndInit();
            this.cmUser.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRules)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Library.MessageBox mbDisplay;
        private System.Windows.Forms.GroupBox gbMain;
        private System.Windows.Forms.DataGridView dgMembers;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button Btn_Disconnect;
        private System.Windows.Forms.Button Btn_Connect;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dgRules;
        private System.Windows.Forms.NotifyIcon iconMenu;
        private System.Windows.Forms.ContextMenuStrip cmUser;
        private System.Windows.Forms.ToolStripMenuItem tsRePwd;
        private System.Windows.Forms.ToolStripMenuItem tsDelete;
        private System.Windows.Forms.ToolStripMenuItem tsNewUser;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem tsOpen;
        private System.Windows.Forms.ToolStripMenuItem tsCopyUser;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColUserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColIP;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColMultipleIP;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColNumber;
        private System.Windows.Forms.CheckBox chkAutoRun;
        private System.Windows.Forms.TextBox txtClient;
        private System.Windows.Forms.Label label1;
    }
}

