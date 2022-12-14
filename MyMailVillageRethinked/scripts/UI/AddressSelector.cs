using Godot;
using System;

public class AddressSelector : Control{
    private Godot.Collections.Dictionary addresses = new Godot.Collections.Dictionary();
    private Server Server;
    public override void _Ready(){
        Server = GetNode<Server>("/root/Server");
        this.addresses = Server.addresses;
        GetNode<ItemList>("LetterList").Connect("item_selected", this, "letterSelected");
    }

    private void letterSelected(int index){
        if (GetNode<ItemList>("LetterList").GetItemText(index) != ""){
            GD.Print(GetNode<ItemList>("LetterList").GetItemText(index));
        }
    }
}
