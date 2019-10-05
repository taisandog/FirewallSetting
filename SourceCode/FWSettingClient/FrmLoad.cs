using SettingLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FWSettingClient
{
    public partial class FrmLoad : Form
    {
        public FrmLoad()
        {
            InitializeComponent();
        }

        public static FWUser ShowLoad(string title)
        {
            using (FrmLoad frm = new FrmLoad())
            {
                
                frm.Text = title;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string json = frm.txtValue.Text;
                    try
                    {
                        return FWUser.LoadJson(json);
                    }catch(Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            return null;
        }

        private void FrmLoad_Load(object sender, EventArgs e)
        {

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtValue.Text))
            {
                MessageBox.Show("请输入配置");
                return;
            }
            this.DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }


    }
}
