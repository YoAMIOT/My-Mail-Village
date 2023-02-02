using Godot;
using System;

public class AddressSelector : Control{
    private Godot.Collections.Dictionary addresses = new Godot.Collections.Dictionary();
    private Server Server;
    private string selectedLetter = "";
    private int selectedLetterIndex;
    private Vector2 min = new Vector2();
    private Vector2 max = new Vector2();
    private Vector2 realCoords = new Vector2();

    public override void _Ready(){
        Server = GetNode<Server>("/root/Server");
        this.addresses = Server.addresses;
        GetNode<ItemList>("LetterList").Connect("item_selected", this, "letterSelected");
        foreach (Button b in GetNode<Control>("Coords").GetChildren()){
            b.Connect("pressed", this, "coordSelected", new Godot.Collections.Array((Button)b));
        }
        GetNode<Button>("Confirm/Select/ConfirmBtn").Connect("pressed", this, "confirmCoords");
        GetNode<Button>("Confirm/Select/CancelBtn").Connect("pressed", this, "cancelCoords");
        GetNode<Button>("ErrorMsg/OkButton").Connect("pressed", this,"okButton");
    }



//LETTER RELATED
    //Triggered when the a letter has been clicked
    private void letterSelected(int index){
        foreach (Button b in GetNode<Control>("Coords").GetChildren()){
            b.Disabled = false;
        }
        selectedLetterIndex = index;
        if (GetNode<ItemList>("LetterList").GetItemText(index) != ""){
            GetNode<Control>("Coords").Visible = true;
            selectedLetter = GetNode<ItemList>("LetterList").GetItemText(index).ToLower();
            min = new Vector2(Convert.ToInt32((addresses[selectedLetter] as Godot.Collections.Dictionary)["minX"]),Convert.ToInt32((addresses[selectedLetter] as Godot.Collections.Dictionary)["minY"]));
            max = new Vector2(Convert.ToInt32((addresses[selectedLetter] as Godot.Collections.Dictionary)["maxX"]),Convert.ToInt32((addresses[selectedLetter] as Godot.Collections.Dictionary)["maxY"]));
            foreach (string s in (addresses[selectedLetter] as Godot.Collections.Dictionary).Keys){
                if(s != "minX" && s != "minY" && s != "maxX" && s != "maxY"){
                    Godot.Collections.Dictionary alreadyAllocated = (addresses[selectedLetter] as Godot.Collections.Dictionary)[s]as Godot.Collections.Dictionary;
                    Vector2 allocatedCoords = new Vector2(Convert.ToInt32(alreadyAllocated["x"]) - min.x, Convert.ToInt32(alreadyAllocated["y"]) - min.y);
                    GetNode<Button>("Coords/" + allocatedCoords.x.ToString() + "," + allocatedCoords.y.ToString()).Disabled = true;
                }
            }
        }
    }

    //Triggered when the player has pressed "No" and resets the selection menu
    public void cancelCoords(){
        resetCoords(addresses);
    }

    //Resets the selection menu
    public void resetCoords(Godot.Collections.Dictionary refreshedAddresses, bool failToAllocate = false){
        GetNode<ItemList>("LetterList").Unselect(selectedLetterIndex);
        selectedLetter = "";
        min = new Vector2();
        max = new Vector2();
        realCoords = new Vector2();
        GetNode<Control>("Coords").Visible = false;
        GetNode<Control>("Confirm").Visible = false;
        GetNode<Control>("Confirm/Checks").Visible = false;
        GetNode<Control>("Confirm/Select").Visible = true;
        if(failToAllocate){
            GetNode<Control>("ErrorMsg").Visible = true;
        }
        this.addresses = refreshedAddresses;
    }

    public void okButton(){
        GetNode<Control>("ErrorMsg").Visible = false;
    }



//COORDINATE RELATED
    //Triggered when coords are selected and open the confirmation menu
    private void coordSelected(Button button){
        char[] separator = {','};
        string[] coordsString = button.Name.Split(separator);
        realCoords = new Vector2(coordsString[0].ToInt() + min.x, coordsString[1].ToInt() + min.y);
        if((realCoords.x >= min.x && realCoords.y >= min.y) && (realCoords.x <= max.x && realCoords.y <= max.y)){
            GetNode<Control>("Confirm").Visible = true;
        }
    }
    
    //Sends coordinates to server for verifying if the slot isn't allowed
    private void confirmCoords(){
        Server.allocateAddress(selectedLetter, realCoords);
        GetNode<Control>("Confirm/Select").Visible = false;
        GetNode<Control>("Confirm/Checks").Visible = true;
    }
}
