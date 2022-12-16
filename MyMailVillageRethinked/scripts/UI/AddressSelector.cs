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
        GetNode<Button>("Confirm/ConfirmBtn").Connect("pressed", this, "confirmCoords");
        GetNode<Button>("Confirm/CancelBtn").Connect("pressed", this, "cancelCoords");
    }

    private void letterSelected(int index){
        foreach (Button b in GetNode<Control>("Coords").GetChildren()){
            b.Disabled = false;
        }
        selectedLetterIndex = index;
        if (GetNode<ItemList>("LetterList").GetItemText(index) != ""){
            GetNode<Control>("Coords").Visible = true;
            selectedLetter = GetNode<ItemList>("LetterList").GetItemText(index).ToLower();
            min = new Vector2((int)(addresses[selectedLetter] as Godot.Collections.Dictionary)["minX"],(int)(addresses[selectedLetter] as Godot.Collections.Dictionary)["minY"]);
            max = new Vector2((int)(addresses[selectedLetter] as Godot.Collections.Dictionary)["maxX"],(int)(addresses[selectedLetter] as Godot.Collections.Dictionary)["maxY"]);
            foreach (string s in (addresses[selectedLetter] as Godot.Collections.Dictionary).Keys){
                if(s != "minX" && s != "minY" && s != "maxX" && s != "maxY"){
                    Godot.Collections.Dictionary alreadyAllocated = (addresses[selectedLetter] as Godot.Collections.Dictionary)[s]as Godot.Collections.Dictionary;
                    Vector2 allocatedCoords = new Vector2((int)alreadyAllocated["x"] - min.x, (int)alreadyAllocated["y"] - min.y);
                    GetNode<Button>("Coords/" + allocatedCoords.x.ToString() + "," + allocatedCoords.y.ToString()).Disabled = true;
                }
            }
        }
    }

    private void coordSelected(Button button){
        char[] separator = {','};
        string[] coordsString = button.Name.Split(separator);
        realCoords = new Vector2(coordsString[0].ToInt() + min.x, coordsString[1].ToInt() + min.y);
        if((realCoords.x >= min.x && realCoords.y >= min.y) && (realCoords.x <= max.x && realCoords.y <= max.y)){
            GetNode<Control>("Confirm").Visible = true;
        }
    }

    private void confirmCoords(){
        Server.allocateAddress(selectedLetter, realCoords);
    }

    private void cancelCoords(){
        GetNode<ItemList>("LetterList").Unselect(selectedLetterIndex);
        selectedLetter = "";
        min = new Vector2();
        max = new Vector2();
        realCoords = new Vector2();
        GetNode<Control>("Coords").Visible = false;
        GetNode<Control>("Confirm").Visible = false;
    }
}
