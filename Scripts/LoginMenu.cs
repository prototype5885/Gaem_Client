using Godot;
using ProToTypeLounge.Password;

public partial class LoginMenu : Control
{
    Database database;

    Control LoginWindow;
    Control RegistrationWindow;

    LineEdit LoginInputUserName;
    LineEdit LoginInputPassword;

    LineEdit RegisterInputUserName;
    LineEdit RegisterInputPassword1;
    LineEdit RegisterInputPassword2;

    Label PassNoMatch;

    //string password1;
    //string password2;

    public override void _Ready()
    {
        // init
        database = GetNode<Database>("/root/Map/Database");

        LoginWindow = GetChild<Control>(0);
        RegistrationWindow = GetChild<Control>(1);

        Panel LoginPanel = LoginWindow.GetChild<Panel>(0);
        Panel RegistrationPanel = RegistrationWindow.GetChild<Panel>(0);

        LoginInputUserName = LoginPanel.GetChild(0) as LineEdit;
        LoginInputPassword = LoginPanel.GetChild(1) as LineEdit;

        RegisterInputUserName = RegistrationPanel.GetChild(0) as LineEdit;
        RegisterInputPassword1 = RegistrationPanel.GetChild(1) as LineEdit;
        RegisterInputPassword2 = RegistrationPanel.GetChild(2) as LineEdit;
        PassNoMatch = RegistrationPanel.GetChild(3) as Label;
        // end

        hide_windows();
        LoginWindow.Visible = true;
    }
    void hide_windows()
    {
        foreach (Control i in GetChildren())
        {
            i.Visible = false;
        }
    }
    void _on_login_pressed()
    {
        string username = LoginInputUserName.Text;
        string hashedPassword = Password.HashPassword(LoginInputPassword.Text);

        //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        database.LoginUser(username, hashedPassword);

        // compare password
        //bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify("XD", hashedPassword);
    }
    void _on_registration_pressed()
    {
        hide_windows();
        RegistrationWindow.Visible = true;
    }
    void _on_back_pressed()
    {
        hide_windows();
        LoginWindow.Visible = true;
    }
    void _on_register_pressed()
    {
        string username = RegisterInputUserName.Text;
        string password1 = RegisterInputPassword1.Text;
        string password2 = RegisterInputPassword2.Text;

        if (!check_if_paswords_match(password1, password2))
        {
            return;
        }

        //string hashedPassword = Password.HashPassword(password1);

        // encrypt password
        //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password1);

        string hashedPassword = Password.HashPassword(password1);

        database.NewUser(username, hashedPassword);
        //database.QueryAndDisplayData();

        //if (!Password.ValidateCredentials(password1, hashedPassword))
        //{
        //    return;
        //}
        //GD.Print("correct password");

    }


    bool check_if_paswords_match(string password1, string password2)
    {
        if (password1 == password2)
        {
            PassNoMatch.Visible = false;
            return true;
        }
        else
        {
            PassNoMatch.Visible = true;
            return false;
        }
    }
}
