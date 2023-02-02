using Godot;
using System;

public class CharacterCustomization : Spatial{
    private Material skinColor;
    private Material hairColor;

    public override void _Ready(){
        Godot.Collections.Array eyes = getFilesInDirectory("res://ressources/meshes/attributes/", nameof(eyes));
        Godot.Collections.Array noses = getFilesInDirectory("res://ressources/meshes/attributes/", nameof(noses));
        Godot.Collections.Array hair = getFilesInDirectory("res://ressources/meshes/attributes/", nameof(hair));
        Godot.Collections.Array[] attributes =  new Godot.Collections.Array[]{eyes, noses, hair};
        skinColor = GD.Load<Material>("res://ressources/meshes/skin.material");
        hairColor = GD.Load<Material>("res://ressources/meshes/attributes/hair/hair.material");

        //For each attributes of the arrays add the item to the options and to the scene
        foreach (Godot.Collections.Array attributeType in attributes){
            var i = 1;
            string type = "";
            foreach (string item in attributeType){
                if (i == 1){
                    type = item.Capitalize();
                } else {
                    GetNode<OptionButton>("UI/VBoxContainer/" + type + "Option").AddItem(item);
                    PackedScene itemScene = GD.Load<PackedScene>("res://ressources/meshes/attributes/" + type.ToLower() + "/" + item.ToLower() + ".glb");
                    Spatial itemInstance = (Spatial)itemScene.Instance();
                    if (type == "Hair"){
                        itemInstance.GetChild<MeshInstance>(0).MaterialOverride = hairColor;
                    } else if(type == "Noses"){
                        itemInstance.GetChild<MeshInstance>(0).MaterialOverride = skinColor;
                    }

                    if (i > 2){
                        itemInstance.Visible = false;
                    }
                    GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/" + type).AddChild(itemInstance);
                }
                i++;
            }
        }
        GetNode<OptionButton>("UI/VBoxContainer/HairOption").Connect("item_selected", this, "hairSelected");
        GetNode<OptionButton>("UI/VBoxContainer/EyesOption").Connect("item_selected", this, "eyesSelected");
        GetNode<OptionButton>("UI/VBoxContainer/NosesOption").Connect("item_selected", this, "noseSelected");
        GetNode<ColorPickerButton>("UI/VBoxContainer/HairColor").Connect("color_changed", this, "hairColorChanged");
        GetNode<ColorPickerButton>("UI/VBoxContainer/SkinColor").Connect("color_changed", this, "skinColorChanged");
        GetNode<HSlider>("UI/CharacterRotationSlider").Connect("value_changed", this, "characterRotated");
        GetNode<Button>("UI/DoneBtn").Connect("pressed", this, "donePressed");
        GetNode<Button>("Confirm/ConfirmMenu/ConfirmBtn").Connect("pressed", this, "confirmPressed");
        GetNode<Button>("Confirm/ConfirmMenu/CancelBtn").Connect("pressed", this, "cancelConfirmationPressed");
    }

    private Godot.Collections.Array getFilesInDirectory(string pathToParentFolder, string folderName){
        Godot.Collections.Array files = new Godot.Collections.Array();
        Directory dir = new Directory();
        dir.Open(pathToParentFolder + folderName);
        dir.ListDirBegin();
        files.Add(folderName);

        while (true){
            string file = dir.GetNext();
            if (file == ""){
                break;
            } else if (file.EndsWith(".glb")){
                string fileTrimmed = file.Remove(file.Length - 4, 4);
                files.Add(fileTrimmed.Capitalize());
            }
        }
        dir.ListDirEnd();
        return files;
    }

    private void hairSelected(int index){
        foreach (Spatial node in GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Hair").GetChildren()){
            node.Visible = false;
        }
        GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Hair/" + GetNode<OptionButton>("UI/VBoxContainer/HairOption").GetItemText(index).ToLower()).Visible = true;
    }

    private void eyesSelected(int index){
        foreach (Spatial node in GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Eyes").GetChildren()){
            node.Visible = false;
        }
        GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Eyes/" + GetNode<OptionButton>("UI/VBoxContainer/EyesOption").GetItemText(index).ToLower()).Visible = true;
    }
    
    private void noseSelected(int index){
        foreach (Spatial node in GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Noses").GetChildren()){
            node.Visible = false;
        }
        GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Noses/" + GetNode<OptionButton>("UI/VBoxContainer/NosesOption").GetItemText(index).ToLower()).Visible = true;
    }

    private void hairColorChanged(Color newColor){
        foreach (Spatial hairStyle in GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Hair").GetChildren()){
            hairStyle.GetChild<MeshInstance>(0).MaterialOverride.Set("albedo_color", newColor);
        }
    }

    private void skinColorChanged(Color newColor){
        foreach (Spatial noseType in GetNode<Spatial>("Char/Armature/Skeleton/HeadAttachment/Position3D/Noses").GetChildren()){
            noseType.GetChild<MeshInstance>(0).MaterialOverride.Set("albedo_color", newColor);
        }
    }

    private void characterRotated(float rotation){
        GetNode<KinematicBody>("Char").RotationDegrees = new Vector3(0, 26 + rotation, 0);
    }

    private void donePressed(){
        GetNode<Control>("UI").Visible = false;
        GetNode<Control>("Confirm").Visible = true;
        GetNode<Control>("Confirm/ConfirmMenu").Visible = true;
    }

    private void confirmPressed(){
        GetNode<Control>("Confirm/ConfirmMenu").Visible = false;
        GetNode<Control>("Confirm/Checks").Visible = true;
        string hairStyle = GetNode<OptionButton>("UI/VBoxContainer/HairOption").GetItemText(GetNode<OptionButton>("UI/VBoxContainer/HairOption").GetSelectedId());
        string eyesType = GetNode<OptionButton>("UI/VBoxContainer/EyesOption").GetItemText(GetNode<OptionButton>("UI/VBoxContainer/EyesOption").GetSelectedId());
        string noseType = GetNode<OptionButton>("UI/VBoxContainer/NosesOption").GetItemText(GetNode<OptionButton>("UI/VBoxContainer/NosesOption").GetSelectedId());
        Color hairColor = GetNode<ColorPickerButton>("UI/VBoxContainer/HairColor").Color;
        Color skinColor = GetNode<ColorPickerButton>("UI/VBoxContainer/SkinColor").Color;
        Server Server = GetNode<Server>("/root/Server");
        Server.SendCharacterDatas(hairStyle, eyesType, noseType, hairColor, skinColor);
    }

    private void cancelConfirmationPressed(){
        GetNode<Control>("Confirm/ConfirmMenu").Visible = false;
        GetNode<Control>("Confirm").Visible = false;
        GetNode<Control>("UI").Visible = true;
    }
}
