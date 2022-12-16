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

    private void stopServerPressed(){
        stopServer();
    }

    private void stopServer(){
        foreach(var i in GetTree().Multiplayer.GetNetworkConnectedPeers()){
            kickPlayer(i, "Server Closed");
        }
        var t = Task.Run(() => DataManager.savePlayersDatas());
        t.Wait();
        t = Task.Run(() => DataManager.saveAddresses());
        GetTree().NetworkPeer = null;
        serverStarted = false;
        Network.Disconnect("peer_connected", this, "PeerConnected");
        Network.Disconnect("peer_disconnected", this, "PeerDisconnected");
        Network.CloseConnection();
        GetNode<Button>("StopServerBtn").Disabled = true;
        GetNode<Button>("StartServerBtn").Disabled = false;
        logPrint("!- SERVER STOPPED -!");
    }

    private void PeerConnected(int playerId) {
        logPrint(playerId + " connected.");
    }
    private void PeerDisconnected(int playerId) {
        logPrint(playerId + " disconnected.");
    }

    public void kickPlayer(int playerId, string reason = "Disconnected from server."){
        logPrint("!- " + playerId + " was kicked: " + reason + " -!");
        RpcId(playerId, "kickedFromServer", reason);
    }



//CREDENTIALS RELATED
    [Remote]
    public void credentialsValidationRequest(bool register, string username, string password, int userId){
        CredentialsManager.checkCredentials(register, username, password, userId);
    }

    public void sendAuthError(int userId, string error){
        RpcId(userId, "authError", error);
    }

    public void logIn(int userId, bool firstConnection){
        if(!firstConnection){
            RpcId(userId, "logIn");
        } else {
            RpcId(userId, "firstConnection", AddressManager.addresses);
        }
    }



//ADDRESS RELATED
    [Remote]
    public void receiveAddressRequest(string username, string letter, Vector2 coords){
        AddressManager.allocateAddressSlot(username, letter, coords);
    }



//DATAS RELATED
    private void autoSave(){
        DataManager.savePlayersDatas();
        DataManager.saveAddresses();
    }
    public void logPrint(string txt){
        GetNode<RichTextLabel>("Log").BbcodeText += "\n" + txt;
    }



//QUITTING RELATED
    private void tryToClose(){
        GetNode<Control>("QuitConfirm").Visible = true;
    }

    private async void quitConfirm(){
        GetNode<Control>("QuitConfirm/UI").Visible = false;
        GetNode<Control>("QuitConfirm/WaitToQuit").Visible = true;
        Timer timer = new Timer();
        timer.WaitTime = 0.5F;
        timer.Autostart = true;
        this.AddChild(timer);
        await ToSignal(timer, "timeout");
        timer.QueueFree();
        if(serverStarted){
            var t = Task.Run(() =>  stopServer());
            t.Wait();
        } else {
            var t = Task.Run(() => DataManager.savePlayersDatas());
            t.Wait();
            t = Task.Run(() => DataManager.saveAddresses());
            t.Wait();
        }
        GetTree().Quit();
    }
}
