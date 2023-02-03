using Godot;
using System;

public class ErrorPage : Control{
    public override void _Ready(){
        GetNode<Button>("OkBtn").Connect("pressed", this, "Ok");
    }

    public void setError(string error){
        GetNode<Label>("Label").Text = error;
    }

    private void Ok(){
        GetTree().ChangeScene("res://scenes/UI/Authentication.tscn");
        this.QueueFree();
    }
}
