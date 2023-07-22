using Godot;
using System;

public class Char : KinematicBody{
    public const int MAX_HEALTH = 10;
    private const int MAX_MANA = 10;
    private const int MAX_LIGHT_RANGE = 30;
    private const float FALL_ACCELERATION = 75;
    private const int MAX_SPEED = 7;
    private const float LERP_VAL = .15F;
    private const float MOUSE_SENS = 0.05f;
    private const int SPRINGARM_MIN_PITCH = -70;
    private const int SPRINGARM_MAX_PITCH = 50;
    public int health = MAX_HEALTH;
    private int mana = MAX_MANA;
    private int speed = MAX_SPEED;
    private Vector3 _velocity = Vector3.Zero;
    private Godot.Collections.Dictionary characterAttribute;
    private Server Server;
    public string target = "";
    private string[] Spells = {"Heal", "Repulse", "Dmg1"};
    private string selectedSpell = "";
    private bool canCast = true;
    private bool echolocation = false;
    private int manaRubyCount = 0;

    public override void _Ready(){
        Server = GetNode<Server>("/root/Server");
        initializePhysicalAttributes();
        GetNode<Timer>("SpellCooldown").Connect("timeout", this, "castCooldown");
        Input.SetMouseMode(Input.MouseMode.Captured);
        selectSpell(Spells[0]);
    }

//INPUT RELATED
    public override void _PhysicsProcess(float delta){
        Vector3 direction = Vector3.Zero;

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
        if (Input.IsActionJustPressed("castSpell") && canCast){
            castSpell();
        }

        //ITEM RELATED
        if (Input.IsActionJustPressed("useManaRuby") && manaRubyCount > 0){
            useManaRuby();
        }

        //TARGETTING RELATED
        if (Input.IsActionJustPressed("target")){
            Area collider = (Area)GetNode<RayCast>("SpringArm/Offset/Camera/RayCast").GetCollider();
            if (collider != null && collider.GetPath().ToString().StartsWith("/root/TestWorld/Ennemies/")){
                string wantedTarget = collider.GetParent().GetParent().GetParent().Name;
                if (target != wantedTarget){
                    setTarget(wantedTarget);
                } else if (target == wantedTarget){
                    resetTarget();
                }
            }
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

    public override void _Input(InputEvent @event){
        if (@event is InputEventMouseMotion mouseMotionEvent){
            Vector3 armRotation = GetNode<SpringArm>("SpringArm").RotationDegrees;
            armRotation.y -= mouseMotionEvent.Relative.x * MOUSE_SENS;
            armRotation.x -= mouseMotionEvent.Relative.y * MOUSE_SENS;
            armRotation.x = Mathf.Clamp(armRotation.x, SPRINGARM_MIN_PITCH, SPRINGARM_MAX_PITCH);
            GetNode<SpringArm>("SpringArm").RotationDegrees = armRotation;
        }
        if (@event is InputEventMouseButton mouseButtonEvent){
            if (mouseButtonEvent.IsPressed()){
                if (mouseButtonEvent.ButtonIndex == (int)ButtonList.WheelUp){
                    int idx = 0;
                    for (int i = 0; i < Spells.Length; i++){
                        if (Spells[i] == selectedSpell){
                            if (i == 0){
                                idx = Spells.Length - 1;
                            } else {
                                idx = i - 1;
                            }
                        }
                    }
                    selectSpell(Spells[idx]);
                } if (mouseButtonEvent.ButtonIndex == (int)ButtonList.WheelDown){
                    int idx = 0;
                    for (int i = 0; i < Spells.Length; i++){
                        if (Spells[i] == selectedSpell){
                            if (i == Spells.Length - 1){
                                idx = 0;
                            } else {
                                idx = i + 1;
                            }
                        }
                    }
                    selectSpell(Spells[idx]);
                }
            }
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
        GetNode<AnimationPlayer>("SpringArm/Offset/Camera/AnimationPlayer").Play("hurt");
    }

    private void regainHealth(int amount){
        health += amount;
        checkHealth();
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
    private void useManaRuby(){
        manaRubyCount -= 1;
        regainMana(5);
        updateManaRubyLabel();
    }

    public void pickManaRuby(){
        manaRubyCount += 1;
        updateManaRubyLabel();
    }

    private void updateManaRubyLabel(){
        GetNode<Label>("HUD/ManaRubiesCount").Text = manaRubyCount.ToString();
    }

    private void consumeMana(int amount){
        mana -= amount;
        checkMana();
    }

    private void regainMana(int amount){
        mana += amount;
        checkMana();
    }

    private void checkMana(){
        if(mana <= 0){
            mana = 0;
        } else if(mana > MAX_MANA){
            mana = MAX_MANA;
        }

        updateManaLight();

        if(mana <= 1 && echolocation == false){
            Echolation(true);
        } else if (mana > 1 && echolocation == true){
            Echolation(false);
        }
    }

    private void updateManaLight(){
        float manaPercentage = (float)((mana * 100) / MAX_MANA);
        float lightRangePercentage = (float)((manaPercentage * MAX_LIGHT_RANGE) / 100);
        float lightEnergy = manaPercentage / 100;
        GetNode<Tween>("Light/LightTween").InterpolateProperty(GetNode<OmniLight>("Light"), "omni_range", GetNode<OmniLight>("Light").OmniRange, lightRangePercentage, 1F);
        GetNode<Tween>("Light/LightTween").InterpolateProperty(GetNode<OmniLight>("Light"), "light_energy", GetNode<OmniLight>("Light").LightEnergy, lightEnergy, 1F);
        GetNode<Tween>("Light/LightTween").Start();
    }



//ECHOLOCATION RELATED
    public void Echolation(bool Status){
        GetNode<Spatial>("Sonar").Visible = Status;
        echolocation = Status;
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
        if (target != ""){
            GetParent().GetNode<MeshInstance>("Ennemies/" + target + "/Appearance/MeshInstance/Selected").Visible = false;
        }
        target = "";
    }



//SPELL RELATED
    private void castSpell(){
        Timer SpellCooldown = GetNode<Timer>("SpellCooldown");
        bool hasCasted = false;
        int healCost = 2;
        int healCooldown = 2;
        int healthRegain = 3;
        int repulseCost = 1;
        int repulseCooldown = 4;
        int repulseRange = 6;
        int repulseDamage = 1;
        int dmg1Cost = 1;
        int dmg1Cooldown = 2;
        int dmg1Damage = 1;

        switch(selectedSpell){
            case "Heal":
                if (mana - healCost >= 0){
                    canCast = false;
                    SpellCooldown.WaitTime = healCooldown;
                    consumeMana(healCost);
                    hasCasted = true;
                    regainHealth(healthRegain);
                }
                break;
            case "Repulse":
                if (mana - repulseCost >= 0){
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
            case "Dmg1":
                if (mana - dmg1Cost >= 0 && target != ""){
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

    private void castCooldown(){
        canCast = true;
    }

    private void manageCastCooldownBar(){
        float totalTime = GetNode<Timer>("SpellCooldown").WaitTime;
        float timeLeft = GetNode<Timer>("SpellCooldown").TimeLeft;
        float percentage = (timeLeft / totalTime) * 100;
        GetNode<ProgressBar>("HUD/CastCooldown").Value = percentage;
    }

    private void selectSpell(string spell){
        selectedSpell = spell;
        GetNode<Label>("HUD/SelectedSpellLabel").Text = selectedSpell;
    }
}
