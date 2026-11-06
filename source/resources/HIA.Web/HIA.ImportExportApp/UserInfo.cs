using HIA.ImportExportApp.Common;
using System.Windows.Forms;

namespace HIA.ImportExportApp
{
    public partial class UserInfo : Form
    {
        public UserInfo()
        {
            InitializeComponent();
        }

        public void PatchValueToForm()
        {
            txtEmail.Text = Storages.CurrentUser?.Email;
            txtFullname.Text = Storages.CurrentUser?.FullName;
            txtRoleName.Text = Storages.CurrentUser?.RoleName;
            txtHospitalName.Text = Storages.CurrentUser?.HospitalName;
            txtMaCSKCB.Text = Storages.CurrentUser?.MA_CSKCB;
        }
    }
}
