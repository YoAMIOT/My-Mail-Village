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

    public void connectToServer(){
        Network.CreateClient(ip, port);
        GetTree().NetworkPeer = Network;
        Network.Connect("connection_failed", this, "connectionFailed");
        Network.Connect("connection_succeeded", this, "connectionSucceeded");
    }

    public void resetNetwork(){
        Network.Disconnect("connection_failed", this, "connectionFailed");
        Network.Disconnect("connection_succeeded", this, "connectionSucceeded");
        GetTree().NetworkPeer = null;
    }

    private void connectionFailed(){
        GD.Print("failed to connect");
    }

    private void connectionSucceeded(){
        GD.Print("success");
    }

    [Remote]
    public void kickedFromServer(string reason){
        GD.Print("You were kicked from server: " + reason);
        Network.DisconnectPeer(GetTree().GetNetworkUniqueId(), true);
        resetNetwork();
    }



    public void sendCredentialsValidationRequest(bool register, string username, string password){
        name = username;
        RpcId(1, "credentialsValidationRequest", register, username, password, GetTree().GetNetworkUniqueId());
    }

    [Remote]
    public void authError(string error){
        GD.Print(error);
    }

    [Remote]
    public void logIn(){
        GD.Print("LOGIN AND GET THE F*** IN GAME");
    }

    [Remote]
    public void goThroughFirstSteps(Godot.Collections.Dictionary addresses){
        this.addresses = addresses;
        GetTree().ChangeScene("res://scenes/UI/AddressSelector.tscn");
    }

    public void allocateAddress(string letter, Vector2 coords){
        RpcId(1, "receiveAddressRequest", name, letter, coords);
    }

    [Remote]
    public void addressAllocationFeedback(bool success){
        //TO-DO make changes to the address selection scene depending on success 
        GD.Print(success);
    }
}
