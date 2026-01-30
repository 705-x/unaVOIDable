using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public enum EquipmentType
{
    Helmet,
    Armor,
    Backpack,
    Holdable
}

public class EquipmentInventory
{
    
    private Dictionary<EquipmentType, Item> slots;

    public EquipmentInventory()
    {   
        slots = new();
    }

    public bool Equip(EquipmentType slot, Item item)
    {
        if (slots.ContainsKey(slot)){
            return false;
        }
        slots[slot] = item;
        return true;
    }

    public bool Drop(EquipmentType slot)
    {
        if (slots.ContainsKey(slot)){
           
        }

        return false;
    }
}
