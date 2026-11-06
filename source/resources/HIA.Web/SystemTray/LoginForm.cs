using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace EgdbhClient
{
    public partial class LoginForm : Form
    {
        Main _main;
        public LoginForm(Main main)
        {
            InitializeComponent();
            txtEmail.Text = Properties.Settings.Default.userName;
            txtPassword.Text = Properties.Settings.Default.password;
            _main = main;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        static HttpClient client = new HttpClient();

        // Put the following code where you want to initialize the class
        // It can be the static constructor or a one-time initializer

        private async void btnLogin_Click(object sender, EventArgs e)
        {
           
            //To set the property: 
            Properties.Settings.Default.userName = txtEmail.Text;
            Properties.Settings.Default.password = txtPassword.Text;
            Properties.Settings.Default.Save();


            var loginBody = new StringContent(JsonConvert.SerializeObject(new
            {
                email = txtEmail.Text,
                password = txtPassword.Text
            }), Encoding.UTF8, "application/json");
            var signInResult = await client.PostAsync($"https://egdbh.vn/Api/SignIn", loginBody);
            if (signInResult.IsSuccessStatusCode)
            {

                string loginData = await signInResult.Content.ReadAsStringAsync();
                var plv = JsonConvert.DeserializeObject<LoginResponse>(loginData);
                if (plv.code == 200)
                {
                    Properties.Settings.Default.token = plv.token;
                    Properties.Settings.Default.Save();
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(plv.token);
                    //label11.Text = "Tài khoản: " + token.Claims.First(claim => claim.Type == "nameidentifier").Value;
                    _main.label11.Text = token.Claims.First(claim => claim.Type == "MA_CSKCB").Value;
                    _main.label1.Text = token.Claims.First(claim => claim.Type == "HospitalName").Value;
                    _main.label2.Text = "Đăng nhập thành công";
                    MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();

                }
                else
                {
                    MessageBox.Show(plv.message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Đăng nhập không thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }


            //To get property: 
            //string _token = "";
            //string serviceUrl = "https://egdbh.vn/";
            //using (var client = new HttpClient())
            //{
            //    client.BaseAddress = new Uri(serviceUrl);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //    if (_token == "")
            //    {

            //        string username = txtEmail.Text;
            //        string password = txtPassword.Text; //to4bhyt123
            //        ApiToken input = new ApiToken
            //        {
            //            username = username,
            //            password = password
            //        };
            //        var values = new Dictionary<string, string>
            //    {
            //        { "email", username },
            //        { "password", password }
            //    };
            //        var content = new FormUrlEncodedContent(values);



            //        HttpResponseMessage response = client.PostAsync("Api/SignIn", content).Result;
            //        if (response.IsSuccessStatusCode)

            //        {
            //            string KQ = await response.Content.ReadAsStringAsync();
            //            LoginResponse plv = (LoginResponse)JsonConvert.DeserializeObject<LoginResponse>(KQ);
            //            //response.Content.ReadAsStringAsync<KQPhienLamViec>().Result;
            //            _token = plv.token;


            //        }
            //    }

            //    string data2 = _token;
            //}
            //if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            //{
            //    MessageBox.Show("Vui lòng nhập email và mật khẩu", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //Regex rgxEmail = new Regex(@$"{RegexValidation.REGEX_EMAIL}");
            //if (!rgxEmail.IsMatch(email))
            //{
            //    MessageBox.Show("Email không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //Regex rgxPass = new Regex(@$"{RegexValidation.REGEX_PASSWORD}");
            //if (!rgxPass.IsMatch(password))
            //{
            //    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //var res = await _authService.SignInWinForm(new SignInModel { Email = email, Password = password });
            //if (res.Code != HttpStatusCode.OK)
            //{
            //    MessageBox.Show(res.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

            //MessageBox.Show("Đăng nhập thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //Storages.CurrentUser = res.Data;

        }
    }
    public class ApiToken
    {
        [DataMember]
        public string email { get; set; }

        [DataMember]
        public string password { get; set; }

    }
    public class LoginResponse
    {
        [DataMember]
        public int code { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string token { get; set; }

        [DataMember]
        public string expiredAt { get; set; }


    }
}
