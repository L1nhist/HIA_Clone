namespace HIA.ImportExportApp
{
    partial class PathConfig
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
            this.txtPathUpload = new System.Windows.Forms.TextBox();
            this.btnBrowerUpload = new System.Windows.Forms.Button();
            this.txtPathDownload = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnBrowerDownload = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Thư mục nhập";
            // 
            // txtPathUpload
            // 
            this.txtPathUpload.Location = new System.Drawing.Point(12, 37);
            this.txtPathUpload.Name = "txtPathUpload";
            this.txtPathUpload.Size = new System.Drawing.Size(530, 31);
            this.txtPathUpload.TabIndex = 1;
            // 
            // btnBrowerUpload
            // 
            this.btnBrowerUpload.Location = new System.Drawing.Point(538, 37);
            this.btnBrowerUpload.Name = "btnBrowerUpload";
            this.btnBrowerUpload.Size = new System.Drawing.Size(34, 32);
            this.btnBrowerUpload.TabIndex = 2;
            this.btnBrowerUpload.Text = "...";
            this.btnBrowerUpload.UseVisualStyleBackColor = true;
            this.btnBrowerUpload.Click += new System.EventHandler(this.btnBrowerUpload_Click);
            // 
            // txtPathDownload
            // 
            this.txtPathDownload.Location = new System.Drawing.Point(12, 118);
            this.txtPathDownload.Name = "txtPathDownload";
            this.txtPathDownload.Size = new System.Drawing.Size(530, 31);
            this.txtPathDownload.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 25);
            this.label2.TabIndex = 4;
            this.label2.Text = "Thư mục xuất";
            // 
            // btnBrowerDownload
            // 
            this.btnBrowerDownload.Location = new System.Drawing.Point(538, 118);
            this.btnBrowerDownload.Name = "btnBrowerDownload";
            this.btnBrowerDownload.Size = new System.Drawing.Size(34, 32);
            this.btnBrowerDownload.TabIndex = 5;
            this.btnBrowerDownload.Text = "...";
            this.btnBrowerDownload.UseVisualStyleBackColor = true;
            this.btnBrowerDownload.Click += new System.EventHandler(this.btnBrowerDownload_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(342, 173);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(112, 34);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Đồng ý";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(460, 173);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 34);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Hủy bỏ";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PathConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 230);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnBrowerDownload);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPathDownload);
            this.Controls.Add(this.btnBrowerUpload);
            this.Controls.Add(this.txtPathUpload);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PathConfig";
            this.Text = "Thư mục";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPathUpload;
        private System.Windows.Forms.Button btnBrowerUpload;
        private System.Windows.Forms.TextBox txtPathDownload;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBrowerDownload;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}