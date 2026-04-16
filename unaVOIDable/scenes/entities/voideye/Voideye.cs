using Godot;
using System;

public partial class Voideye : CharacterBody2D
{
	[Export] public float Damage = 10.0f;
    [Export] public float AttackRange = 1.5f;
    public Player player;

    public override void _Ready()
    {
		player = GetNode<Player>("/root/Main/Player");
    }

	public override void _PhysicsProcess(double delta)
	{
		float distance = GlobalTransform.Origin.DistanceTo(player.GlobalPosition);

        if (distance < AttackRange)
        {
            Attack();
        }
	}

	private void Attack()
    {
        if (player is Player playerBody)
        {
            playerBody.Damaged(20);
        }
    }
}
