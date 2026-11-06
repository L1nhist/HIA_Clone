namespace HIA.ImportExportApp
{
    partial class UserInfo
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFullname = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtRoleName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtHospitalName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMaCSKCB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Email";
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(156, 6);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.ReadOnly = true;
            this.txtEmail.Size = new System.Drawing.Size(404, 31);
            this.txtEmail.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "Họ và tên";
            // 
            // txtFullname
            // 
            this.txtFullname.Location = new System.Drawing.Point(156, 47);
            this.txtFullname.Name = "txtFullname";
            this.txtFullname.ReadOnly = true;
            this.txtFullname.Size = new System.Drawing.Size(404, 31);
            this.txtFullname.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 96);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 25);
            this.label3.TabIndex = 4;
            this.label3.Text = "Vai trò";
            // 
            // txtRoleName
            // 
            this.txtRoleName.Location = new System.Drawing.Point(156, 90);
            this.txtRoleName.Name = "txtRoleName";
            this.txtRoleName.ReadOnly = true;
            this.txtRoleName.Size = new System.Drawing.Size(404, 31);
            this.txtRoleName.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 25);
            this.label4.TabIndex = 6;
            this.label4.Text = "Tên bệnh viện";
            // 
            // txtHospitalName
            // 
            this.txtHospitalName.Location = new System.Drawing.Point(156, 134);
            this.txtHospitalName.Name = "txtHospitalName";
            this.txtHospitalName.ReadOnly = true;
            this.txtHospitalName.Size = new System.Drawing.Size(404, 31);
            this.txtHospitalName.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 184);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 25);
            this.label5.TabIndex = 8;
            this.label5.Text = "Ma MA_CSKCB";
            // 
            // txtMaCSKCB
            // 
            this.txtMaCSKCB.Location = new System.Drawing.Point(156, 178);
            this.txtMaCSKCB.Name = "txtMaCSKCB";
            this.txtMaCSKCB.ReadOnly = true;
            this.txtMaCSKCB.Size = new System.Drawing.Size(404, 31);
            this.txtMaCSKCB.TabIndex = 9;
            // 
            // UserInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 224);
            this.Controls.Add(this.txtMaCSKCB);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtHospitalName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtRoleName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtFullname);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserInfo";
            this.Text = "Thông tin người dùng";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFullname;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtRoleName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtHospitalName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMaCSKCB;
    }
}