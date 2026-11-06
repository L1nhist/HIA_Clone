using HIA.ImportExportApp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HIA.ImportExportApp
{
    public partial class PathConfig : Form
    {
        public PathConfig()
        {
            InitializeComponent();
        }

        public void PatchValueToPathConfig()
        {
            if (string.IsNullOrWhiteSpace(Storages.UploadPath) || string.IsNullOrWhiteSpace(Storages.DownloadPath)) return;

            txtPathUpload.Text = Storages.UploadPath;
            txtPathDownload.Text = Storages.DownloadPath;
        }

        private void btnBrowerUpload_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtPathUpload.Text = folderBrowser.SelectedPath;
            }
        }

        private void btnBrowerDownload_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                txtPathDownload.Text = folderBrowser.SelectedPath;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPathUpload.Text) || string.IsNullOrWhiteSpace(txtPathDownload.Text))
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Storages.UploadPath = txtPathUpload.Text;
            Storages.DownloadPath = txtPathDownload.Text;

            MessageBox.Show("Cài đặt thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
