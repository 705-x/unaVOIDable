using System;
using Godot;

[GlobalClass]
public partial class RecoveryItem : Item
{
    [Export]
    public int healthReplenish = 25;
    [Export]
    public int staminaReplenish = 25;

    public override void Use(Player player)
    {
        player.Heal(healthReplenish);
        player.AddStamina(staminaReplenish);

        if(equipmentType == EquipmentType.Consumable)
        {
            if(this.uses > 0)
            {
                uses-=1;
            }else{
            player.RemoveActiveItem();
            }
        }
    }
}