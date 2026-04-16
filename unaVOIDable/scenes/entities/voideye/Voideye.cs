using Godot;
using System;

public partial class Voideye : CharacterBody2D
{
	[Export] public float Damage = 10.0f;
    [Export] public float AttackRange = 1.5f;

	public override void _PhysicsProcess(double delta)
	{
		
	}
}
