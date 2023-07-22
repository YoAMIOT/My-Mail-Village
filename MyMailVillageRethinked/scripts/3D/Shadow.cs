using Godot;
using System;

public class Shadow : KinematicBody{
    public const int MAX_HEALTH = 2;
    public int health = MAX_HEALTH;
    private const int BASE_DAMAGE = 1;
    private const int ATTACK_RANGE = 5;
    private const float SPEED = 7.2f;
    private const float REPULSE_POWER = SPEED * 200;
    private const int FALL_ACCELERATION = 75;
    private Vector3 velocity = Vector3.Zero;
    private bool canAttack = true;
    private Char player;
    private bool repulsed = false;
    private bool dying = false;

    public override void _Ready(){
        GetNode<AnimationPlayer>("AnimationPlayer").Connect("animation_finished", this, "animationFinished");
        GetNode<Timer>("AttackCooldown").Connect("timeout", this, "cooldownTimeout");
        GetNode<Area>("Appearance/MeshInstance/SelectArea").Connect("input_event", this, "selected");
        player = GetParent().GetParent().GetNode<Char>("Char");
        checkHealth();
    }

    public override void _PhysicsProcess(float delta){ 
        if (Translation.DistanceTo(player.Translation) <= 40 && !repulsed){
            velocity = Translation.DirectionTo(player.Translation) * SPEED;
        } 
        if (repulsed){
            velocity = Translation.DirectionTo(player.Translation) * -REPULSE_POWER / Translation.DistanceTo(player.Translation);
            velocity.y = 0;
        }
        velocity.y -= FALL_ACCELERATION * delta;
        velocity = MoveAndSlide(velocity);

        //Look at player
        GetNode<Spatial>("Appearance").LookAt(player.Translation, Vector3.Up);
        GetNode<Spatial>("Appearance").RotationDegrees = new Vector3(0, GetNode<Spatial>("Appearance").RotationDegrees.y, 0);

        //Manage attacking
        if (Translation.DistanceTo(player.Translation) < ATTACK_RANGE && canAttack){
            attack();
        }
    }

//ATTACK RELATED
    private async void attack(){
        canAttack = false;
        if (!dying){
            GetNode<Timer>("AttackCooldown").Start();
            GetNode<AnimationPlayer>("AnimationPlayer").Play("Attack");
            await ToSignal(GetTree().CreateTimer(0.65f), "timeout");
            if (Translation.DistanceTo(player.Translation) < ATTACK_RANGE){
                GetNode<Particles>("Appearance/HitParticles").Emitting = true;
                player.getsHit(BASE_DAMAGE);
            }
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

//HEALTH RELATED
    public void getsHit(int damage){
        health -= damage;
        checkHealth();
        GetNode<Particles>("Appearance/HurtParticles").Emitting = true;
    }

    private void checkHealth(){
        float spentHealth = MAX_HEALTH - health;
        float spentHealthPercent = (float)((spentHealth * 100) / MAX_HEALTH);
        float lightRatio = (float)(spentHealthPercent / 100);

        if (lightRatio >= 0.01f && GetNode<MeshInstance>("Appearance/MeshInstance/Light").Visible == false){
            GetNode<MeshInstance>("Appearance/MeshInstance/Light").Visible = true;
        } if (!dying){
            GetNode<MeshInstance>("Appearance/MeshInstance/Light").Scale = new Vector3(lightRatio, lightRatio, lightRatio);
            GetNode<OmniLight>("Appearance/MeshInstance/Light/OmniLight").LightEnergy = lightRatio;
        }

        if (health <= 0){
            die();
        } else if (health > MAX_HEALTH){
            health = MAX_HEALTH;
        }
    }

    private async void die(){
        dying = true;
        GetNode<Timer>("AttackCooldown").Stop();
        GetNode<AnimationPlayer>("AnimationPlayer").Stop();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("Dying");
        GetNode<Particles>("Appearance/MeshInstance/Particles").SpeedScale = 1.5f;
        GetNode<Particles>("Appearance/MeshInstance/Particles").Emitting = false;
        await ToSignal(GetNode<AnimationPlayer>("AnimationPlayer"), "animation_finished");
        GetNode<CollisionShape>("Appearance/MeshInstance/SelectArea/CollisionShape").Disabled = true;
        
        Random rnd = new Random();
        int chance = rnd.Next(1,3);
        if (chance == 1){
            PackedScene scene = GD.Load<PackedScene>("res://scenes/3D/entities/ManaSource.tscn");
            ManaSource manaSource = (ManaSource) scene.Instance();
            GetParent<Spatial>().GetParent<Spatial>().AddChild(manaSource);
            manaSource.Translation = this.Translation;
        }

        if (player.target == this.Name){
            player.resetTarget();
        }
        QueueFree();
    }

//SPELL EFFECTS RELATED
    public async void repulse(int damage){
        repulsed = true;
        getsHit(damage);
        await ToSignal(GetTree().CreateTimer(0.1F), "timeout");
        repulsed = false;
    }
}
