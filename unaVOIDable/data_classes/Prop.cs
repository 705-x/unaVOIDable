using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

//! Contains data for props. Also see PropNode2D.
[GlobalClass]
public partial class Prop : Resource
{
	
	[Export]
	public Texture2D texture; //!< Texture.
	[Export]
	public float Mass; //!<Determines how hard the object is to push.
	[Export]
	public bool Tall; //!<When false, casts shadows only if the player is crouched (not implemented)
	[Export]
	public int spawnChance; //!<Chance to spawn when it is chosen. See WorldObjectManager
}
