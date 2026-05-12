using Godot;
using System;

//! Class handling the logic for when the player dies. Contains a button to restart the run.
public partial class DeadScene : Node
{
	
	// Called when the node enters the scene tree for the first time.
	Button retry;
	public override void _Ready()
	{
		retry = GetNode<Button>("Button");
		retry.Pressed += Retry;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Retry()
	{
		GetTree().ChangeSceneToFile("res://scenes/GAME(main)/testing.tscn");
	}
}
