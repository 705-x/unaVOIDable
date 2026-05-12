using Godot;
using System;


//! Class for the Voideye enemy. 
public partial class Voideye : CharacterBody2D
{
    
	[Export] public float Damage = 10.0f; //!<Damage done per tick to the player if they are too close to the Voideye.
    [Export] public float AttackRange = 1.5f;
    public Player player;

    public override void _Ready()
    {
		player = GetNode<Player>("/root/Main/Player");
    }

	public override void _PhysicsProcess(double delta)
	{
        //!Checks the distance between the player and the Voideye. If the player is within range, attacks.
		float distance = GlobalTransform.Origin.DistanceTo(player.GlobalPosition);

        if (distance < AttackRange)
        {
            Attack();
        }
	}

	private void Attack()
    {
        //!Damages the player.
        if (player is Player playerBody)
        {
            playerBody.Damaged(20);
        }
    }
}
