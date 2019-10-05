using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirewallSetting
{
    public partial class FrmText : Form
    {
        public FrmText()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 显示文本窗体
        /// </summary>
        /// <param name="title"></param>
        /// <param name="value"></param>
        public static void ShowText(string title,string value)
        {
            using (FrmText frm = new FrmText())
            {
                frm.txtText.Text = value;
                frm.Text = title;
                frm.ShowDialog();
            }
        }

        private void FrmText_Load(object sender, EventArgs e)
        {

        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtText.Text))
            {
                Clipboard.SetDataObject(txtText.Text);
            }
        }
    }
}
