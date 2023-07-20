using Godot;
using System;

public class ManaSource : Spatial {
    public override void _Ready() {
        GetNode<Area>("Area").Connect("body_entered", this, "bodyEntered");
    }

    private void bodyEntered(Node body){
        if(body.IsInGroup("player")){
            Char player = (Char)body;
            player.pickManaRuby();
            this.QueueFree();
        }
    }
}
