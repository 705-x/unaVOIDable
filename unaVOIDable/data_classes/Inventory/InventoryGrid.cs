using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

//!Currently unimplemented.

public class InventoryGrid
{
    public int slotAmount;

    private List<Item> items;

    public InventoryGrid(int _slotAmount)
    {
        slotAmount = _slotAmount;
        items = new List<Item>(slotAmount);
    }

}