using Godot;
using System;

public class Char : KinematicBody{
    public const int MAX_HEALTH = 100;
    public int health = MAX_HEALTH;
    private const int MAX_MANA = 100;
    private int mana = MAX_MANA;
    private const float FALL_ACCELERATION = 75;
    private const int MAX_SPEED = 7;
    private int speed = MAX_SPEED;
    private Vector3 _velocity = Vector3.Zero;
    private const float LERP_VAL = .15F;
    private const float ZOOM_STEP = 4F;
    private const float ZOOM_MIN = ZOOM_STEP * 2;
    private const float ZOOM_MAX = ZOOM_STEP * 5;
    private Godot.Collections.Dictionary characterAttribute;
    private Server Server;
    public string target = "";
    private bool lightOn = false;
    private const float LIGHT_CONSUMPTION_COOLDOWN = 0.5F;
    private const int LIGHT_CONSUMPTION = 1;
    private string[] Spells = {"Heal", "Repulse", "Dmg1", "", ""};
    private string selectedSpell = "";
    private bool canCast = true;
    private const int MANA_REGENERATION = 2;

    public override void _Ready(){
        Server = GetNode<Server>("/root/Server");
        initializePhysicalAttributes();
        GetNode<Timer>("Light/Timer").Connect("timeout", this, "lightManaCooldown");
        GetNode<Timer>("Light/Timer").WaitTime = LIGHT_CONSUMPTION_COOLDOWN;
        int i = 0;
        foreach (Button b in GetNode<Control>("SpellMenu").GetChildren()){
            b.Connect("pressed", this, "spellSelected", new Godot.Collections.Array((Button)b));
            b.Text = Spells[i];
            i++;
            if(b.Text == ""){
                b.Disabled = true;
            }
        }
        selectedSpell = Spells[0].ToLower();
        GetNode<Timer>("SpellCooldown").Connect("timeout", this, "castCooldown");
        GetNode<Timer>("ManaCooldown").Connect("timeout", this, "manaCooldown");
    }

//INPUT RELATED
    public override void _PhysicsProcess(float delta){
        Vector3 direction = Vector3.Zero;

        //CAMERA ROTATION RELATED
        if (Input.IsActionJustPressed("cameraRotateLeft") && !GetNode<Tween>("CamTween").IsActive()){
            GetNode<Tween>("CamTween").InterpolateProperty(GetNode<Spatial>("SpringArm"),"rotation_degrees", GetNode<Spatial>("SpringArm").RotationDegrees, new Vector3(GetNode<Spatial>("SpringArm").RotationDegrees.x, GetNode<Spatial>("SpringArm").RotationDegrees.y + 90, GetNode<Spatial>("SpringArm").RotationDegrees.z), 0.4F, Tween.TransitionType.Sine, Tween.EaseType.InOut);
            GetNode<Tween>("CamTween").Start();
        } if (Input.IsActionJustPressed("cameraRotateRight") && !GetNode<Tween>("CamTween").IsActive()){
            GetNode<Tween>("CamTween").InterpolateProperty(GetNode<Spatial>("SpringArm"),"rotation_degrees", GetNode<Spatial>("SpringArm").RotationDegrees, new Vector3(GetNode<Spatial>("SpringArm").RotationDegrees.x, GetNode<Spatial>("SpringArm").RotationDegrees.y - 90, GetNode<Spatial>("SpringArm").RotationDegrees.z), 0.4F, Tween.TransitionType.Sine, Tween.EaseType.InOut);
            GetNode<Tween>("CamTween").Start();
        }

        //CAMERA ZOOM RELATED
        if (Input.IsActionJustPressed("zoomCycle") && !GetNode<Tween>("ZoomTween").IsActive()){
            float currentZoom = GetNode<SpringArm>("SpringArm").SpringLength;
            if (GetNode<SpringArm>("SpringArm").SpringLength < ZOOM_MAX){
                GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, GetNode<SpringArm>("SpringArm").SpringLength + ZOOM_STEP, 0.25F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
            } else if (GetNode<SpringArm>("SpringArm").SpringLength == ZOOM_MAX){
                GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, ZOOM_MIN, 0.5F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
            }
            GetNode<Tween>("ZoomTween").Start();
        }
        
        //MOVEMENT RELATED
        if (Input.IsActionPressed("right")){
            direction.x += 1f;
        } if (Input.IsActionPressed("left")){
            direction.x -= 1f;
        } if (Input.IsActionPressed("backward")){
            direction.z += 1f;
        } if (Input.IsActionPressed("forward")){
            direction.z -= 1f;
        }

        //SPELL RELATED
        if (Input.IsActionJustPressed("switchLight") && mana > 0){
            lightOn = !lightOn;
            switchLight(lightOn);
        } if (Input.IsActionJustPressed("castSpell") && GetNode<Control>("SpellMenu").Visible == false && canCast){
            castSpell();
        } if (Input.IsActionJustPressed("selectSpell")){
            GetNode<Control>("SpellMenu").Visible = !GetNode<Control>("SpellMenu").Visible;
        }

        direction = direction.Normalized();
        direction = direction.Rotated(Vector3.Up, GetNode<Spatial>("SpringArm").Rotation.y);
        if (direction != Vector3.Zero){  
            _velocity.x = Mathf.Lerp(_velocity.x, direction.x * speed, LERP_VAL);
            _velocity.z = Mathf.Lerp(_velocity.z, direction.z * speed, LERP_VAL);   
            if(GetNode<Spatial>("Armature").Rotation.y != (float)Math.Atan2(_velocity.x, _velocity.z)){
                float newAngle = (float)Mathf.LerpAngle(GetNode<Spatial>("Armature").Rotation.y, (float)Math.Atan2(_velocity.x, _velocity.z), delta * 12);
                GetNode<Spatial>("Armature").Rotation = new Vector3(GetNode<Spatial>("Armature").Rotation.x, newAngle, GetNode<Spatial>("Armature").Rotation.z);
            }
        } else {
            _velocity.x = Mathf.Lerp(_velocity.x, 0.0F, LERP_VAL);
            _velocity.z = Mathf.Lerp(_velocity.z, 0.0F, LERP_VAL);  
        }
        
        GetNode<AnimationTree>("AnimationTree").Set("parameters/BlendSpace1D/blend_position", _velocity.Length() / speed);

        _velocity.y -= FALL_ACCELERATION * delta;

        _velocity = MoveAndSlide(_velocity, Vector3.Up);

        manageTargetPointer();

        if(GetNode<Timer>("SpellCooldown").TimeLeft > 0){
            manageCastCooldownBar();
        }
    }

//INITIALIZE PHYSICAL APPEARANCE
    private void initializePhysicalAttributes(){
        // characterAttribute = Server.characterAttribute;
        // string hair = (string)characterAttribute["hairStyle"];
        // string eyes = (string)characterAttribute["eyesType"];
        // string nose = (string)characterAttribute["noseType"];
        // PackedScene hairScene = GD.Load<PackedScene>("res://ressources/meshes/attributes/hair/" + hair.ToLower() + ".glb");
        // PackedScene eyesScene = GD.Load<PackedScene>("res://ressources/meshes/attributes/eyes/" + eyes.ToLower() + ".glb");
        // PackedScene noseScene = GD.Load<PackedScene>("res://ressources/meshes/attributes/noses/" + nose.ToLower() + ".glb");
        // Spatial hairInstance = (Spatial)hairScene.Instance();
        // Spatial eyesInstance = (Spatial)eyesScene.Instance();
        // Spatial noseInstance = (Spatial)noseScene.Instance();
        // hairInstance.GetChild<MeshInstance>(0).MaterialOverride = GD.Load<Material>("res://ressources/meshes/attributes/hair/hair.material");
        // noseInstance.GetChild<MeshInstance>(0).MaterialOverride = GD.Load<Material>("res://ressources/meshes/skin.material");
        // hairInstance.GetChild<MeshInstance>(0).MaterialOverride.Set("albedo_color", new Color((string)characterAttribute["hairColor"]));
        // noseInstance.GetChild<MeshInstance>(0).MaterialOverride.Set("albedo_color", new Color((string)characterAttribute["skinColor"]));
        // GetNode<Position3D>("Armature/Skeleton/HeadAttachment/Position3D").AddChild(hairInstance);
        // GetNode<Position3D>("Armature/Skeleton/HeadAttachment/Position3D").AddChild(eyesInstance);
        // GetNode<Position3D>("Armature/Skeleton/HeadAttachment/Position3D").AddChild(noseInstance);
    }

//HEALTH RELATED
    public void getsHit(int damage){
        health -= damage;
        checkHealth();
        GetNode<AnimationPlayer>("HUD/Health/AnimationPlayer").Stop();
        GetNode<AnimationPlayer>("HUD/Health/AnimationPlayer").Play("hit");
    }

    private void regainHealth(int amount){
        health += amount;
        checkHealth();
        GetNode<AnimationPlayer>("HUD/Health/AnimationPlayer").Stop();
        GetNode<AnimationPlayer>("HUD/Health/AnimationPlayer").Play("heal");
    }

    private void checkHealth(){
        updateHealthBar();
        if(health <= 0){
            health = 0;
            die();
        } else if(health > MAX_HEALTH){
            health = MAX_HEALTH;
        }
    }
    
    private void updateHealthBar(){
        GetNode<ProgressBar>("HUD/Health").Value = health;
    }

    private void die(){
        GD.Print("Player died");
    }

//MANA RELATED
    private void consumeMana(int amount){
        mana -= amount;
        checkMana();
        GetNode<AnimationPlayer>("HUD/Mana/AnimationPlayer").Stop();
        GetNode<AnimationPlayer>("HUD/Mana/AnimationPlayer").Play("use");
    }

    private void regainMana(int amount){
        mana += amount;
        checkMana();
        GetNode<AnimationPlayer>("HUD/Mana/AnimationPlayer").Stop();
        GetNode<AnimationPlayer>("HUD/Mana/AnimationPlayer").Play("regain");
    }

    private void manaCooldown(){
        if (mana < 100){
            regainMana(MANA_REGENERATION);
        }
    }

    private void checkMana(){
        updateManaBar();
        manageManaLight();
        if(mana <= 0){
            mana = 0;
            if (lightOn){
                switchLight(false);
            }
        } else if(mana > MAX_MANA){
            mana = MAX_MANA;
        }
    }
    
    private void updateManaBar(){
        GetNode<ProgressBar>("HUD/Mana").Value = mana;
    }

    private void manageManaLight(){
        float manaSpent = (int)(MAX_MANA - mana);
        manaSpent = manaSpent / 100;
        float currentMana = mana;
        currentMana = currentMana / 100;
        GetNode<OmniLight>("Armature/Skeleton/BoneAttachment/ManaLight").LightColor = new Color(manaSpent, currentMana, 0, 1);
    }

//TARGET SYSTEM RELATED
    public void setTarget(string targetName){
        if (target != ""){
            GetParent().GetNode<MeshInstance>("Ennemies/" + target + "/Appearance/MeshInstance/Selected").Visible = false;
        }
        GetParent().GetNode<MeshInstance>("Ennemies/" + targetName + "/Appearance/MeshInstance/Selected").Visible = true;
        target = targetName;
    }

    private void manageTargetPointer(){
        Spatial pointer = GetNode<Spatial>("TargetPointer");
        if(target == ""){
            pointer.Visible = false;
        } else if(target != ""){
            pointer.Visible = true;
            pointer.LookAt(GetParent().GetNode<Spatial>("Ennemies/" + target + "/Appearance").GlobalTransform.origin, Vector3.Up);
            pointer.Rotation = new Vector3(pointer.Rotation.x, pointer.Rotation.y, pointer.Rotation.z);
        }
    }

    public void resetTarget(){
        target = "";
    }

//SPELL RELATED
    private void lightManaCooldown(){
        consumeMana(LIGHT_CONSUMPTION);
    }

    private void switchLight(bool status){
        if(status){
            GetNode<AnimationPlayer>("Light/AnimationPlayer").Play("lightOn");
            GetNode<Timer>("Light/Timer").Start();
        } else if(!status){
            GetNode<AnimationPlayer>("Light/AnimationPlayer").Play("lightOff");
            GetNode<Timer>("Light/Timer").Stop();
            GetNode<Timer>("Light/Timer").WaitTime = LIGHT_CONSUMPTION_COOLDOWN;
        }
    }

    private void castSpell(){
        Timer SpellCooldown = GetNode<Timer>("SpellCooldown");
        bool hasCasted = false;
        int healCost = 20;
        int healCooldown = 2;
        int healthRegain = 30;
        int repulseCost = 30;
        int repulseCooldown = 4;
        int repulseRange = 5;
        int repulseDamage = 20;
        int dmg1Cost = 15;
        int dmg1Cooldown = 2;
        int dmg1Damage = 30;

        switch(selectedSpell){
            case "heal":
                if (mana - healCost > 0){
                    canCast = false;
                    SpellCooldown.WaitTime = healCooldown;
                    consumeMana(healCost);
                    hasCasted = true;
                    regainHealth(healthRegain);
                }
                break;
            case "repulse":
                if (mana - repulseCost > 0){
                    canCast = false;
                    SpellCooldown.WaitTime = repulseCooldown;
                    consumeMana(repulseCost);
                    hasCasted = true;
                    foreach (Shadow ennemy in GetParent().GetNode<Spatial>("Ennemies").GetChildren()){
                        if (Translation.DistanceTo(ennemy.Translation) < repulseRange){
                            ennemy.repulse(repulseDamage);
                        }
                    }
                }
                break;
            case "dmg1":
                if (mana - dmg1Cost > 0 && target != ""){
                    canCast = false;
                    SpellCooldown.WaitTime = dmg1Cooldown;
                    consumeMana(dmg1Cost);
                    hasCasted = true;
                    GetParent().GetNode<Shadow>("Ennemies/" + target).getsHit(dmg1Damage);
                }
                break;
        } if (hasCasted){
            SpellCooldown.Start();
            checkHealth();
            checkMana();
        }
    }

    private void spellSelected(Button button){
        selectedSpell = button.Text.ToLower();
        GetNode<Control>("SpellMenu").Visible = false;
    }

    private void castCooldown(){
        canCast = true;
    }

    private void manageCastCooldownBar(){
        float totalTime = GetNode<Timer>("SpellCooldown").WaitTime;
        float timeLeft = GetNode<Timer>("SpellCooldown").TimeLeft;
        float percentage = (timeLeft / totalTime) * 100;
        GetNode<ProgressBar>("HUD/CastCooldown").Value = percentage;
    }
}
