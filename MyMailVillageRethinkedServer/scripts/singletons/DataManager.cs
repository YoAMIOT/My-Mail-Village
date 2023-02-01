using Godot;
using System;
using System.Threading.Tasks;

public class DataManager : Node{
    public Godot.Collections.Dictionary playersDatas = new Godot.Collections.Dictionary();
    public Godot.Collections.Dictionary charactersDatas = new Godot.Collections.Dictionary();
    public Godot.Collections.Dictionary connectedPlayers = new Godot.Collections.Dictionary();
    private string playersDatasFile = "res://data/playersDatas.json";
    private string addressesFile = "res://data/addresses.json";
    private string charactersDatasFile = "res://data/characterDatas.json";
    private AddressManager AddressManager;

    public override void _Ready(){
        AddressManager = GetNode<AddressManager>("/root/AddressManager");
        loadPlayersDatas();
        loadAddresses();
        loadCharactersDatas();
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

    //Creates the formatted datas to store the player datas
    public void createDatasOfAPlayer(string username, string password, string salt){
        playersDatas[username] = new Godot.Collections.Dictionary(){
            {"password", password},
            {"salt", salt}
        };
    }

    //Checks in the stored datas if a player exists
    public bool userExists(string username){
        bool exists = false;
        foreach (string user in playersDatas.Keys){
            if(user == username){
                exists = true;
            }
        }
        return exists;
    }

    public void playerConnected(int userId, string username){
        connectedPlayers[userId] = username;
    }

    public void playerDisconnected(int userId){
        connectedPlayers.Remove(userId);
    }

//ADDRESS RELATED
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

//CHARACTER RELATED
    private void loadCharactersDatas(){
        File file = new File();
        if (!file.FileExists(charactersDatasFile)) {
            saveCharactersDatas();
            return;
        }
        file.Open(charactersDatasFile, File.ModeFlags.Read);
        if (file.GetAsText() != "") {
            charactersDatas = (Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result;
            file.Close();
        }
    }

    public void saveCharactersDatas(){
        File file = new File();
        file.Open(charactersDatasFile, File.ModeFlags.Write);
        file.StoreLine(JSON.Print(charactersDatas));
        file.Close();
    }

    public void createCharacterDatasOfAPlayer(int userId, string hairStyle, string eyesType, string noseType, string hairColor, string skinColor){
        charactersDatas[connectedPlayers[userId]] = new Godot.Collections.Dictionary{
            {"hairStyle" , hairStyle},
            {"hairColor", hairColor},
            {"eyesType", eyesType},
            {"noseType", noseType},
            {"skinColor", skinColor}
        };
    }
}
