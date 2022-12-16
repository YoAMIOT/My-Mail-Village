using Godot;
using System;

public class AddressManager : Node{
    public Godot.Collections.Dictionary addresses;
    public override void _Ready(){
        addresses = new Godot.Collections.Dictionary(){
            //first row
            {"a", new Godot.Collections.Dictionary(){{"minX", 0}, {"minY", 0}, {"maxX", 9}, {"maxY", 9}}},
            {"b", new Godot.Collections.Dictionary(){{"minX", 10}, {"minY", 0}, {"maxX", 19}, {"maxY", 9}}},
            {"c", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 0}, {"maxX", 29}, {"maxY", 9}}},
            {"d", new Godot.Collections.Dictionary(){{"minX", 30}, {"minY", 0}, {"maxX", 39}, {"maxY", 9}}},
            {"e", new Godot.Collections.Dictionary(){{"minX", 40}, {"minY", 0}, {"maxX", 49}, {"maxY", 9}}},
            //second row
            {"f", new Godot.Collections.Dictionary(){{"minX", 0}, {"minY", 10}, {"maxX", 9}, {"maxY", 19}}},
            {"g", new Godot.Collections.Dictionary(){{"minX", 10}, {"minY", 10}, {"maxX", 19}, {"maxY", 19}}},
            {"h", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 10}, {"maxX", 29}, {"maxY", 19}}},
            {"i", new Godot.Collections.Dictionary(){{"minX", 30}, {"minY", 10}, {"maxX", 39}, {"maxY", 19}}},
            {"j", new Godot.Collections.Dictionary(){{"minX", 40}, {"minY", 10}, {"maxX", 49}, {"maxY", 19}}},
            //third row
            {"k", new Godot.Collections.Dictionary(){{"minX", 0}, {"minY", 20}, {"maxX", 9}, {"maxY", 29}}},
            {"l", new Godot.Collections.Dictionary(){{"minX", 10}, {"minY", 20}, {"maxX", 19}, {"maxY", 29}}},
            {"m", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 20}, {"maxX", 29}, {"maxY", 29}}},
            {"n", new Godot.Collections.Dictionary(){{"minX", 30}, {"minY", 20}, {"maxX", 39}, {"maxY", 29}}},
            {"o", new Godot.Collections.Dictionary(){{"minX", 40}, {"minY", 20}, {"maxX", 49}, {"maxY", 29}}},
            //fourth row
            {"p", new Godot.Collections.Dictionary(){{"minX", 0}, {"minY", 30}, {"maxX", 9}, {"maxY", 39}}},
            {"q", new Godot.Collections.Dictionary(){{"minX", 10}, {"minY", 30}, {"maxX", 19}, {"maxY", 39}}},
            {"r", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 30}, {"maxX", 29}, {"maxY", 39}}},
            {"s", new Godot.Collections.Dictionary(){{"minX", 30}, {"minY", 30}, {"maxX", 39}, {"maxY", 39}}},
            {"t", new Godot.Collections.Dictionary(){{"minX", 40}, {"minY", 30}, {"maxX", 49}, {"maxY", 39}}},
            //fift row
            {"u", new Godot.Collections.Dictionary(){{"minX", 0}, {"minY", 40}, {"maxX", 9}, {"maxY", 49}}},
            {"v", new Godot.Collections.Dictionary(){{"minX", 10}, {"minY", 40}, {"maxX", 19}, {"maxY", 49}}},
            {"w", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 40}, {"maxX", 29}, {"maxY", 49}}},
            {"x", new Godot.Collections.Dictionary(){{"minX", 30}, {"minY", 40}, {"maxX", 39}, {"maxY", 49}}},
            {"y", new Godot.Collections.Dictionary(){{"minX", 40}, {"minY", 40}, {"maxX", 49}, {"maxY", 49}}},
            //last section
            {"z", new Godot.Collections.Dictionary(){{"minX", 20}, {"minY", 50}, {"maxX", 29}, {"maxY", 59}}}
        };
    }

    public void allocateAddressSlot(string username, string letter,  Vector2 slotCoordinates){
        Vector2 min = new Vector2((int)(addresses[letter] as Godot.Collections.Dictionary)["minX"],(int)(addresses[letter] as Godot.Collections.Dictionary)["minY"]);
        Vector2 max = new Vector2((int)(addresses[letter] as Godot.Collections.Dictionary)["maxX"],(int)(addresses[letter] as Godot.Collections.Dictionary)["maxY"]);
        if((slotCoordinates.x >= min.x && slotCoordinates.x <= max.x) && (slotCoordinates.y >= min.y && slotCoordinates.y <= max.y)){
            bool alreadyAllocated = false;
            foreach (string s in (addresses[letter] as Godot.Collections.Dictionary).Keys){
                if(s != "minX" && s != "minY" && s != "maxX" && s != "maxY"){
                    Godot.Collections.Dictionary allocatedAddress = (addresses[letter] as Godot.Collections.Dictionary)[s]as Godot.Collections.Dictionary;
                    Vector2 allocatedCoords = new Vector2((int)allocatedAddress["x"], (int)allocatedAddress["y"]);
                    if(slotCoordinates == allocatedCoords){
                        alreadyAllocated = true;
                    }
                }
            }
            if(alreadyAllocated == false){
                (addresses[letter] as Godot.Collections.Dictionary)[username] = new Godot.Collections.Dictionary(){{"x", slotCoordinates.x}, {"y", slotCoordinates.y}};
            }
        }
    }
}
