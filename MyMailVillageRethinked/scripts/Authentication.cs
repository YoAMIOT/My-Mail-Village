using Godot;
using System;

public class Authentication : Control{
    private string usernameRegEx = "^[A-Za-z0-9_]{4,20}$";
    private string passwordRegEx = "^[A-Za-z0-9_@$!%*#?&]{8,20}$";
    private RegEx regEx = new RegEx();
    private Server Server;
    public override void _Ready(){
        Server = GetNode<Server>("/root/Server");
        GetNode<Button>("Login/RegisterBtn").Connect("pressed", this, "switchTabToReg");
        GetNode<Button>("Register/LoginBtn").Connect("pressed", this, "switchTabToLogin");
        GetNode<Button>("Login/LoginBtn").Connect("pressed", this, "login");
        GetNode<Button>("Register/RegisterBtn").Connect("pressed", this, "register");
        GetNode<LineEdit>("Login/UsernameInput").Connect("text_changed", this, "checkPossibilityLogin");
        GetNode<LineEdit>("Login/PasswordInput").Connect("text_changed", this, "checkPossibilityLogin");
        GetNode<LineEdit>("Register/RegUsernameInput").Connect("text_changed", this, "checkPossibilityReg");
        GetNode<LineEdit>("Register/RegPasswordInput").Connect("text_changed", this, "checkPossibilityReg");
        GetNode<LineEdit>("Register/RegPasswordInputConfirm").Connect("text_changed", this, "checkPossibilityReg");
    }

    private void switchTabToReg(){
        GetNode<Control>("Login").Visible = false;
        GetNode<Control>("Register").Visible = true;
        GetNode<LineEdit>("Login/UsernameInput").Text = "";
        GetNode<LineEdit>("Login/PasswordInput").Text = "";
        GetNode<Button>("Login/LoginBtn").Disabled = true;
    }

    private void switchTabToLogin(){
        GetNode<Control>("Register").Visible = false;
        GetNode<Control>("Login").Visible = true;
        GetNode<LineEdit>("Register/RegUsernameInput").Text = "";
        GetNode<LineEdit>("Register/RegPasswordInput").Text = "";
        GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text = "";
        GetNode<Button>("Register/RegisterBtn").Disabled = true;
    }

    private void checkPossibilityLogin(string txt){
        if(GetNode<LineEdit>("Login/UsernameInput").Text != "" && GetNode<LineEdit>("Login/PasswordInput").Text != ""){
            if(GetNode<LineEdit>("Login/UsernameInput").Text.Length() >= 4 && GetNode<LineEdit>("Login/PasswordInput").Text.Length() >= 8){
                regEx.Compile(usernameRegEx);
                Godot.RegExMatch usernameResult = regEx.Search(GetNode<LineEdit>("Login/UsernameInput").Text);
                regEx.Compile(passwordRegEx);
                Godot.RegExMatch passwordResult = regEx.Search(GetNode<LineEdit>("Login/PasswordInput").Text);
                if(usernameResult != null && passwordResult != null){
                    GetNode<Button>("Login/LoginBtn").Disabled = false;
                } else {
                    GetNode<Button>("Login/LoginBtn").Disabled = true;
                }
            } else {    
                GetNode<Button>("Login/LoginBtn").Disabled = true;
            }
        } else {
            GetNode<Button>("Login/LoginBtn").Disabled = true;
        }
    }

    private void login(){
        string usernameTxt = GetNode<LineEdit>("Login/UsernameInput").Text;
        string passwordTxt = GetNode<LineEdit>("Login/PasswordInput").Text;
        if(usernameTxt != "" && passwordTxt != ""){
            if(usernameTxt.Length() >= 4 && passwordTxt.Length() >= 8){
                regEx.Compile(usernameRegEx);
                Godot.RegExMatch usernameResult = regEx.Search(usernameTxt);
                regEx.Compile(passwordRegEx);
                Godot.RegExMatch passwordResult = regEx.Search(passwordTxt);
                if(usernameResult != null && passwordResult != null){
                    Server.sendCredentialsValidationRequest(false, usernameTxt, passwordTxt);
                }
            }
        }
    }

    private void checkPossibilityReg(string txt){
        if(GetNode<LineEdit>("Register/RegUsernameInput").Text != "" && GetNode<LineEdit>("Register/RegPasswordInput").Text != "" && GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text != ""){
            if(GetNode<LineEdit>("Register/RegUsernameInput").Text.Length() >= 4 && GetNode<LineEdit>("Register/RegPasswordInput").Text.Length() >= 8 && GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text.Length() >= 8){
                if(GetNode<LineEdit>("Register/RegPasswordInput").Text == GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text){
                    regEx.Compile(usernameRegEx);
                    Godot.RegExMatch usernameResult = regEx.Search(GetNode<LineEdit>("Register/RegUsernameInput").Text);
                    regEx.Compile(passwordRegEx);
                    Godot.RegExMatch passwordResult = regEx.Search(GetNode<LineEdit>("Register/RegPasswordInput").Text);
                    Godot.RegExMatch confirmPasswordResult = regEx.Search(GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text);
                    if (usernameResult != null && passwordResult != null && confirmPasswordResult != null){
                        GetNode<Button>("Register/RegisterBtn").Disabled = false;
                    } else {
                        GetNode<Button>("Register/RegisterBtn").Disabled = true;
                    }
                } else {
                    GetNode<Button>("Register/RegisterBtn").Disabled = true;
                }
            } else {
                GetNode<Button>("Register/RegisterBtn").Disabled = true;
            }
        } else {
            GetNode<Button>("Register/RegisterBtn").Disabled = true;
        }
    }

    private void register(){
        string usernameTxt = GetNode<LineEdit>("Register/RegUsernameInput").Text;
        string passwordTxt = GetNode<LineEdit>("Register/RegPasswordInput").Text;
        string confirmPasswordTxt = GetNode<LineEdit>("Register/RegPasswordInputConfirm").Text;
        if(usernameTxt != "" && passwordTxt != "" && confirmPasswordTxt != ""){
            if(usernameTxt.Length() >= 4 && passwordTxt.Length() >= 8 && confirmPasswordTxt.Length() >= 8){
                if(passwordTxt == confirmPasswordTxt){
                    regEx.Compile(usernameRegEx);
                    Godot.RegExMatch usernameResult = regEx.Search(usernameTxt);
                    regEx.Compile(passwordRegEx);
                    Godot.RegExMatch passwordResult = regEx.Search(passwordTxt);
                    Godot.RegExMatch confirmPasswordResult = regEx.Search(confirmPasswordTxt);
                    if (usernameResult != null && passwordResult != null && confirmPasswordResult != null){
                        Server.sendCredentialsValidationRequest(true, usernameTxt, passwordTxt);
                    }
                }
            }
        }
    }
}
