using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class RoomNode : RefCounted
{
    public int Id;
    public List<RoomNode> Conns = new();
    public List<Vector2I> lightPositions;
    public Vector2I size = new();
    public Vector2I Position;
    public int Depth;
}