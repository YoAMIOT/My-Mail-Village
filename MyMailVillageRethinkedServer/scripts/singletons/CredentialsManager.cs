using Godot;
using System;

public class CredentialsManager : Node{
    private string usernameRegEx = "^[A-Za-z0-9_]{4,20}$";
    private string passwordRegEx = "^[A-Za-z0-9_@$!%*#?&]{8,20}$";
    private DataManager DataManager;
    private Server Server;

    public override void _Ready(){
        DataManager = GetNode<DataManager>("/root/DataManager");
        Server = GetNode<Server>("/root/Server");
    }

    public string generateSalt() {
        GD.Randomize();
        string salt = GD.Randi().ToString().SHA256Text();
        return salt;
    }

    public string generateHashedString(string txt, string salt) {
        int rounds = (int)Math.Pow(2, 18);
        while (rounds > 0) {
            txt = (txt + salt).SHA256Text();
            rounds -= 1;
        }
        return txt;
    }

    public void checkCredentials(bool register, string username, string password, int userId, bool firstConnection = false){
        RegEx regEx = new RegEx();
        regEx.Compile(usernameRegEx);
        Godot.RegExMatch usernameResult = regEx.Search(username);
        regEx.Compile(passwordRegEx);
        Godot.RegExMatch passwordResult = regEx.Search(password);
        if(usernameResult != null && passwordResult != null){
            if(!register){
                if(DataManager.userExists(username)){
                    string savedSalt = (string)(DataManager.playersDatas[username] as Godot.Collections.Dictionary)["salt"];
                    string hashedPassword = generateHashedString(password, savedSalt);
                    if(hashedPassword == (string)(DataManager.playersDatas[username] as Godot.Collections.Dictionary)["password"]){
                        Server.logIn(userId, firstConnection);
                    } else if (hashedPassword != (string)(DataManager.playersDatas[username] as Godot.Collections.Dictionary)["password"]){
                        Server.sendAuthError(userId, "Wrong password.");
                    }
                } else if(!DataManager.userExists(username)){
                    Server.sendAuthError(userId, "User not registered, please check your credentials and try again or register.");
                }
            } else if(register){
                if(DataManager.userExists(username)){
                    Server.sendAuthError(userId, "Username is already used, please change username.");
                } else if(!DataManager.userExists(username)){
                    string salt = generateSalt();
                    string hashedPassword = generateHashedString(password, salt);
                    DataManager.createDatasOfAPlayer(username, hashedPassword, salt);
                    checkCredentials(false, username, password, userId, true);
                }
            }
        } else {
            Server.sendAuthError(userId, "Authentication failed, please check your credentials and try again.");
        }
    }
}
