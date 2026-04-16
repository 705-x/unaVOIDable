using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.

	Button quitButton;
	Button startButton;
	public override void _Ready()
	{
		quitButton = GetNode<Button>("QUIT");
		startButton = GetNode<Button>("BEGIN");
		quitButton.Pressed += Quit;
		startButton.Pressed += Start;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Quit()
	{
		GetTree().Quit();
	}

	public void Start()
	{
		GetTree().ChangeSceneToFile("res://scenes/GAME(main)/testing.tscn");
	}
}
