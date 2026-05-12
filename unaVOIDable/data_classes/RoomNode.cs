using Godot;
using System;
using System.Collections.Generic;


//!Class containing data for Rooms. Almost exclusively used during floor generation. See Testing.
[GlobalClass]
public partial class RoomNode : RefCounted
{
    public int Id;
    public List<RoomNode> Conns = new(); //!<Connections of this particular room. 
    public List<Vector2I> lightPositions; 
    public Vector2I size = new(); //!<Room size. Used so rooms don't generate on top of eachother.
    public Vector2I Position;  //!<Position in the world.
    public int Depth; //!<Room's depth, defined as it's distance in rooms from the starting room.
}