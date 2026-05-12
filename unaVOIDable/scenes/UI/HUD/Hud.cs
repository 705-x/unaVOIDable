using Godot;
using System;
using System.Collections.Generic;

//! Handles all player HUD rendering including inventory slots, stamina bar, damage effects, and active item highlighting.
public partial class Hud : CanvasLayer
{
	Sprite2D bloodSprite; //!< Blood overlay sprite used for screen damage effects.

	TextureProgressBar healthBar; //!< UI health bar reference.

	TextureProgressBar staminaBar; //!< UI stamina bar reference.

	ShaderMaterial material; //!< Post-processing material used for screen effects.

	Player player; //!< Reference to the player instance.

	EquipmentInventory playerInventory; //!< Reference to the player's inventory.

	Dictionary<EquipmentType, List<Control>> uiSlots = new(); //!< Dictionary mapping equipment types to their corresponding UI slot controls.

	//! Initializes HUD references, connects player signals, builds slot mappings, and draws the initial HUD state.
	public override void _Ready()
	{
		material = (ShaderMaterial)GetNode<ColorRect>("ColorRect").Material;
		bloodSprite = (Sprite2D)GetNode("Control/Sprite2D");
		staminaBar = GetNode<TextureProgressBar>("StaminaBar");
		player = GetNode<Player>("../Player");
		playerInventory = player.playerInventory;
		player.HealthChanged += DrawDamage;
		player.StaminaChanged += DrawStamina;
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

		DrawHUD();
	}

	//! Handles per-frame HUD updates.
	public override void _Process(double delta)
	{
	}


	//! Updates inventory-related UI elements.
	public void DrawInventory()
	{
		
	}

	//! Draws equipped items, updates active slot highlighting, and refreshes item icons.
	public void DrawHUD()
	{
		foreach(var kvp in playerInventory.slots)
		{
			EquipmentType type = kvp.Key;
        	List<Item> items = kvp.Value;
			if (!uiSlots.ContainsKey(type))
        		continue;
        	
			var controls = uiSlots[type];

        	bool isActiveType = kvp.Key == playerInventory.activeSlot.Key;
			int activeIndex = playerInventory.activeSlot.Value;

			for (int i = 0; i < controls.Count; i++)
			{
    			bool isActiveSlot = isActiveType && i == activeIndex;

    			if(isActiveSlot)
				{
        			controls[i].SelfModulate = new Color(1, 1, 1, 0.4f);
				}else{
        			controls[i].SelfModulate = new Color(0.4f, 0.4f, 0.4f, 0.1f);
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

		//this draws slots and each selected slot
	}

	//! Applies visual damage effects based on the player's current health.
	public void DrawDamage(int hp)
	{
		material.SetShaderParameter("saturation", hp/100.0f);
		float alpha = 0.5f - (hp / 100.0f);
		bloodSprite.SelfModulate = bloodSprite.SelfModulate with { A = alpha };
	}
	//! Updates the stamina bar value.
	public void DrawStamina(float stamina)
	{
		staminaBar.Value = stamina;
	}
}
