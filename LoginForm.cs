namespace StokTakipSistemi;

public sealed class LoginForm : Form
{
    private readonly TextBox usernameBox = new();
    private readonly TextBox passwordBox = new();

    public LoginForm()
    {
        Text = "Stok Takip Sistemi - Giriş";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(420, 280);

        var title = new Label
        {
            Text = "Stok Takip Sistemi",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(92, 28)
        };

        var userLabel = new Label { Text = "Kullanıcı adı", Location = new Point(55, 92), AutoSize = true };
        usernameBox.Location = new Point(160, 88);
        usernameBox.Width = 190;
        usernameBox.Text = "admin";

        var passLabel = new Label { Text = "Şifre", Location = new Point(55, 132), AutoSize = true };
        passwordBox.Location = new Point(160, 128);
        passwordBox.Width = 190;
        passwordBox.PasswordChar = '*';
        passwordBox.Text = "admin123";

        var loginButton = new Button
        {
            Text = "Giriş Yap",
            Location = new Point(160, 172),
            Size = new Size(190, 36)
        };
        loginButton.Click += (_, _) => Login();

        var info = new Label
        {
            Text = "Admin: admin / admin123     Personel: personel / personel123",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Location = new Point(35, 232)
        };

        AcceptButton = loginButton;
        Controls.AddRange([title, userLabel, usernameBox, passLabel, passwordBox, loginButton, info]);
    }

    private void Login()
    {
        var session = Database.Authenticate(usernameBox.Text, passwordBox.Text);
        if (session is null)
        {
            MessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş başarısız", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Hide();
        using var mainForm = new MainForm(session);
        mainForm.ShowDialog();
        Show();
        passwordBox.Clear();
        passwordBox.Focus();
    }
}
