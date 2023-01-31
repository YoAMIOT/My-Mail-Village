using Godot;
using System;

public class testChar : KinematicBody{
    private const float FALL_ACCELERATION = 75;
    private const int SPEED = 7;
    private Vector3 _velocity = Vector3.Zero;
    private const float LERP_VAL = .15F;
    private const float ZOOM_MIN = 4F;
    private const float ZOOM_MAX = 20F;
    private const float ZOOM_SPEED = 4F;

    public override void _Input(InputEvent @event){
        if (@event is InputEventMouseButton mouseEvent){
            if (mouseEvent.IsPressed() && !GetNode<Tween>("ZoomTween").IsActive()){
                if (mouseEvent.ButtonIndex == (int)ButtonList.WheelUp){
                    if(GetNode<SpringArm>("SpringArm").SpringLength >= ZOOM_MIN + ZOOM_SPEED){
                        GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, GetNode<SpringArm>("SpringArm").SpringLength - ZOOM_SPEED, 0.25F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
                        GetNode<Tween>("ZoomTween").Start();
                    }
                } if (mouseEvent.ButtonIndex == (int)ButtonList.WheelDown){
                    if(GetNode<SpringArm>("SpringArm").SpringLength <= ZOOM_MAX - ZOOM_SPEED){
                        GetNode<Tween>("ZoomTween").InterpolateProperty(GetNode<SpringArm>("SpringArm"), "spring_length", GetNode<SpringArm>("SpringArm").SpringLength, GetNode<SpringArm>("SpringArm").SpringLength + ZOOM_SPEED, 0.25F, Tween.TransitionType.Linear, Tween.EaseType.OutIn);
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
        if (direction != Vector3.Zero){  
            _velocity.x = Mathf.Lerp(_velocity.x, direction.x * SPEED, LERP_VAL);
            _velocity.z = Mathf.Lerp(_velocity.z, direction.z * SPEED, LERP_VAL);   
            if(GetNode<Spatial>("Armature").Rotation.y != (float)Math.Atan2(_velocity.x, _velocity.z)){
                float newAngle = (float)Mathf.LerpAngle(GetNode<Spatial>("Armature").Rotation.y, (float)Math.Atan2(_velocity.x, _velocity.z), delta * 12);
                GetNode<Spatial>("Armature").Rotation = new Vector3(GetNode<Spatial>("Armature").Rotation.x, newAngle, GetNode<Spatial>("Armature").Rotation.z);
            }
        } else {
            _velocity.x = Mathf.Lerp(_velocity.x, 0.0F, LERP_VAL);
            _velocity.z = Mathf.Lerp(_velocity.z, 0.0F, LERP_VAL);  
        }
        
        GetNode<AnimationTree>("AnimationTree").Set("parameters/BlendSpace1D/blend_position", _velocity.Length() / SPEED);

        if(!IsOnFloor()){
            _velocity.y -= FALL_ACCELERATION * delta;
        }

        _velocity = MoveAndSlide(_velocity, Vector3.Up);
    }
}
