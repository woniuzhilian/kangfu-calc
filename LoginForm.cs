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

        // 登录面板
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtPass = new TextBox();
        private readonly TextBox txtForgotEmail = new TextBox();
        private readonly Label lblForgotEmail = new Label { Text = "绑定邮箱（用于找回密码）", Left = 20, Top = 108, AutoSize = true, ForeColor = Color.DimGray };
        private Button btnForgot = null!;
        private bool forgotMode;
        // 注册面板
        private readonly TextBox txtSPhone = new TextBox();
        private readonly TextBox txtSEmail = new TextBox();
        private readonly TextBox txtSPass = new TextBox();
        private readonly TextBox txtSConfirm = new TextBox();

        public LoginForm()
        {
            Text = "登录 - 单建地下室整体抗浮计算";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(340, 420);
            Font = new Font("Microsoft YaHei", 9f);

            var title = new Label { Text = "欢迎使用抗浮计算", Left = 20, Top = 14, AutoSize = true, Font = new Font("Microsoft YaHei", 13f, FontStyle.Bold) };
            var hint = new Label { Text = "登录后可导出计算书；游客可试用全部计算功能。", Left = 20, Top = 42, AutoSize = true, ForeColor = Color.Gray };
            Controls.Add(title); Controls.Add(hint);

            BuildLoginPanel();
            BuildSignupPanel();
            pnlSignup.Visible = false;
            Controls.Add(pnlLogin); Controls.Add(pnlSignup);

            lblStatus.Left = 20; lblStatus.Top = 356; lblStatus.Width = 300; lblStatus.Height = 58;
            lblStatus.ForeColor = Color.Firebrick;
            Controls.Add(lblStatus);
        }

        private void BuildLoginPanel()
        {
            pnlLogin.Location = new Point(0, 70);
            pnlLogin.Size = new Size(340, 280);

            pnlLogin.Controls.Add(MakeLabel("手机号", 20, 8));
            txtPhone.SetBounds(20, 28, 300, 25); txtPhone.MaxLength = 11;
            pnlLogin.Controls.Add(txtPhone);

            pnlLogin.Controls.Add(MakeLabel("密码", 20, 58));
            txtPass.SetBounds(20, 78, 300, 25); txtPass.UseSystemPasswordChar = true;
            pnlLogin.Controls.Add(txtPass);

            pnlLogin.Controls.Add(lblForgotEmail);
            lblForgotEmail.Visible = false;
            txtForgotEmail.SetBounds(20, 128, 300, 25);
            pnlLogin.Controls.Add(txtForgotEmail);
            txtForgotEmail.Visible = false;

            var btnLogin = MakeButton("登 录", 20, 164, 140); btnLogin.BackColor = Color.FromArgb(31, 111, 235); btnLogin.ForeColor = Color.White;
            btnLogin.Click += async (s, e) => await DoLogin();
            pnlLogin.Controls.Add(btnLogin);

            btnForgot = MakeButton("忘记密码", 180, 164, 140); btnForgot.Click += async (s, e) => await btnForgot_Click();
            pnlLogin.Controls.Add(btnForgot);

            var btnToSignup = MakeButton("注册新用户", 20, 200, 300); btnToSignup.Click += (s, e) => ShowPanel(true);
            pnlLogin.Controls.Add(btnToSignup);

            var btnGuest = MakeButton("游客进入（不能导出计算书）", 20, 240, 300); btnGuest.Click += (s, e) => { AppSession.SetGuest(); DialogResult = DialogResult.OK; Close(); };
            pnlLogin.Controls.Add(btnGuest);
        }

        private void BuildSignupPanel()
        {
            pnlSignup.Location = new Point(0, 70);
            pnlSignup.Size = new Size(340, 280);

            pnlSignup.Controls.Add(MakeLabel("手机号（作为登录用户名）", 20, 6));
            txtSPhone.SetBounds(20, 26, 300, 25); txtSPhone.MaxLength = 11;
            pnlSignup.Controls.Add(txtSPhone);

            pnlSignup.Controls.Add(MakeLabel("邮箱（用于找回密码）", 20, 56));
            txtSEmail.SetBounds(20, 76, 300, 25);
            pnlSignup.Controls.Add(txtSEmail);

            pnlSignup.Controls.Add(MakeLabel("设置密码（至少 6 位）", 20, 106));
            txtSPass.SetBounds(20, 126, 300, 25); txtSPass.UseSystemPasswordChar = true;
            pnlSignup.Controls.Add(txtSPass);

            pnlSignup.Controls.Add(MakeLabel("确认密码", 20, 156));
            txtSConfirm.SetBounds(20, 176, 300, 25); txtSConfirm.UseSystemPasswordChar = true;
            pnlSignup.Controls.Add(txtSConfirm);

            var btnSignup = MakeButton("提交注册", 20, 216, 140); btnSignup.BackColor = Color.FromArgb(31, 111, 235); btnSignup.ForeColor = Color.White;
            btnSignup.Click += async (s, e) => await DoSignup();
            pnlSignup.Controls.Add(btnSignup);

            var btnBack = MakeButton("返回登录", 180, 216, 140); btnBack.Click += (s, e) => ShowPanel(false);
            pnlSignup.Controls.Add(btnBack);
        }

        private void ShowPanel(bool signup)
        {
            pnlLogin.Visible = !signup;
            pnlSignup.Visible = signup;
            lblStatus.Text = "";
            if (!signup) ResetForgot();
        }

        private void ResetForgot()
        {
            forgotMode = false;
            lblForgotEmail.Visible = false;
            txtForgotEmail.Visible = false;
            txtForgotEmail.Text = "";
            btnForgot.Text = "忘记密码";
        }

        private async Task btnForgot_Click()
        {
            if (!forgotMode)
            {
                if (!PhoneRe.IsMatch(txtPhone.Text.Trim())) { Fail("请先在手机号栏输入已注册的手机号。"); return; }
                forgotMode = true;
                lblForgotEmail.Visible = true;
                txtForgotEmail.Visible = true;
                btnForgot.Text = "发送重置邮件";
                lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "请输入注册时绑定的邮箱，然后点击“发送重置邮件”。";
                txtForgotEmail.Focus();
                return;
            }
            await DoForgot();
        }

        private static Label MakeLabel(string text, int x, int y)
            => new Label { Text = text, Left = x, Top = y, AutoSize = true, ForeColor = Color.DimGray };

        private static Button MakeButton(string text, int x, int y, int w)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 30, FlatStyle = FlatStyle.Flat, AutoEllipsis = true };
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
            string phone = txtSPhone.Text.Trim(), email = txtSEmail.Text.Trim(), pass = txtSPass.Text, confirm = txtSConfirm.Text;
            if (!PhoneRe.IsMatch(phone)) { Fail("请输入 11 位手机号。"); return; }
            if (!EmailRe.IsMatch(email)) { Fail("请输入正确的邮箱。"); return; }
            if (pass.Length < 6) { Fail("密码至少 6 位。"); return; }
            if (pass != confirm) { Fail("两次输入的密码不一致。"); return; }
            lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "注册中…";
            var r = await SupabaseClient.Signup(phone, email, pass);
            if (r.Success) { AppSession.SetLoggedIn(phone, r.Email, r.AccessToken); DialogResult = DialogResult.OK; Close(); }
            else Fail(r.Message);
        }

        private async Task DoForgot()
        {
            string phone = txtPhone.Text.Trim();
            string email = txtForgotEmail.Text.Trim();
            if (!PhoneRe.IsMatch(phone)) { Fail("请先在手机号栏输入已注册的手机号。"); return; }
            if (!EmailRe.IsMatch(email)) { Fail("请在“绑定邮箱”栏输入注册时预留的邮箱。"); return; }
            lblStatus.ForeColor = Color.DimGray; lblStatus.Text = "校验并发送中…";
            var bound = await SupabaseClient.LookupEmailByPhone(phone);
            if (string.IsNullOrEmpty(bound)) { Fail("该手机号未注册。"); return; }
            if (!string.Equals(bound!.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Fail("邮箱与该手机号绑定的不一致，无法发送重置邮件。");
                return;
            }
            var r = await SupabaseClient.Recover(bound!);
            if (r.Success)
            {
                var msg = r.Message;
                ResetForgot();
                lblStatus.ForeColor = Color.FromArgb(26, 127, 55);
                lblStatus.Text = msg;
            }
            else
            {
                lblStatus.ForeColor = Color.Firebrick;
                lblStatus.Text = r.Message;
            }
        }

        private void Fail(string msg) { lblStatus.ForeColor = Color.Firebrick; lblStatus.Text = msg; }
    }
}
