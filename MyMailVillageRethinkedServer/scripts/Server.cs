using Godot;
using System;
using System.Threading.Tasks;

public class Server : Control{
    private NetworkedMultiplayerENet Network = new NetworkedMultiplayerENet();
    private string ip = "Unknown Address";
    private int port = 4180;
    private int maxPlayers = 2000;
    private UPNP UPNP = new UPNP();
    private bool serverStarted = false;
    private CredentialsManager CredentialsManager;
    private DataManager DataManager;
    private AddressManager AddressManager;

    public override void _Ready(){
        CredentialsManager = GetNode<CredentialsManager>("/root/CredentialsManager");
        DataManager = GetNode<DataManager>("/root/DataManager");
        AddressManager = GetNode<AddressManager>("/root/AddressManager");
        GetTree().SetAutoAcceptQuit(false);
        UPNP.Discover(2000, 2, "InternetGatewayDevice");
        ip = UPNP.QueryExternalAddress();
        UPNP.AddPortMapping(port, port, "BoattleServer", "UDP");
        UPNP.AddPortMapping(port, port, "BoattleServer", "TCP");
        GetNode<Button>("StartServerBtn").Connect("pressed", this, "startServer");
        GetNode<Button>("StopServerBtn").Connect("pressed", this, "stopServerPressed");
        GetNode<Button>("QuitButton").Connect("pressed", this, "tryToClose");
        GetNode<Timer>("AutoSave").Connect("timeout", this, "autoSave");
        GetNode<Button>("QuitConfirm/UI/ConfirmBtn").Connect("pressed", this, "quitConfirm");
    }



//SERVER STATES AND CONNECTIONS//
    //Starts server
    private void startServer(){
        serverStarted = true;
        GetNode<Button>("StartServerBtn").Disabled = true;
        Network.CreateServer(port, maxPlayers);
        GetTree().NetworkPeer = Network;
        logPrint("!- SERVER STARTED ON " + ip + "-!");
        Network.Connect("peer_connected", this, "PeerConnected");
        Network.Connect("peer_disconnected", this, "PeerDisconnected");
        GetNode<Button>("StopServerBtn").Disabled = false;
    }

    private async void stopServerPressed(){
        bool serverStopped = await stopServer();
    }

    //Stops server
    private async Task<bool> stopServer(){
        //kick every player one by one
        foreach(var i in GetTree().Multiplayer.GetNetworkConnectedPeers()){
            kickPlayer(i, "Server Closed");
        }
        await ToSignal(GetTree().CreateTimer(1), "timeout");
        //Saves datas
        bool playersDatasCompleted = await DataManager.savePlayersDatas();
        bool addressesCompleted = await DataManager.saveAddresses();
        bool charactersCompleted = await DataManager.saveCharactersDatas();
        GetTree().NetworkPeer = null;
        serverStarted = false;
        Network.Disconnect("peer_connected", this, "PeerConnected");
        Network.Disconnect("peer_disconnected", this, "PeerDisconnected");
        Network.CloseConnection();
        GetNode<Button>("StopServerBtn").Disabled = true;
        GetNode<Button>("StartServerBtn").Disabled = false;
        logPrint("!- SERVER STOPPED -!");
        return true;
    }

    private void PeerConnected(int userId) {
        logPrint(userId + " connected.");
    }

    private void PeerDisconnected(int userId) {
        logPrint(userId + " disconnected.");
        DataManager.playerDisconnected(userId);
    }

    //Kicks a player
    public void kickPlayer(int userId, string reason = "Disconnected from server."){
        RpcId(userId, "kicked", reason);
        logPrint("!- " + userId + " was kicked: " + reason + " -!");
        DataManager.playerDisconnected(userId);
    }



//CREDENTIALS RELATED
    //Receives a credential validation request
    [Remote]
    public void credentialsValidationRequest(bool register, string username, string password, int userId){
        CredentialsManager.checkCredentials(register, username, password, userId);
    }

    //Sends an error about the auhthentication
    public void sendAuthError(int userId, string error){
        RpcId(userId, "authError", error);
    }

    //Dispatching the loging in
    public void logIn(int userId, string username){
        DataManager.playerConnected(userId, username);
        Godot.Collections.Dictionary firstSteps = DataManager.checkEveryFirstSteps(username);
        bool everySteps = true;
        foreach (var step in firstSteps.Keys){
            if ((bool)firstSteps[step] == false){
                everySteps = false;
            }
        }

        if(everySteps == true){
            //Gets in the game
            RpcId(userId, "logIn", DataManager.charactersDatas[username]);
        } else if (everySteps == false){
            //Gets in the address selection etc...
            RpcId(userId, "goThroughFirstSteps", firstSteps, AddressManager.addresses);
        }
    }



//FIRSTS STEPS RELATED
    //Receives the address validation request
    [Remote]
    public void receiveAddressRequest(string username, string letter, Vector2 coords){
        AddressManager.allocateAddressSlot(username, GetTree().GetRpcSenderId(), letter, coords);
    }

    //Sends a feedback about how the address allocation went
    public void addressAllocationFeedback(int userId, bool success){
        RpcId(userId, "addressAllocationFeedback", success, AddressManager.addresses);
    }

    //Receives the character customization datas
    [Remote]
    public void receiveCharacterData(string hairStyle, string eyesType, string noseType, string hairColor, string skinColor){
        int userId = GetTree().GetRpcSenderId();
        DataManager.createCharacterDatasOfAPlayer(userId ,hairStyle, eyesType, noseType, hairColor, skinColor);
    }

    //Sends a feedback to the client
    public void characterDatasSaved(int userId){
        RpcId(userId, "characterDatasSaved");
    }



//DATAS RELATED
    private async void autoSave(){
        bool saved = await DataManager.savePlayersDatas();
        saved = await DataManager.saveAddresses();
        saved = await DataManager.saveCharactersDatas();
    }

    //Prints a line in the server console
    public void logPrint(string txt){
        GetNode<RichTextLabel>("Log").BbcodeText += "\n" + txt;
    }



//QUITTING RELATED
    private void tryToClose(){
        GetNode<Control>("QuitConfirm").Visible = true;
    }

    //Closes the server client
    private async void quitConfirm(){
        GetNode<Control>("QuitConfirm/UI").Visible = false;
        GetNode<Control>("QuitConfirm/WaitToQuit").Visible = true;
        await ToSignal(GetTree().CreateTimer(0.2F), "timeout");
        //Saves before quitting
        if(serverStarted){
            bool serverStopped = await stopServer();
        } else {
            bool saved = await DataManager.savePlayersDatas();
            saved = await DataManager.saveAddresses();
            saved = await DataManager.saveCharactersDatas();
        }
        GetTree().Quit();
    }
}
