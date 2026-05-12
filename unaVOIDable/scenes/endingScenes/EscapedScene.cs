using Godot;
using System;


//! Class handling the logic for when the player finishes the game. Contains a button to quit the game.
public partial class EscapedScene : Control
{
	
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
