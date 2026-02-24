using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Numerics;
using System.Xml.Schema;
using System.Xml.XPath;

public partial class Testing : Node
{	
	TileMapLayer tileLayer;
	Array<PackedScene> roomArray = [];
	Random rand = new();
	PackedScene SEQUENCE_START;
	public override void _Ready()
	{
		tileLayer = GetNode<TileMapLayer>("TileMapLayer");
		LoadLevelRooms(0);
		var packedScene = GD.Load<PackedScene>("res://scenes/groundItem/item_node_2d.tscn");
		Item item = GD.Load<Item>("res://scenes/test(main)/TestItem.tres");
		spawnItem(new(10,10), packedScene, item);
	
		packedScene = GD.Load<PackedScene>("res://scenes/prop/prop_node_2d.tscn");
		Prop prop = GD.Load<Prop>("res://scenes/test(main)/Crate.tres");
		spawnProp(new(10,10), packedScene, prop);

		GenerateFloor(100);

	}
		
	public bool IsCellOccupied(Godot.Vector2I pos)
	{
		return tileLayer.GetCellSourceId(pos) != -1;	
	}
		
	public void GenerateFloor(int howMany)
	{	
		var graph = GenerateFloorGraph(howMany);
		RoomNode start = graph[0];
		CalculateRoomDepths(start);
		Queue<RoomNode> QueuedRooms = new();
		HashSet<Vector2I> SpawnedRooms = new();	
		Vector2I[] directions =
		{
			Vector2I.Up,
			Vector2I.Down,
			Vector2I.Right,
			Vector2I.Left
		};
		start.Position = new(0,0);
		GenerateRoom(start.Position, SEQUENCE_START);
		SpawnedRooms.Add(start.Position);
		QueuedRooms.Enqueue(start);

		while(QueuedRooms.Count > 0)
		{
			var currentRoom = QueuedRooms.Dequeue();
			SpawnedRooms.Add(currentRoom.Position);
			foreach(var conn in currentRoom.Conns)
			{
				if(conn.Position != Vector2I.Zero || conn == start) continue;
				
				foreach(var dir in directions.OrderBy(x=>rand.Next())){
					
					var roomToGenerate = roomArray.PickRandom();
					var newPos = currentRoom.Position;
					var currentSize = currentRoom.size;
					var toGenSize = getRoomSize(roomToGenerate);

					if (dir == Vector2I.Up || dir == Vector2I.Down)
						newPos.Y += (currentSize.Y/2 + toGenSize.Y/2) * dir.Y;
					else
						newPos.X += (currentSize.X/2 + toGenSize.X/2) * dir.X;
						
					if (!SpawnedRooms.Contains(newPos))
					{
						if(GenerateRoom(newPos, roomToGenerate)){
							conn.size = getRoomSize(roomToGenerate);
							conn.Position = newPos;
    						QueuedRooms.Enqueue(conn);
    						SpawnedRooms.Add(newPos);
    						break;
						}
					}
				}
			}	
		}
	}

	public List<RoomNode> GenerateFloorGraph(int count)
	{
    	var rooms = new List<RoomNode>();

    	for(int i = 0; i < count; i++)
        	rooms.Add(new RoomNode{ Id = i });

    	for(int i = 1; i < rooms.Count; i++)
    	{
        	int randomIndex = rand.Next(0, i);
        	ConnectFloorRooms(rooms[i], rooms[randomIndex]);
    	}

   		return rooms;
	}
	public void CalculateRoomDepths(RoomNode start)
	{
		Queue<RoomNode> queue = new();
		HashSet<RoomNode> visited = new();
		
		start.Depth = 0;
		queue.Enqueue(start);

		while(queue.Count > 0)
		{
			var current = queue.Dequeue();
			visited.Add(current);

			foreach(var conn in current.Conns)
			{
				if(visited.Contains(conn)) continue;

				conn.Depth = current.Depth + 1;
				queue.Enqueue(conn);
			}
		}
	}
	void ConnectFloorRooms(RoomNode a, RoomNode b)
	{
    	a.Conns.Add(b);
    	b.Conns.Add(a);
	}
	public bool GenerateRoom(Godot.Vector2I TilePos, PackedScene roomScene)
	{
		Node2D room = (Node2D)roomScene.Instantiate();
		room.Position = tileLayer.MapToLocal(TilePos);
		TileMapLayer roomTiles = (TileMapLayer)room.GetChild(0);
		foreach(var cell in roomTiles.GetUsedCells())
		{
			if(tileLayer.GetUsedCells().Contains(cell+TilePos))
			{
				GD.Print("SPACE TAKEN UP. STOPPING GENERATION.");
				return false;
			}
		}

		foreach (Vector2I cell in roomTiles.GetUsedCells())
		{
			int tileId = roomTiles.GetCellSourceId(cell);
			Vector2I tileAtlas = roomTiles.GetCellAtlasCoords(cell);
			var tileAlt = roomTiles.GetCellAlternativeTile(cell);
			tileLayer.SetCell(cell+TilePos, tileId, tileAtlas, tileAlt);
		}
		GD.Print("Room generated at: " + TilePos);
		return true;
	}

	public Vector2I getRoomSize(PackedScene roomScene)
	{
		Node2D room = (Node2D)roomScene.Instantiate();
		TileMapLayer roomTiles = (TileMapLayer)room.GetChild(0);
		return roomTiles.GetUsedRect().Size;
	}
	public void LoadLevelRooms(int level)
	{
		string folderString = $"res://scenes/rooms/LVL{level}/";
		SEQUENCE_START = GD.Load<PackedScene>(folderString + "BEGIN.tscn");
		foreach(var fileName in DirAccess.GetFilesAt(folderString))
		{
			switch (fileName.GetExtension())
			{
				case ".import":
					fileName.ReplaceN(".import", "");
					break;
				case ".remap":
					fileName.ReplaceN(".remap", "");
					break;
			}
			roomArray.Add(GD.Load<PackedScene>(folderString + fileName));
		}
	}

	public void spawnProp(Godot.Vector2 pos, PackedScene propScene, Prop propData)
	{
		var worldPropNode = propScene.Instantiate();
		var worldProp = worldPropNode as PropNode2D;
		worldProp.propData = propData.Duplicate() as Prop;
		worldProp.GlobalPosition = new(200,200);
		GetTree().CurrentScene.AddChild(worldProp);
	}

	public void spawnItem(Godot.Vector2 pos, PackedScene itemScene, Item itemData)
	{
		var worldItemNode = itemScene.Instantiate();
		if (worldItemNode is not ItemNode2D worldItem)
		{
			GD.PrintErr("cannot cast root to RigidBody2D!");
			return;
		}
		worldItem.ItemData = itemData.Duplicate() as Item;
		worldItem.GlobalPosition = new (100, 100);
		GetTree().CurrentScene.AddChild(worldItem);
	}

	public override void _Process(double delta)
	{
	}
}
