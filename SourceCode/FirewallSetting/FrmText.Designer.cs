namespace FirewallSetting
{
    partial class FrmText
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtText = new System.Windows.Forms.TextBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtText
            // 
            this.txtText.BackColor = System.Drawing.Color.White;
            this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtText.Location = new System.Drawing.Point(0, 0);
            this.txtText.Multiline = true;
            this.txtText.Name = "txtText";
            this.txtText.ReadOnly = true;
            this.txtText.Size = new System.Drawing.Size(384, 96);
            this.txtText.TabIndex = 0;
            // 
            // btnCopy
            // 
            this.btnCopy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCopy.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.btnCopy.Location = new System.Drawing.Point(0, 96);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(384, 34);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "复制";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.BtnCopy_Click);
            // 
            // FrmText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 130);
            this.Controls.Add(this.txtText);
            this.Controls.Add(this.btnCopy);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmText";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FrmText";
            this.Load += new System.EventHandler(this.FrmText_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtText;
        private System.Windows.Forms.Button btnCopy;
    }
}