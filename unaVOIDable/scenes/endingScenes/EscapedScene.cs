using Godot;
using System;

public partial class EscapedScene : Control
{
	// Called when the node enters the scene tree for the first time.
	Button quit;
	public override void _Ready()
	{
		quit = GetNode<Button>("Button");
		quit.Pressed += Quit;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Quit()
	{
		GetTree().Quit();
	}
}
