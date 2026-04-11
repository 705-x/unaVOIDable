using Godot;
using System;
using System.Collections.Generic;

public partial class Hud : CanvasLayer
{

	Sprite2D bloodSprite;	
	ShaderMaterial material;
	Player player;
	EquipmentInventory playerInventory;
	Dictionary<EquipmentType, List<Control>> uiSlots = new();

	public override void _Ready()
	{
		material = (ShaderMaterial)GetNode<ColorRect>("ColorRect").Material;
		bloodSprite = (Sprite2D)GetNode("Control/Sprite2D");
		player = GetNode<Player>("../Player");
		playerInventory = player.playerInventory;

		player.HealthChanged += DrawDamage;
		player.SelectedSlot += DrawHUD;
		playerInventory.InventoryChanged += DrawInventory;
		playerInventory.InventoryChanged += DrawHUD;


		foreach (Control slotNode in GetNode("EquippedItems").GetChildren())
		{
			var type = (EquipmentType)Enum.Parse(typeof(EquipmentType), slotNode.Name, true);
			/*this determines the type based on the ControlNode's name. Proves problematic since two control
			nodes cannot have the same name, ergo only one of them can actually be parsed... 
			The simplest (and dumbest) fix is to just have the parse ignore cases and name the Control nodes
			with diffrent cases (eg. consumable, Consumable, CoNsUmAblE). Doesn't take much effort and should give
			me way more room than i'll ever need without the need of writing an overcomplicated helper method.*/
			if (!uiSlots.ContainsKey(type))
			{
    			uiSlots[type] = new List<Control>();
			}
			uiSlots[type].Add(slotNode);
		}
	}
	public override void _Process(double delta)
	{
	}

	public void DrawInventory()
	{
		
	}
	public void DrawHUD()
	{
		foreach(var kvp in playerInventory.slots)
		{
			EquipmentType type = kvp.Key;
        	List<Item> items = kvp.Value;
			if (!uiSlots.ContainsKey(type))
        		continue;
        	
			var controls = uiSlots[type];

        	for (int i = 0; i < controls.Count; i++)
        	{
				if (kvp.Key == playerInventory.activeSlot.Key 
    				&& i == playerInventory.activeSlot.Value
    				&& i < kvp.Value.Count)
				{
    				controls[i].SelfModulate = new(255,255,255, 0.2f);
    				GD.Print(kvp.Key, kvp.Value[i]);
				}
				else 
				{
					controls[i].SelfModulate = new(100,100,100, 0.2f);
				}
            	var icon = controls[i].GetNode<TextureRect>("Icon");

            	if (i < items.Count)
            	{
                	icon.Texture = items[i].Icon;
                	icon.Visible = true;
					
            	}
            	else
            	{
					icon.Texture = null;
                	icon.Visible = false;
            	}
        	}
		}

		//this draws ever
	}
	public void DrawDamage(int hp)
	{
		material.SetShaderParameter("saturation", hp/100.0f);
		float alpha = 0.5f - (hp / 100.0f);
		bloodSprite.SelfModulate = bloodSprite.SelfModulate with { A = alpha };
	}
}
