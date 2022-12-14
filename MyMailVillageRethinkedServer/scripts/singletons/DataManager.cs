using Godot;
using System;
using System.Threading.Tasks;

public class DataManager : Node{
    public Godot.Collections.Dictionary playersDatas = new Godot.Collections.Dictionary();
    private string playersDatasFile = "res://data/playersDatas.json";
    private string addressesFile = "res://data/addresses.json";
    private AddressManager AddressManager;

    public override void _Ready(){
        AddressManager = GetNode<AddressManager>("/root/AddressManager");
        loadPlayersDatas();
        loadAddresses();
    }

    //PLAYERS DATAS RELATED
    private void loadPlayersDatas(){
        File file = new File();
        if (!file.FileExists(playersDatasFile)) {
            savePlayersDatas();
            return;
        }
        file.Open(playersDatasFile, File.ModeFlags.Read);
        if (file.GetAsText() != "") {
            playersDatas = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            file.Close();
        }
    }

    public void savePlayersDatas(){
        File file = new File();
        file.Open(playersDatasFile, File.ModeFlags.Write);
        file.StoreLine(JSON.Print(playersDatas));
        file.Close();
    }

    public void createDatasOfAPlayer(string username, string password, string salt){
        playersDatas[username] = new Godot.Collections.Dictionary(){
            {"password", password},
            {"salt", salt}
        };
    }

    public bool userExists(string username){
        bool exists = false;
        foreach (string user in playersDatas.Keys){
            if(user == username){
                exists = true;
            }
        }
        return exists;
    }



    //ADDRESSES RELATED
    private void loadAddresses(){
        File file = new File();
        if (!file.FileExists(addressesFile)) {
            saveAddresses();
            return;
        }
        file.Open(addressesFile, File.ModeFlags.Read);
        if (file.GetAsText() != "") {
            AddressManager.addresses = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            file.Close();
        }
    }

    public void saveAddresses(){
        File file = new File();
        file.Open(addressesFile, File.ModeFlags.Write);
        file.StoreLine(JSON.Print(AddressManager.addresses));
        file.Close();
    }
}
