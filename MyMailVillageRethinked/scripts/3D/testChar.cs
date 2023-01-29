using Godot;
using System;

public class testChar : KinematicBody{
    private float fallAcceleration = 75;
    private int speed = 14;
    private Vector3 _velocity = Vector3.Zero;
    private float lerpValue = .15F;
    private int zoomMin = 8;
    private int zoomMax = 20;
    private int zoomSpeed = 4;

    public override void _Input(InputEvent @event){
        if (@event is InputEventMouseButton mouseEvent){
            if (mouseEvent.IsPressed()){
                if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp){
                    if(GetNode<SpringArm>("SpringArm").SpringLength > zoomMin){
                        //GetNode<SpringArm>("SpringArm").SpringLength -= zoomSpeed;
                        GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, GetNode<SpringArm>("SpringArm").SpringLength - zoomSpeed, 0.25F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
                        GetNode<Tween>("ZoomTween").Start();
                    }
                } if (mouseEvent.ButtonIndex == (int)ButtonList.WheelDown){
                    if(GetNode<SpringArm>("SpringArm").SpringLength < zoomMax){
                        GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, GetNode<SpringArm>("SpringArm").SpringLength + zoomSpeed, 0.25F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
                        GetNode<Tween>("ZoomTween").Start();
                    }
                }
            }
        }
    }

    public override void _PhysicsProcess(float delta){
        Vector3 direction = Vector3.Zero;
        if (Input.IsActionJustPressed("cameraRotateRight") && !GetNode<Tween>("CamTween").IsActive()){
            GetNode<Tween>("CamTween").InterpolateProperty(GetNode<Spatial>("SpringArm"),"rotation_degrees", GetNode<Spatial>("SpringArm").RotationDegrees, new Vector3(GetNode<Spatial>("SpringArm").RotationDegrees.x, GetNode<Spatial>("SpringArm").RotationDegrees.y + 90, GetNode<Spatial>("SpringArm").RotationDegrees.z), 0.4F, Tween.TransitionType.Sine, Tween.EaseType.InOut);
            GetNode<Tween>("CamTween").Start();
        } if (Input.IsActionJustPressed("cameraRotateLeft") && !GetNode<Tween>("CamTween").IsActive()){
            GetNode<Tween>("CamTween").InterpolateProperty(GetNode<Spatial>("SpringArm"),"rotation_degrees", GetNode<Spatial>("SpringArm").RotationDegrees, new Vector3(GetNode<Spatial>("SpringArm").RotationDegrees.x, GetNode<Spatial>("SpringArm").RotationDegrees.y - 90, GetNode<Spatial>("SpringArm").RotationDegrees.z), 0.4F, Tween.TransitionType.Sine, Tween.EaseType.InOut);
            GetNode<Tween>("CamTween").Start();
        } if (Input.IsActionPressed("right")){
            direction.x += 1f;
        } if (Input.IsActionPressed("left")){
            direction.x -= 1f;
        } if (Input.IsActionPressed("backward")){
            direction.z += 1f;
        } if (Input.IsActionPressed("forward")){
            direction.z -= 1f;
        }

        direction = direction.Normalized();
        direction = direction.Rotated(Vector3.Up, GetNode<Spatial>("SpringArm").Rotation.y);
        _velocity.x = direction.x * speed;
        _velocity.z = direction.z * speed;            

        if (direction != Vector3.Zero){
            Vector3 wantedRotation = new Vector3(GetNode<Spatial>("Armature").Rotation.x, (float)Math.Atan2(_velocity.x, _velocity.z), GetNode<Spatial>("Armature").Rotation.z);
            Vector3 currentRotation = new Vector3(GetNode<Spatial>("Armature").Rotation.x, GetNode<Spatial>("Armature").Rotation.y, GetNode<Spatial>("Armature").Rotation.z);
            
            if(currentRotation.y != wantedRotation.y){
                currentRotation.y = Mathf.LerpAngle(currentRotation.y, wantedRotation.y, delta * 12);
                GetNode<Spatial>("Armature").Rotation = currentRotation;
            }
        }

        if(!IsOnFloor()){
            _velocity.y -= fallAcceleration * delta;
        }

        _velocity = MoveAndSlide(_velocity, Vector3.Up);
    }
}
