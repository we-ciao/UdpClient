namespace DampingForwardUDP
{
    partial class Form1
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
            this.lstIP = new System.Windows.Forms.ListBox();
            this.txtMsg = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lstIndex = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lstAll = new System.Windows.Forms.ListBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstIP
            // 
            this.lstIP.FormattingEnabled = true;
            this.lstIP.ItemHeight = 12;
            this.lstIP.Location = new System.Drawing.Point(13, 355);
            this.lstIP.Name = "lstIP";
            this.lstIP.Size = new System.Drawing.Size(789, 88);
            this.lstIP.TabIndex = 2;
            // 
            // txtMsg
            // 
            this.txtMsg.Location = new System.Drawing.Point(13, 21);
            this.txtMsg.Name = "txtMsg";
            this.txtMsg.ReadOnly = true;
            this.txtMsg.Size = new System.Drawing.Size(789, 311);
            this.txtMsg.TabIndex = 1;
            this.txtMsg.Text = "";
            this.txtMsg.TextChanged += new System.EventHandler(this.TxtMsg_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.lstIndex);
            this.groupBox2.Location = new System.Drawing.Point(808, 21);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(131, 422);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "未接收的传感器";
            // 
            // lstIndex
            // 
            this.lstIndex.FormattingEnabled = true;
            this.lstIndex.ItemHeight = 12;
            this.lstIndex.Location = new System.Drawing.Point(6, 20);
            this.lstIndex.Name = "lstIndex";
            this.lstIndex.Size = new System.Drawing.Size(120, 400);
            this.lstIndex.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstAll);
            this.groupBox1.Location = new System.Drawing.Point(945, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(131, 422);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "已接收的传感器";
            // 
            // lstAll
            // 
            this.lstAll.FormattingEnabled = true;
            this.lstAll.ItemHeight = 12;
            this.lstAll.Location = new System.Drawing.Point(6, 20);
            this.lstAll.Name = "lstAll";
            this.lstAll.Size = new System.Drawing.Size(120, 400);
            this.lstAll.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1087, 456);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.txtMsg);
            this.Controls.Add(this.lstIP);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ListBox lstIP;
        private System.Windows.Forms.RichTextBox txtMsg;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ListBox lstIndex;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstAll;
    }
}

