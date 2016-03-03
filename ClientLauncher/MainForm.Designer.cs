namespace ClientLauncher
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnRun = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pnlContent = new System.Windows.Forms.Panel();
            this.btnAccount = new System.Windows.Forms.Button();
            this.btnNews = new System.Windows.Forms.Button();
            this.pnlAccount = new System.Windows.Forms.Panel();
            this.btnRegister = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMail = new System.Windows.Forms.TextBox();
            this.txtRepeatPass = new System.Windows.Forms.TextBox();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.pnlNews = new System.Windows.Forms.Panel();
            this.rtNews = new System.Windows.Forms.RichTextBox();
            this.lblWaitText = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.pnlContent.SuspendLayout();
            this.pnlAccount.SuspendLayout();
            this.pnlNews.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(513, 49);
            this.panel1.TabIndex = 0;
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            this.panel1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.CausesValidation = false;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(164)))), ((int)(((byte)(141)))));
            this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(467, 49);
            this.label1.TabIndex = 1;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.UseCompatibleTextRendering = true;
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label1_MouseDown);
            this.label1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label1_MouseMove);
            this.label1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label1_MouseUp);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("DejaVu Sans", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(164)))), ((int)(((byte)(141)))));
            this.btnClose.Location = new System.Drawing.Point(481, 8);
            this.btnClose.Margin = new System.Windows.Forms.Padding(8);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(20, 19);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRun.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRun.Enabled = false;
            this.btnRun.FlatAppearance.BorderSize = 0;
            this.btnRun.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnRun.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnRun.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRun.Font = new System.Drawing.Font("Lucida Sans Unicode", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(164)))), ((int)(((byte)(141)))));
            this.btnRun.Location = new System.Drawing.Point(318, 207);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(183, 72);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = false;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 1000;
            this.toolTip1.ReshowDelay = 100;
            // 
            // pnlContent
            // 
            this.pnlContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pnlContent.Controls.Add(this.btnAccount);
            this.pnlContent.Controls.Add(this.btnNews);
            this.pnlContent.Controls.Add(this.pnlAccount);
            this.pnlContent.Controls.Add(this.pnlNews);
            this.pnlContent.Controls.Add(this.lblWaitText);
            this.pnlContent.Location = new System.Drawing.Point(12, 56);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Size = new System.Drawing.Size(297, 223);
            this.pnlContent.TabIndex = 7;
            // 
            // btnAccount
            // 
            this.btnAccount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnAccount.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnAccount.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.btnAccount.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAccount.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAccount.Location = new System.Drawing.Point(84, 3);
            this.btnAccount.Name = "btnAccount";
            this.btnAccount.Size = new System.Drawing.Size(75, 23);
            this.btnAccount.TabIndex = 4;
            this.btnAccount.Text = "Account";
            this.btnAccount.UseVisualStyleBackColor = false;
            this.btnAccount.Visible = false;
            this.btnAccount.Click += new System.EventHandler(this.btnAccount_Click);
            // 
            // btnNews
            // 
            this.btnNews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.btnNews.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnNews.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DimGray;
            this.btnNews.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Silver;
            this.btnNews.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNews.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNews.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnNews.Location = new System.Drawing.Point(3, 3);
            this.btnNews.Name = "btnNews";
            this.btnNews.Size = new System.Drawing.Size(75, 23);
            this.btnNews.TabIndex = 3;
            this.btnNews.Text = "News";
            this.btnNews.UseVisualStyleBackColor = false;
            this.btnNews.Click += new System.EventHandler(this.btnNews_Click);
            // 
            // pnlAccount
            // 
            this.pnlAccount.Controls.Add(this.btnRegister);
            this.pnlAccount.Controls.Add(this.label6);
            this.pnlAccount.Controls.Add(this.label5);
            this.pnlAccount.Controls.Add(this.label4);
            this.pnlAccount.Controls.Add(this.label3);
            this.pnlAccount.Controls.Add(this.txtMail);
            this.pnlAccount.Controls.Add(this.txtRepeatPass);
            this.pnlAccount.Controls.Add(this.txtPass);
            this.pnlAccount.Controls.Add(this.txtName);
            this.pnlAccount.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAccount.Location = new System.Drawing.Point(0, 0);
            this.pnlAccount.Name = "pnlAccount";
            this.pnlAccount.Size = new System.Drawing.Size(297, 223);
            this.pnlAccount.TabIndex = 5;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(212, 190);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(75, 23);
            this.btnRegister.TabIndex = 8;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label6.Location = new System.Drawing.Point(23, 159);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Email";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label5.Location = new System.Drawing.Point(23, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Repeat Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label4.Location = new System.Drawing.Point(23, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label3.Location = new System.Drawing.Point(23, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Name";
            // 
            // txtMail
            // 
            this.txtMail.Location = new System.Drawing.Point(129, 157);
            this.txtMail.Name = "txtMail";
            this.txtMail.Size = new System.Drawing.Size(158, 20);
            this.txtMail.TabIndex = 3;
            // 
            // txtRepeatPass
            // 
            this.txtRepeatPass.Location = new System.Drawing.Point(129, 121);
            this.txtRepeatPass.Name = "txtRepeatPass";
            this.txtRepeatPass.Size = new System.Drawing.Size(158, 20);
            this.txtRepeatPass.TabIndex = 2;
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(129, 85);
            this.txtPass.Name = "txtPass";
            this.txtPass.Size = new System.Drawing.Size(158, 20);
            this.txtPass.TabIndex = 1;
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(129, 49);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(158, 20);
            this.txtName.TabIndex = 0;
            // 
            // pnlNews
            // 
            this.pnlNews.Controls.Add(this.rtNews);
            this.pnlNews.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNews.Location = new System.Drawing.Point(0, 0);
            this.pnlNews.Name = "pnlNews";
            this.pnlNews.Size = new System.Drawing.Size(297, 223);
            this.pnlNews.TabIndex = 2;
            // 
            // rtNews
            // 
            this.rtNews.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtNews.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.rtNews.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtNews.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtNews.ForeColor = System.Drawing.SystemColors.MenuBar;
            this.rtNews.Location = new System.Drawing.Point(10, 39);
            this.rtNews.Margin = new System.Windows.Forms.Padding(10);
            this.rtNews.Name = "rtNews";
            this.rtNews.ReadOnly = true;
            this.rtNews.Size = new System.Drawing.Size(277, 174);
            this.rtNews.TabIndex = 0;
            this.rtNews.Text = "Test News\nBlabla";
            // 
            // lblWaitText
            // 
            this.lblWaitText.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWaitText.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblWaitText.Location = new System.Drawing.Point(10, 85);
            this.lblWaitText.Name = "lblWaitText";
            this.lblWaitText.Size = new System.Drawing.Size(277, 36);
            this.lblWaitText.TabIndex = 1;
            this.lblWaitText.Text = "No news available \r\n(or not able to connect to the server)";
            this.lblWaitText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.ClientSize = new System.Drawing.Size(513, 291);
            this.ControlBox = false;
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spellborn Launcher";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.panel1.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.pnlAccount.ResumeLayout(false);
            this.pnlAccount.PerformLayout();
            this.pnlNews.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Button btnAccount;
        private System.Windows.Forms.Button btnNews;
        private System.Windows.Forms.Panel pnlNews;
        private System.Windows.Forms.RichTextBox rtNews;
        private System.Windows.Forms.Panel pnlAccount;
        private System.Windows.Forms.Label lblWaitText;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtMail;
        private System.Windows.Forms.TextBox txtRepeatPass;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.TextBox txtName;
    }
}

