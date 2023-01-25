using Godot;
using System;
using System.Threading.Tasks;

public class Server : Node{
    private NetworkedMultiplayerENet Network = new NetworkedMultiplayerENet();
    private string ip = "90.8.118.48";
    private int port = 4180;
    public Godot.Collections.Dictionary addresses = new Godot.Collections.Dictionary();
    string name = "";

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
    public void resetNetwork(){
        Network.Disconnect("connection_failed", this, "connectionFailed");
        Network.Disconnect("connection_succeeded", this, "connectionSucceeded");
        GetTree().NetworkPeer = null;
    }
    //Triggered when connection to server has failed 
    private void connectionFailed(){
        GD.Print("failed to connect");
    }
    //Triggered when connection to server succeeded 
    private void connectionSucceeded(){
        GD.Print("success");
    }
    //Triggered by the server to kick the player
    [Remote]
    public void kickedFromServer(string reason){
        GD.Print("You were kicked from server: " + reason);
        Network.DisconnectPeer(GetTree().GetNetworkUniqueId(), true);
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
        GD.Print(error);
    }
    //Gets in the game
    [Remote]
    public void logIn(){
        GD.Print("LOGIN AND GET THE F*** IN GAME");
    }
    //Get throught the first steps of the game like selecting the address, customize the house etc...
    [Remote]
    public void goThroughFirstSteps(Godot.Collections.Dictionary addresses){
        this.addresses = addresses;
        GetTree().ChangeScene("res://scenes/UI/AddressSelector.tscn");
    }
    //Sends a request to the server to allocate the selected address
    public void allocateAddress(string letter, Vector2 coords){
        RpcId(1, "receiveAddressRequest", name, letter, coords);
    }
    //Receives the feedback after the allocation request has been processed
    [Remote]
    public void addressAllocationFeedback(bool success){
        if(GetParent().HasNode("AddressSelector") && !success){
            GetNode<AddressSelector>("/root/AddressSelector").cancelCoords();
        } else if (success){
            //TO-DO Do something
        }
    }
}
