using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class RoomNode : RefCounted
{
    public enum RoomType
    {
    Start,
    Normal,
    Boss,
    Shop
    }
    public int Id;
    public List<RoomNode> Conns = new();
    public Vector2I size = new();
    public Vector2I Position;
    public int Depth;
    public RoomType Type;
}