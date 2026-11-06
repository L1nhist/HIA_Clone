using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Windows.Forms;

namespace EgdbhClient
{
    public partial class Main : Form
    {
        private Timer timer1;
        private BindingSource bindingSource1 = new BindingSource();
        private static int SoHoSoDongBo = 0;

        public Main()
        {
            InitializeComponent();
            InitTimer();
            this.toolTip1.SetToolTip(this.button5, "Thu nhỏ chạy ẩn");
            string tokenn = Properties.Settings.Default.token;
            if (Properties.Settings.Default.token.Length == 0) { label2.Text = "Chưa đăng nhập."; }
            else
            {
                label2.Text = Properties.Settings.Default.userName;
                var jwt = Properties.Settings.Default.token;
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);
                //label11.Text = "Tài khoản: " + token.Claims.First(claim => claim.Type == "nameidentifier").Value;
                label11.Text = token.Claims.First(claim => claim.Type == "MA_CSKCB").Value;
                label1.Text = token.Claims.First(claim => claim.Type == "HospitalName").Value;
            }
            if (Properties.Settings.Default.inFolder.Length == 0 || Properties.Settings.Default.outFolder.Length == 0)
            { label4.Text = "Chưa cấu hình."; }
            else
            {
                if (Directory.Exists(Properties.Settings.Default.inFolder) && Directory.Exists(Properties.Settings.Default.outFolder))
                {
                    label8.Text = Directory.GetFiles(Properties.Settings.Default.inFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                    label9.Text = Directory.GetFiles(Properties.Settings.Default.outFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                    label4.Text = "Đã cấu hình thư mục nhập/xuất.";
                    if (Directory.Exists(Properties.Settings.Default.outFolder + "//Failed"))
                    {
                        label17.Text = Directory.GetFiles(Properties.Settings.Default.outFolder + "//Failed", "*.xml", SearchOption.AllDirectories).Length.ToString();

                    }
                }
            }

            inPath.Text = Properties.Settings.Default.inFolder;
            outPath.Text = Properties.Settings.Default.outFolder;



            // Populate the data source.
            //bindingSource1.Add(new HoSo(Title.SyncAgain, "20211104083403_HT338279705818538289_HLU01_1261_348459_BangKe01.XML", true));


            // Initialize the DataGridView.
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSize = true;
            dataGridView1.DataSource = bindingSource1;



            // Initialize and add a text box column.
            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "TenHoSo";
            column.Name = "Tên hồ sơ";
            column.Width = 379;
            dataGridView1.Columns.Add(column);

            // Initialize and add a check box column.
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Good";
            column.Name = "Tình trạng";
            column.Width = 100;
            dataGridView1.Columns.Add(column);

            //dataGridView1.Columns.Add(CreateComboBoxWithEnums());

            // Initialize the form.
            this.Controls.Add(dataGridView1);
            this.AutoSize = true;
            this.Text = "eGDBH - Tự động đồng bộ hồ sơ";

            var alo = rkApp.GetValue("EgdbhClient");

            if (rkApp.GetValue("EgdbhClient") == null)
            {
                // The value doesn't exist, the application is not set to run at startup
                checkBox1.Checked = false;
            }
            else
            {
                // The value exists, the application is set to run at startup
                checkBox1.Checked = true;
            }
        }
        DataGridViewComboBoxColumn CreateComboBoxWithEnums()
        {
            DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn();
            combo.DataSource = Enum.GetValues(typeof(Title));
            combo.DataPropertyName = "Action";
            combo.Name = "Action";
            return combo;
        }
        public void InitTimer()
        {
            timer1 = new Timer();

            timer1.Tick += new EventHandler(UploadFile);

            timer1.Interval = 3000; // in miliseconds
            timer1.Start();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized
            //hide it from the task bar
            //and show the system tray icon (represented by the NotifyIcon control)
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000);
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var loginForm = new LoginForm(this);
            loginForm.ShowDialog();
        }

        //string path = @"G:\xq\";
        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                inPath.Text = folderBrowser.SelectedPath;
                Properties.Settings.Default.inFolder = folderBrowser.SelectedPath;
                Properties.Settings.Default.Save();
                if (Properties.Settings.Default.inFolder.Length == 0 || Properties.Settings.Default.outFolder.Length == 0)
                { label4.Text = "Chưa cấu hình."; }
                else
                {
                    if (Directory.Exists(Properties.Settings.Default.inFolder) && Directory.Exists(Properties.Settings.Default.outFolder))
                    {
                        label8.Text = Directory.GetFiles(Properties.Settings.Default.inFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                        label9.Text = Directory.GetFiles(Properties.Settings.Default.outFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                        label4.Text = "Đã cấu hình thư mục nhập/xuất.";
                    }
                    if (Directory.Exists(Properties.Settings.Default.outFolder + "//Failed"))
                    {
                        label17.Text = Directory.GetFiles(Properties.Settings.Default.outFolder + "//Failed", "*.xml", SearchOption.AllDirectories).Length.ToString();

                    }
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                outPath.Text = folderBrowser.SelectedPath;
                Properties.Settings.Default.outFolder = folderBrowser.SelectedPath;
                Properties.Settings.Default.Save();

                if (Properties.Settings.Default.inFolder.Length == 0 || Properties.Settings.Default.outFolder.Length == 0)
                { label4.Text = "Chưa cấu hình."; }
                else
                {
                    if (Directory.Exists(Properties.Settings.Default.inFolder) && Directory.Exists(Properties.Settings.Default.outFolder))
                    {
                        label8.Text = Directory.GetFiles(Properties.Settings.Default.inFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                        label9.Text = Directory.GetFiles(Properties.Settings.Default.outFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                        label4.Text = "Đã cấu hình thư mục nhập/xuất.";
                        if (Directory.Exists(Properties.Settings.Default.outFolder + "//Failed"))
                        {
                            label17.Text = Directory.GetFiles(Properties.Settings.Default.outFolder + "//Failed", "*.xml", SearchOption.AllDirectories).Length.ToString();

                        }
                    }
                }
            }
        }
        private static Encoding GetEncodingHT(byte[] bom)
        {
            // Analyze the BOM

            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return Encoding.UTF32; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return new UTF32Encoding(true, true);  //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return Encoding.ASCII;
        }
        static HttpClient client = new HttpClient();

        static bool runFlag = true;


        private async void UploadFile(object sender, EventArgs e)
        {
            timer1.Stop();

            if (runFlag)
            {
                try
                {
                    if (Properties.Settings.Default.token.Length == 0 || Properties.Settings.Default.inFolder.Length == 0 || Properties.Settings.Default.outFolder.Length == 0)
                    {
                        timer1.Start(); return;
                    }
                    if (!Directory.Exists(Properties.Settings.Default.inFolder))
                    {
                        timer1.Start(); return;
                    }
                    if (!Directory.Exists(Properties.Settings.Default.outFolder))
                    {
                        timer1.Start(); return;
                    }
                    #region thử nghiệm cho đoạn lấy XML từ các SUB Directory
                    String[] subDir = Directory.GetDirectories(Properties.Settings.Default.inFolder);
                    foreach(var subD in subDir)
                    {
                        DirectoryInfo di = new DirectoryInfo(subD);


                        var xmlFiles = di.GetFiles("*.xml").Take(5);
                        if (xmlFiles.Count() <= 0)
                        {
                            timer1.Start(); continue;
                        }


                        FileUploadRequest FileUploadRequest = new FileUploadRequest();
                        FileUploadRequest.token = Properties.Settings.Default.token;
                        
                        FileUploadRequest.arrayFiles = new List<FileUpload>();


                        foreach (var file in xmlFiles)
                        {
                            BinaryReader b = new BinaryReader(File.Open(file.FullName, FileMode.Open));

                            FileUpload FileUpload = new FileUpload();
                            byte[] binData = b.ReadBytes((int)file.Length);

                            string base64EncodedExternalAccount = Convert.ToBase64String(binData);

                            FileUpload.fileName = file.Name;
                            FileUpload.fileBytes = base64EncodedExternalAccount;

                            FileUploadRequest.arrayFiles.Add(FileUpload);

                            b.Close();

                            //Linhist changed08/03/23
                            //Chuyen file sang out folder sau khi doc noi dung file
                            MoveToOutFolder(file.Name);
                        }

                        var loginBody = new StringContent(JsonConvert.SerializeObject(FileUploadRequest), Encoding.UTF8, "application/json");
                        var signInResult = await client.PostAsync($"https://egdbh.vn/Api/UploadFile", loginBody);
                        if (signInResult.IsSuccessStatusCode)
                        {

                            string loginData = await signInResult.Content.ReadAsStringAsync();
                            var plv = JsonConvert.DeserializeObject<LoginResponse>(loginData);

                            var dynamicObject = Json.Decode(loginData);
                            //List<string> failedFile = new List<string>();// (IEnumerable<string>)dynamicObject.data.failedFiles;
                            //var list = JsonConvert.DeserializeObject<List<dynamic>>(dynamicObject.data.failedFiles);
                            var failedFile = ((DynamicJsonArray)dynamicObject.data.failedFiles).OfType<string>().ToList();
                            //var list = fileFail.TryConvert(new SerializationBinder(<List<string>>));
                            //var alo = ((IEnumerable<Key>)fileFail).GetEnumerator();
                            //var alo = fileFail.OfType<string>();

                            if (plv.code == 200)
                            {
                                SoHoSoDongBo += xmlFiles.Count();
                                //thành công
                                foreach (var file in xmlFiles)
                                {
                                    if (!failedFile.Contains(file.Name))
                                    {
                                        //Linhist removed 08/03/23
                                        //Bo qua vi buoc nay da thuc hien o tren
                                        //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                                        //string destFile = System.IO.Path.Combine(Properties.Settings.Default.outFolder, file.Name);
                                        bindingSource1.Add(new HoSo(file.FullName, "Thành công"));
                                        dataGridView1.DataSource = bindingSource1;
                                        //System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                                        // To copy a file to another location and
                                        // overwrite the destination file if it already exists.

                                        //Linhist removed 08/03/23
                                        //Bo qua vi buoc nay da thuc hien o tren
                                        //if (File.Exists(destFile)) { File.Delete(destFile); }

                                        //System.IO.File.Move(sourceFile, destFile);
                                    }
                                    else
                                    {
                                        //Linhist changed 08/03/23
                                        //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                                        //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                                        //System.IO.Directory.CreateDirectory(desThatbai);

                                        //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                                        //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                                        ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                                        //// To copy a file to another location and
                                        //// overwrite the destination file if it already exists.
                                        //if (File.Exists(destFile)) { File.Delete(destFile); }

                                        //System.IO.File.Move(sourceFile, destFile);

                                        MoveToFailFolder(file.Name);

                                        bindingSource1.Add(new HoSo(file.FullName, "Thất bại"));
                                        dataGridView1.DataSource = bindingSource1;

                                    }
                                }
                            }
                            else
                            {
                                //Linhist changed 08/03/23
                                //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                                //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                                //System.IO.Directory.CreateDirectory(desThatbai);
                                foreach (var file in xmlFiles)
                                {
                                    //Linhist changed 08/03/23
                                    //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                                    //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                                    //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                                    ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                                    //// To copy a file to another location and
                                    //// overwrite the destination file if it already exists.
                                    //if (File.Exists(destFile)) { File.Delete(destFile); }

                                    //System.IO.File.Move(sourceFile, destFile);
                                    MoveToFailFolder(file.Name);

                                    bindingSource1.Add(new HoSo(file.FullName, "Thất bại"));
                                    dataGridView1.DataSource = bindingSource1;
                                }
                            }
                        }
                        else
                        {
                            //thất bại
                            //Linhist changed 08/03/23
                            //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                            //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                            //System.IO.Directory.CreateDirectory(desThatbai);
                            foreach (var file in xmlFiles)
                            {
                                //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                                //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                                ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                                //// To copy a file to another location and
                                //// overwrite the destination file if it already exists.
                                //if (File.Exists(destFile)) { File.Delete(destFile); }

                                //System.IO.File.Move(sourceFile, destFile);
                                MoveToFailFolder(file.Name);

                                bindingSource1.Add(new HoSo(file.FullName, "Thất bại"));
                                dataGridView1.DataSource = bindingSource1;
                            }

                        }
                    }

                    #endregion

                    //DirectoryInfo di = new DirectoryInfo(Properties.Settings.Default.inFolder);
                    

                    //var xmlFiles = di.GetFiles("*.xml").Take(5);
                    //if (xmlFiles.Count() <= 0)
                    //{
                    //    timer1.Start(); return;
                    //}


                    //FileUploadRequest FileUploadRequest = new FileUploadRequest();
                    //FileUploadRequest.token = Properties.Settings.Default.token;
                    //FileUploadRequest.arrayFiles = new List<FileUpload>();


                    //foreach (var file in xmlFiles)
                    //{
                    //    BinaryReader b = new BinaryReader(File.Open(file.FullName, FileMode.Open));

                    //    FileUpload FileUpload = new FileUpload();
                    //    byte[] binData = b.ReadBytes((int)file.Length);

                    //    string base64EncodedExternalAccount = Convert.ToBase64String(binData);

                    //    FileUpload.fileName = file.Name;
                    //    FileUpload.fileBytes = base64EncodedExternalAccount;

                    //    FileUploadRequest.arrayFiles.Add(FileUpload);

                    //    b.Close();

                    //    //Linhist changed08/03/23
                    //    //Chuyen file sang out folder sau khi doc noi dung file
                    //    MoveToOutFolder(file.Name);
                    //}

                    //var loginBody = new StringContent(JsonConvert.SerializeObject(FileUploadRequest), Encoding.UTF8, "application/json");
                    //var signInResult = await client.PostAsync($"https://egdbh.vn/Api/UploadFile", loginBody);
                    //if (signInResult.IsSuccessStatusCode)
                    //{

                    //    string loginData = await signInResult.Content.ReadAsStringAsync();
                    //    var plv = JsonConvert.DeserializeObject<LoginResponse>(loginData);

                    //    var dynamicObject = Json.Decode(loginData);
                    //    //List<string> failedFile = new List<string>();// (IEnumerable<string>)dynamicObject.data.failedFiles;
                    //    //var list = JsonConvert.DeserializeObject<List<dynamic>>(dynamicObject.data.failedFiles);
                    //    var failedFile = ((DynamicJsonArray)dynamicObject.data.failedFiles).OfType<string>().ToList();
                    //    //var list = fileFail.TryConvert(new SerializationBinder(<List<string>>));
                    //    //var alo = ((IEnumerable<Key>)fileFail).GetEnumerator();
                    //    //var alo = fileFail.OfType<string>();

                    //    if (plv.code == 200)
                    //    {
                    //        SoHoSoDongBo += xmlFiles.Count();
                    //        //thành công
                    //        foreach (var file in xmlFiles)
                    //        {
                    //            if (!failedFile.Contains(file.Name))
                    //            {
                    //                //Linhist removed 08/03/23
                    //                //Bo qua vi buoc nay da thuc hien o tren
                    //                //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                    //                //string destFile = System.IO.Path.Combine(Properties.Settings.Default.outFolder, file.Name);
                    //                bindingSource1.Add(new HoSo(file.FullName, "Thành công"));
                    //                dataGridView1.DataSource = bindingSource1;
                    //                //System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                    //                // To copy a file to another location and
                    //                // overwrite the destination file if it already exists.

                    //                //Linhist removed 08/03/23
                    //                //Bo qua vi buoc nay da thuc hien o tren
                    //                //if (File.Exists(destFile)) { File.Delete(destFile); }

                    //                //System.IO.File.Move(sourceFile, destFile);
                    //            }
                    //            else 
                    //            {
                    //                //Linhist changed 08/03/23
                    //                //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                    //                //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                    //                //System.IO.Directory.CreateDirectory(desThatbai);

                    //                //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                    //                //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                    //                ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                    //                //// To copy a file to another location and
                    //                //// overwrite the destination file if it already exists.
                    //                //if (File.Exists(destFile)) { File.Delete(destFile); }

                    //                //System.IO.File.Move(sourceFile, destFile);

                    //                MoveToFailFolder(file.Name);

                    //                bindingSource1.Add(new HoSo(file.FullName, "Thất bại"));
                    //                dataGridView1.DataSource = bindingSource1;

                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //Linhist changed 08/03/23
                    //        //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                    //        //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                    //        //System.IO.Directory.CreateDirectory(desThatbai);
                    //        foreach (var file in xmlFiles)
                    //        {
                    //            //Linhist changed 08/03/23
                    //            //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                    //            //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                    //            //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                    //            ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                    //            //// To copy a file to another location and
                    //            //// overwrite the destination file if it already exists.
                    //            //if (File.Exists(destFile)) { File.Delete(destFile); }

                    //            //System.IO.File.Move(sourceFile, destFile);
                    //            MoveToFailFolder(file.Name);

                    //            bindingSource1.Add(new HoSo(file.FullName, "Thất bại"));
                    //            dataGridView1.DataSource = bindingSource1;
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    //thất bại
                    //    //Linhist changed 08/03/23
                    //    //Chuyen file truc tiep tu out folder vao failed folder vif file da move luc dau
                    //    //string desThatbai = Properties.Settings.Default.outFolder + "//Failed";
                    //    //System.IO.Directory.CreateDirectory(desThatbai);
                    //    foreach (var file in xmlFiles)
                    //    {
                    //        //string sourceFile = System.IO.Path.Combine(Properties.Settings.Default.inFolder, file.Name);
                    //        //string destFile = System.IO.Path.Combine(desThatbai, file.Name);

                    //        ////System.IO.Directory.CreateDirectory(Properties.Settings.Default.outFolder);

                    //        //// To copy a file to another location and
                    //        //// overwrite the destination file if it already exists.
                    //        //if (File.Exists(destFile)) { File.Delete(destFile); }

                    //        //System.IO.File.Move(sourceFile, destFile);
                    //        MoveToFailFolder(file.Name);

                    //        bindingSource1.Add(new HoSo( file.FullName, "Thất bại"));
                    //        dataGridView1.DataSource = bindingSource1;
                    //    }

                    //}
                    
                    label16.Text = SoHoSoDongBo.ToString();
                }
                catch (Exception er)
                {
                    if (er != null)
                        richTextBox1.Text = er.InnerException?.ToString();
                    richTextBox1.ForeColor = Color.Red;
                }

            }

            if (Directory.Exists(Properties.Settings.Default.inFolder) && Directory.Exists(Properties.Settings.Default.outFolder))
            {
                label8.Text = Directory.GetFiles(Properties.Settings.Default.inFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                label9.Text = Directory.GetFiles(Properties.Settings.Default.outFolder, "*.xml", SearchOption.AllDirectories).Length.ToString();
                label4.Text = "Đã cấu hình thư mục nhập/xuất.";
                if (Directory.Exists(Properties.Settings.Default.outFolder + "//Failed"))
                {
                    label17.Text = Directory.GetFiles(Properties.Settings.Default.outFolder + "//Failed", "*.xml", SearchOption.AllDirectories).Length.ToString();

                }
            }

            timer1.Start();
        }

        private void MoveToOutFolder(string fileName)
        {
            string sourceFile = Path.Combine(Properties.Settings.Default.inFolder, fileName);
            string destFile = Path.Combine(Properties.Settings.Default.outFolder, fileName);
            if (File.Exists(destFile)) { File.Delete(destFile); }
            File.Move(sourceFile, destFile);
        }

        private void MoveToFailFolder(string fileName)
        {
            string failPath = Properties.Settings.Default.outFolder + "//Failed";
            if (!Directory.Exists(failPath))
            {
                Directory.CreateDirectory(failPath);
            }

            string sourceFile = Path.Combine(Properties.Settings.Default.outFolder, fileName);
            string destFile = Path.Combine(failPath, fileName);
            if (File.Exists(destFile)) { File.Delete(destFile); }
            File.Move(sourceFile, destFile);
        }

        //      {
        //"token": "string",
        //"arrayFiles": [
        //  {
        //    "fileName": "string",
        //    "fileBytes": "string"
        //  }
        //]
        public class FileUpload
        {
            [DataMember]
            public string fileName { get; set; }

            [DataMember]
            public string fileBytes { get; set; }

        }
        public class FileUploadRequest
        {
            [DataMember]
            public string token { get; set; }

            [DataMember]
            public List<FileUpload> arrayFiles { get; set; }

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            runFlag = false;
            label10.Text = "Dừng đồng bộ";
            label10.ForeColor = Color.Red;
        }


        private void button7_Click(object sender, EventArgs e)
        {
            runFlag = true;
            label10.Text = "Đang đồng bộ";
            label10.ForeColor = Color.Green;

        }

        private void label3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://egdbh.vn/");
        }
        public enum Title
        {
            Cancel,
            SyncAgain
        };
        private class HoSo
        {
            private string _tenHoSo;
            private string _good;
            

            public HoSo(string title, string good)
            {
                _good = good;
                _tenHoSo = title;
               
            }

            //public HoSo()
            //{
            //    hisTitle = Title.SyncAgain;
            //    tenHoSo = "<enter name>";
            //    good = true;
            //}

            public string TenHoSo
            {
                get
                {
                    return _tenHoSo;
                }

                set
                {
                    _tenHoSo = value;
                }
            }

            public string Good
            {
                get
                {
                    return _good;
                }

                set
                {
                    _good = value;
                }
            }
        }

        private void inPath_TextAlignChanged(object sender, EventArgs e)
        {
            //Properties.Settings.Default.outFolder = outPath.Text;
            //Properties.Settings.Default.Save();
            //if (!Directory.Exists(outPath.Text)) label4.Text = "Thư mục không tồn tại" ;

        }

        private void outPath_TextChanged(object sender, EventArgs e)
        {
            //Properties.Settings.Default.outFolder = inPath.Text;
            //Properties.Settings.Default.Save();
            //if (!Directory.Exists(inPath.Text)) label4.Text = "Thư mục không tồn tại";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.outFolder = "";
            Properties.Settings.Default.inFolder = "";
            Properties.Settings.Default.Save();
            inPath.Text = "";
            outPath.Text = "";
        }

        private void button7_Click_1(object sender, EventArgs e)
        {

            if (Directory.Exists(outPath.Text) && Directory.Exists(inPath.Text))
            {
                Properties.Settings.Default.outFolder = outPath.Text;
                Properties.Settings.Default.inFolder = inPath.Text;
                Properties.Settings.Default.Save();
                MessageBox.Show("Lưu cấu hình thành công !", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Thư mục không tồn tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://egdbh.vn/");
        }
        // The path to the key where Windows looks for startup applications
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue("EgdbhClient", Application.ExecutablePath);
            }
            else
            {
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue("EgdbhClient", false);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
    }


}
