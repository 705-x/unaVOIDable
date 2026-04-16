using Godot;
using System;

public partial class Smiler : CharacterBody2D
{
	[Export] public float Speed = 4.0f;
    [Export] public float Damage = 10.0f;
    [Export] public float AttackRange = 1.5f;

    private NavigationAgent2D agent;
    public Player player;

    public override void _Ready()
    {
		player = GetNode<Player>("/root/Main/Player");
        agent = GetNode<NavigationAgent2D>("NavigationAgent2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null)
            return;

        agent.TargetPosition = player.GlobalPosition;

        Vector2 nextPosition = agent.GetNextPathPosition();
        Vector2 direction = (nextPosition - GlobalPosition).Normalized();
		GlobalRotation = (GlobalPosition - player.GlobalPosition).Angle();

        Velocity = direction * Speed;
        MoveAndSlide();

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
