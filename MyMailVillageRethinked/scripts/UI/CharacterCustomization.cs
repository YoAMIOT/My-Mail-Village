using Godot;
using System;

public class CharacterCustomization : Spatial{
    public override void _Ready(){
        Godot.Collections.Array eyes = getFilesInDirectory("res://ressources/meshes/attributes/", nameof(eyes));
        Godot.Collections.Array noses = getFilesInDirectory("res://ressources/meshes/attributes/", nameof(noses));
        Godot.Collections.Array hair = getFilesInDirectory("", nameof(hair));
        Godot.Collections.Array[] attributes =  new Godot.Collections.Array[]{eyes, noses, hair};
        //For each attributes of the arrays add the item to the options
        foreach (Godot.Collections.Array attributeType in attributes){
            var i = 1;
            string type = "";
            foreach (string item in attributeType){
                if (i == 1){
                    type = item.Capitalize();
                    i++;
                } else {
                    GetNode<OptionButton>("UI/VBoxContainer/" + type + "Option").AddItem(item);
                }
            }
        }
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
                files.Add(fileTrimmed);
            }
        }

        dir.ListDirEnd();
        return files;
    }
}
