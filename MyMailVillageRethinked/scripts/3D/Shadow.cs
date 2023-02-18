using Godot;
using System;

public class Shadow : KinematicBody{
    public const int MAX_HEALTH = 100;
    public int health = MAX_HEALTH;
    private bool inFOV = false;
    private bool canAttack = true;
    private int attackRange = 5;
    private int baseDamage = 20;
    private Char player;
    private const float SPEED = 7.2f;
    private Vector3 velocity = Vector3.Zero;

    public override void _Ready(){
        GetNode<Area>("FOV").Connect("body_entered", this, "bodyEnteredFOV");
        GetNode<Area>("FOV").Connect("body_exited", this, "bodyExitedFOV");
        GetNode<AnimationPlayer>("AnimationPlayer").Connect("animation_finished", this, "animationFinished");
        GetNode<Timer>("AttackCooldown").Connect("timeout", this, "cooldownTimeout");
        GetNode<Area>("Appearance/MeshInstance/SelectArea").Connect("input_event", this, "selected");
        player = GetParent().GetParent().GetNode<Char>("Char");
    }

    public override void _PhysicsProcess(float delta){
        if (inFOV){
            GetNode<Spatial>("Appearance").LookAt(player.Translation, Vector3.Up);
            velocity = Translation.DirectionTo(player.Translation) * SPEED;
            velocity = MoveAndSlide(velocity);
        }
        if (Translation.DistanceTo(player.Translation) < attackRange && canAttack){
            attack();
        }
    }

    private void bodyEnteredFOV(Node body){
        if(body == player){
            inFOV = true;
        }
    }
    private void bodyExitedFOV(Node body){
        if(body == player){
            inFOV = false;
        }
    }

    private async void attack(){
        canAttack = false;
        GetNode<Timer>("AttackCooldown").Start();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Attack");
        await ToSignal(GetTree().CreateTimer(0.65f), "timeout");
        if (Translation.DistanceTo(player.Translation) < attackRange){
            GetNode<Particles>("Appearance/HitParticles").Emitting = true;
            player.getsHit(baseDamage);
        }
    }

    private void animationFinished(string animation){
        if(animation == "Attack"){
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Idle");
        }
    }

    private void cooldownTimeout(){
        canAttack = true;
    }

    public void getsHit(int damage){
        health -= damage;
        checkHealth();
    }

    public void checkHealth(){
        if(health <= 0){
            die();
        } else if(health > MAX_HEALTH){
            health = MAX_HEALTH;
        }
    }

    public void selected(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shapeIdx){
        if(@event is InputEventMouseButton btn && btn.ButtonIndex == (int)ButtonList.Right && @event.IsPressed()){
            player.setTarget(this.Name);
        }
    }

    public void die(){
        QueueFree();
    }
}
