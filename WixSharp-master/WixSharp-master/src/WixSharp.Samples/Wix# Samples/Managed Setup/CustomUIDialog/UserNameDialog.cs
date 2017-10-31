using System;
using System.Drawing;
using System.Windows.Forms;
using WixSharp;
using WixSharp.UI.Forms;

namespace MyProduct
{
    public partial class UserNameDialog : ManagedForm, IManagedDialog
    {
        public UserNameDialog()
        {
            InitializeComponent();
        }

        void dialog_Load(object sender, EventArgs e)
        {
            banner.Image = MsiRuntime.Session.GetEmbeddedBitmap("WixUI_Bmp_Banner");

            name.Text = Defaults.UserName;
            password.Text = MsiRuntime.Session["PASSWORD"];

            localDomain.Checked = true;
        }

        void back_Click(object sender, EventArgs e)
        {
            Shell.GoPrev();
        }

        void next_Click(object sender, EventArgs e)
        {
            MsiRuntime.Session["PASSWORD"] = password.Text;
            MsiRuntime.Session["DOMAIN"] = domain.Text;

            Shell.GoNext();
        }

        void cancel_Click(object sender, EventArgs e)
        {
            Shell.Cancel();
        }

        void DomainType_CheckedChanged(object sender, EventArgs e)
        {
            if (localDomain.Checked)
                domain.Text = Environment.MachineName;
            else if (networkDomain.Checked)
                domain.Text = Environment.UserDomainName;

            UpdateEnabledStates();
        }

        void password_TextChanged(object sender, EventArgs e)
        {
            UpdateEnabledStates();
        }

        void UpdateEnabledStates()
        {
            domain.Enabled = networkDomain.Checked;
            next.Enabled = password.Text.IsNotEmpty();
        }
    }
}