using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 抗浮计算书
{
    /// <summary>启动登录窗口：游客进入 / 手机号登录 / 注册 / 忘记密码。取代原打赏入口。</summary>
    internal sealed class LoginForm : Form
    {
        private static readonly Regex PhoneRe = new Regex(@"^1\d{10}$");
        private static readonly Regex EmailRe = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        private readonly Panel pnlLogin = new Panel();
        private readonly Panel pnlSignup = new Panel();
        private readonly Label lblStatus = new Label();

        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtPass = new TextBox();
        private readonly TextBox txtSPhone = new TextBox();
        private readonly TextBox txtSEmail = new TextBox();
        private readonly TextBox txtSPass = new TextBox();

        public LoginForm()
        {
            Text = "登录 - 单建地下室整体抗浮计算";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(340, 380);
            Font = new Font("Microsoft YaHei", 9f);

            var title = new Label { Text = "欢迎使用抗浮计算", Left = 20, Top = 14, AutoSize = true, Font = new Font("Microsoft YaHei", 13f, FontStyle.Bold) };
            var hint = new Label { Text = "登录后可导出计算书；游客可试用全部计算功能。", Left = 20, Top = 42, AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(title); Controls.Add(hint);

            BuildLoginPanel();
            BuildSignupPanel();
            pnlSignup.Visible = false;
            Controls.Add(pnlLogin); Controls.Add(pnlSignup);

            lblStatus.Left = 20; lblStatus.Top = 300; lblStatus.Width = 300; lblStatus.Height = 60;
            lblStatus.ForeColor = Color.Firebrick;
            Controls.Add(lblStatus);
        }

        private void BuildLoginPanel()
        {
            pnlLogin.Location = new Point(0, 70);
            pnlLogin.Size = new Size(340, 220);

            pnlLogin.Controls.Add(MakeLabel("手机号", 20, 12));
            txtPhone.SetBounds(20, 32, 300, 25); txtPhone.MaxLength = 11;
            pnlLogin.Controls.Add(txtPhone);

            pnlLogin.Controls.Add(MakeLabel("密码", 20, 64));
            txtPass.SetBounds(20, 84, 300, 25); txtPass.UseSystemPasswordChar = true;
            pnlLogin.Controls.Add(txtPass);

            var btnLogin = MakeButton("登 录", 20, 122, 140); btnLogin.BackColor = Color.FromArgb(31, 111, 235); btnLogin.ForeColor = Color.White;
            btnLogin.Click += async (s, e) => await DoLogin();
            pnlLogin.Controls.Add(btnLogin);

            var btnForgot = MakeButton("忘记密码", 180, 122, 140); btnForgot.Click += async (s, e) => await DoForgot();
            pnlLogin.Controls.Add(btnForgot);

            var btnToSignup = MakeButton("注册新用户", 20, 158, 140); btnToSignup.Click += (s, e) => ShowPanel(true);
            pnlLogin.Controls.Add(btnToSignup);

            var btnGuest = MakeButton("以游客身份进入（不能导出）", 180, 158, 140); btnGuest.Click += (s, e) => { AppSession.SetGuest(); DialogResult = DialogResult.OK; Close(); };
            pnlLogin.Controls.Add(btnGuest);
        }

        private void BuildSignupPanel()
        {
            pnlSignup.Location = new Point(0, 70);
            pnlSignup.Size = new Size(340, 220);

            pnlSignup.Controls.Add(MakeLabel("手机号（作为登录用户名）", 20, 6));
            txtSPhone.SetBounds(20, 26, 300, 25); txtSPhone.MaxLength = 11;
            pnlSignup.Controls.Add(txtSPhone);

            pnlSignup.Controls.Add(MakeLabel("邮箱（用于找回密码）", 20, 56));
            txtSEmail.SetBounds(20, 76, 300, 25);
            pnlSignup.Controls.Add(txtSEmail);

            pnlSignup.Controls.Add(MakeLabel("设置密码（至少 6 位）", 20, 106));
            txtSPass.SetBounds(20, 126, 300, 25); txtSPass.UseSystemPasswordChar = true;
            pnlSignup.Controls.Add(txtSPass);

            var btnSignup = MakeButton("提交注册", 20, 162, 140); btnSignup.BackColor = Color.FromArgb(31, 111, 235); btnSignup.ForeColor = Color.White;
            btnSignup.Click += async (s, e) => await DoSignup();
            pnlSignup.Controls.Add(btnSignup);

            var btnBack = MakeButton("返回登录", 180, 162, 140); btnBack.Click += (s, e) => ShowPanel(false);
            pnlSignup.Controls.Add(btnBack);
        }

        private void ShowPanel(bool signup)
        {
            pnlLogin.Visible = !signup;
            pnlSignup.Visible = signup;
            lblStatus.Text = "";
        }

        private static Label MakeLabel(string text, int x, int y)
            => new Label { Text = text, Left = x, Top = y, AutoSize = true, ForeColor = Color.DimGray };

        private static Button MakeButton(string text, int x, int y, int w)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 30, FlatStyle = FlatStyle.Flat };
            b.FlatAppearance.BorderColor = Color.FromArgb(208, 215, 222);
            return b;
        }

        private async Task DoLogin()
        {
            string phone = txtPhone.Text.Trim(), pass = txtPass.Text;
            if (!PhoneRe.IsMatch(phone)) { Fail("请输入 11 位手机号。"); return; }
            if (string.IsNullOrEmpty(pass)) { Fail("请输入密码。"); return; }
            lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "登录中…";
            var r = await SupabaseClient.Login(phone, pass);
            if (r.Success) { AppSession.SetLoggedIn(phone, r.Email, r.AccessToken); DialogResult = DialogResult.OK; Close(); }
            else Fail(r.Message);
        }

        private async Task DoSignup()
        {
            string phone = txtSPhone.Text.Trim(), email = txtSEmail.Text.Trim(), pass = txtSPass.Text;
            if (!PhoneRe.IsMatch(phone)) { Fail("请输入 11 位手机号。"); return; }
            if (!EmailRe.IsMatch(email)) { Fail("请输入正确的邮箱。"); return; }
            if (pass.Length < 6) { Fail("密码至少 6 位。"); return; }
            lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "注册中…";
            var r = await SupabaseClient.Signup(phone, email, pass);
            if (r.Success) { AppSession.SetLoggedIn(phone, r.Email, r.AccessToken); DialogResult = DialogResult.OK; Close(); }
            else Fail(r.Message);
        }

        private async Task DoForgot()
        {
            string phone = txtPhone.Text.Trim();
            if (!PhoneRe.IsMatch(phone)) { Fail("请先在手机号栏输入已注册的手机号。"); return; }
            lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "查询并发送中…";
            var email = await SupabaseClient.LookupEmailByPhone(phone);
            if (string.IsNullOrEmpty(email)) { Fail("该手机号未注册。"); return; }
            var r = await SupabaseClient.Recover(email!);
            lblStatus.ForeColor = r.Success ? Color.FromArgb(26, 127, 55) : Color.Firebrick;
            lblStatus.Text = r.Message;
        }

        private void Fail(string msg) { lblStatus.ForeColor = Color.Firebrick; lblStatus.Text = msg; }
    }
}
