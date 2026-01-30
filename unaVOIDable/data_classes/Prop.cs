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
}
