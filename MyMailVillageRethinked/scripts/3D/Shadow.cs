using Godot;
using System;

public class Shadow : KinematicBody{
    public override void _Ready(){
        GetNode<Area>("FOV").Connect("body_entered", this, "bodyEnteredFOV");
        GetNode<Area>("FOV").Connect("body_exited", this, "bodyExitedFOV");
    }

    private void bodyEnteredFOV(Node body){
        if(body != this){
            GD.Print(body);
        }
    }

    private void bodyExitedFOV(Node body){
        if(body != this){
            GD.Print(body);
        }
    }
}
