    using Godot;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;



    public partial class EquipmentInventory:GodotObject
    {
        //EquipmentType is an enum of which slot it should be equippable to, declared in Item.cs
        [Signal]
        public delegate void InventoryChangedEventHandler();    
        [Signal]
        public delegate void DropItemEventHandler(Item item);
        //Slot needs to be converted from it's enum to int, since my custon enum cannot be conv.
        //to Godot.Variant and Godot doesn't accept that.
        public Dictionary<EquipmentType, List<Item>> slots;
        public KeyValuePair<EquipmentType, int> activeSlot;
        private Dictionary<EquipmentType, int> slotLimits = new()
        {
            { EquipmentType.HeadGear, 1 },
            { EquipmentType.Armor, 1 },
            { EquipmentType.Backpack, 1 },
            { EquipmentType.LargeItem, 1 },
            { EquipmentType.SmallItem, 2 },
            { EquipmentType.Consumable, 2 }
        };  

        //slot limits for each slot type. I have no other idea on how to do this lul
        public EquipmentInventory()
        {   
            slots = new();
            foreach (var type in Enum.GetValues(typeof(EquipmentType)))
            {
                slots[(EquipmentType)type] = new();
            }
            //Declares a new list in each of the slots, so none of them throw a nullrefExcept. Eh.

        }
        public bool Equip(EquipmentType slot, Item item)
        {
            var items = slots[slot];    

            if (items.Count >= slotLimits[slot])
                return false;
            activeSlot = new(slot, items.Count);
            GD.Print(slot, items.Count);
            //activeSlot is set here since the size is always going to be 1 bigger then the last index,
            //so the size is grabbed BEFORE a new element is added to reflect the index of the last item adter the Add().

            //completely illogical and frankly, idiotic. But it's my code.
            items.Add(item);
            EmitSignal(SignalName.InventoryChanged);
            return true;
            //to implement this is js a placeholder
        }

        public bool Drop(EquipmentType slot, int which)
        {
            var item = slots[slot];

            if (item.Count == 0)
                return false;
            
            EmitSignal(SignalName.DropItem, item[which]);
            //casting the enum to int. Can be converted back by casting the int into the enum type

            item.RemoveAt(which);
            if (activeSlot.Key == slot)
            {
                if (item.Count == 0)
                {
                    activeSlot = default;
                }
                else if (activeSlot.Value >= item.Count)
                {
                    activeSlot = new(slot, item.Count - 1);
                }
            }
            //sets the active slot properly so it works
            EmitSignal(SignalName.InventoryChanged);
            return true;
        }

        public Item getActiveItem()
        {
            if (!slots.ContainsKey(activeSlot.Key))
                return null;

            var list = slots[activeSlot.Key];

            if (activeSlot.Value < 0 || activeSlot.Value >= list.Count)
                return null;

            return list[activeSlot.Value];
        }

        public bool RemoveActiveItem()
        {
            if (!slots.ContainsKey(activeSlot.Key))
                return false;

            var list = slots[activeSlot.Key];

            if (activeSlot.Value < 0 || activeSlot.Value >= list.Count)
                return false;

            list.RemoveAt(activeSlot.Value);
            if (list.Count == 0)
            {
                activeSlot = default;
            }
            else if (activeSlot.Value >= list.Count)
            {
                activeSlot = new(activeSlot.Key, list.Count - 1);
            }

            EmitSignal(SignalName.InventoryChanged);
            return true;
        }
    }
