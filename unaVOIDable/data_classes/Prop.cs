using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[GlobalClass]
public partial class Prop : Resource
{
	[Export]
	public Texture2D texture;
	[Export]
	public float Mass;
	[Export]
	public bool Tall;
	[Export]
	public int spawnChance; //in promiles, chance to be spawned when it is picked at random. This works like shit. I think.
}
