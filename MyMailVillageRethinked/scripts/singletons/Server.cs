using Godot;
using System;
using System.Threading.Tasks;

public class Server : Node{
    private NetworkedMultiplayerENet Network = new NetworkedMultiplayerENet();
    private string ip = "90.8.118.48";
    private int port = 4180;
    public Godot.Collections.Dictionary addresses = new Godot.Collections.Dictionary();
    public string name = "";
    public Godot.Collections.Dictionary characterAttribute = new Godot.Collections.Dictionary();

    public override void _Ready(){
        connectToServer();
    }



//CONNECTION RELATED
    //Connects to the server using the IP address and port
    public void connectToServer(){
        Network.CreateClient(ip, port);
        GetTree().NetworkPeer = Network;
        Network.Connect("connection_failed", this, "connectionFailed");
        Network.Connect("connection_succeeded", this, "connectionSucceeded");
    }

    //Reset the network peer
    public async void resetNetwork(){
        Network.Disconnect("connection_failed", this, "connectionFailed");
        Network.Disconnect("connection_succeeded", this, "connectionSucceeded");
        Network.CloseConnection();
        GetTree().NetworkPeer = null;
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        connectToServer();
    }

    //Triggered when connection to server has failed 
    private void connectionFailed(){
        if (GetParent().HasNode("Authentication")){
            Authentication auth = GetNode<Authentication>("/root/Authentication");
            auth.connectionFailed("Failed to connect to the server");
        }
        resetNetwork();
    }

    //Triggered when connection to server succeeded 
    private void connectionSucceeded(){
        if (GetParent().HasNode("Authentication")){
            Authentication auth = GetNode<Authentication>("/root/Authentication");
            auth.connectionSucceeded();
        }
    }

    //Triggered by the server to kick the player
        
    [Remote]
    public void kicked(string reason){
        PackedScene ErrorScene =  ResourceLoader.Load<PackedScene>("res://scenes/UI/ErrorPage.tscn");
        ErrorPage err = (ErrorPage)ErrorScene.Instance();
        err.setError(reason);
        GetTree().CurrentScene.AddChild(err);
        resetNetwork();
    }



//CREDENTIALS RELATED
    //Sends credentials to Server
    public void sendCredentialsValidationRequest(bool register, string username, string password){
        name = username;
        RpcId(1, "credentialsValidationRequest", register, username, password, GetTree().GetNetworkUniqueId());
    }

    //Receives an error about authentication
    [Remote]
    public void authError(string error){
        if(GetParent().HasNode("Authentication")){
            GetNode<Authentication>("/root/Authentication").authError(error);
        }
    }

    //Gets in the game
    [Remote]
    public void logIn(Godot.Collections.Dictionary character){
        characterAttribute = character;
        GetTree().ChangeScene("res://scenes/3D/TestWorld.tscn");
    }



//FIRSTS STEPS RELATED
    //Get throught the first steps of the game like selecting the address, customize the house etc...
    [Remote]
    public void goThroughFirstSteps(Godot.Collections.Dictionary firstStep, Godot.Collections.Dictionary addresses){
        this.addresses = addresses;
        if ((bool)firstStep["address"] == false){
            GetTree().ChangeScene("res://scenes/UI/AddressSelector.tscn");
        } else if ((bool)firstStep["address"] == true && (bool)firstStep["character"] == false){
            GetTree().ChangeScene("res://scenes/3D/CharacterCustomization.tscn");
        }
    }

    //Sends a request to the server to allocate the selected address
    public void allocateAddress(string letter, Vector2 coords){
        RpcId(1, "receiveAddressRequest", name, letter, coords);
    }

    //Receives the feedback after the allocation request has been processed
    [Remote]
    public void addressAllocationFeedback(bool success, Godot.Collections.Dictionary refreshedAddresses){
        //If the request has failed then select again
        if(!success){
            GetNode<AddressSelector>("/root/AddressSelector").resetCoords(refreshedAddresses, true);
        } else if (success){
            GetTree().ChangeScene("res://scenes/3D/CharacterCustomization.tscn");
        }
    }

    //Sends character customization datas to server
    public void SendCharacterDatas(string hairStyle, string eyesType, string noseType, Color hairColor, Color skinColor){
        RpcId(1, "receiveCharacterData", hairStyle, eyesType, noseType, hairColor.ToHtml(), skinColor.ToHtml());
    }
    
    [Remote]
    public void characterDatasSaved(){
        GD.Print("Character Datas Saved");
        //TO-DO Next step
    }
}
