using System;
using Godot;

//! Consumable item that restores player health and stamina when used. Can optionally remove itself after all uses are depleted.
[GlobalClass]
public partial class RecoveryItem : Item
{
    [Export]
    public int healthReplenish = 25; //!< Amount of health restored on use.

    [Export]
    public int staminaReplenish = 25; //!< Amount of stamina restored on use.


    //! Restores player health and stamina, then decreases remaining uses if the item is consumable.
    public override void Use(Player player)
    {
        player.Heal(healthReplenish);

        player.AddStamina(staminaReplenish);


        //! Handles consumable durability / usage count logic.
        if (equipmentType == EquipmentType.Consumable)
        {
            if (this.uses > 0)
            {
                uses -= 1;
            }
            else
            {
                //! Removes the item once all uses are depleted.
                player.RemoveActiveItem();
            }
        }
    }
}