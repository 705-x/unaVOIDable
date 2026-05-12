using Godot;
using System;


//! Class for the Smiler enemy. Currently isn't working as expected, mainly due to pathfinding performance problems.
public partial class Smiler : CharacterBody2D
{
	
	[Export] public float Speed = 400.0f;
    [Export] public float Damage = 10.0f;
    [Export] public float AttackRange = 500.0f;

    private NavigationAgent2D agent;
    public Player player;

	private bool canAttack = true;
	private float attackCooldown = 1.5f;

	private float repathTimer = 0f;
	private float repathDelay = 1.0f;

    public override void _Ready()
    {
		player = GetNode<Player>("/root/Main/Player");
        agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
    }
	

	public override void _PhysicsProcess(double delta)
	{
		if (player == null)
			return;

		repathTimer -= (float)delta;

		float distance = GlobalPosition.DistanceTo(player.GlobalPosition);

		if (repathTimer <= 0f)
		{
			agent.TargetPosition = player.GlobalPosition;
			repathTimer = repathDelay;
			
		}
		Vector2 nextPosition = agent.GetNextPathPosition();
		Vector2 direction = (nextPosition - GlobalPosition).Normalized();
		Velocity = direction * Speed;
		MoveAndSlide();


		if (distance < AttackRange)
		{
			Attack();
		}
	}

	

	private async void Attack()
	{
		if (!canAttack)
			return;

		canAttack = false;

		if (player is Player playerBody)
		{
			playerBody.Damaged(20);
		}

		await ToSignal(GetTree().CreateTimer(attackCooldown), SceneTreeTimer.SignalName.Timeout);

		canAttack = true;
	}
}
