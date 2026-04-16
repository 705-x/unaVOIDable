using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Testing : Node
{	
	TileMapLayer tileLayer;
	Player player;
	Array<PackedScene> roomArray = [];
	PackedScene SEQUENCE_CHAIN;
	HashSet<Vector2I> usedCells = new();
	System.Collections.Generic.Dictionary<PackedScene, Vector2I> roomSizeCache = new();
	LightManager lightManager;
	WorldObjectManager objManager;
	Random rand = new();
	int GeneratedRoomCounter = 0;
	public override void _Ready()
	{
		
		tileLayer = GetNode<TileMapLayer>("TileMapLayer");
		lightManager = new(this, tileLayer);
		objManager = new(this, tileLayer);
		PackedScene teleporter = ResourceLoader.Load<PackedScene>("res://scenes/rooms/LVL0_structures/TELEPORTER.tscn");

		player = (Player)GetNode("Player");
		player.worldObjectManager = objManager;
		player.tileLayer = tileLayer;
		//shi passed to player by reference
		
		LoadLevelRooms(0);
		GenerateFloor(5000, 7, 2);
		SolidifyOutlines();
		GD.Print(GeneratedRoomCounter);	
		lightManager.CreateLights();
		objManager.loadObjects();
		objManager.spawnWorldObjects();

		GenerateRoom(usedCells.ElementAt(rand.Next(usedCells.Count)), teleporter);
		var timer = new Timer();
    	timer.WaitTime = 0.5f;
    	timer.Autostart = true;
    	AddChild(timer);
    	timer.Timeout += () => lightManager.UpdateNearPlayer((Vector2I)player.Position, 35);

		
	}
	public void GenerateFloor(int howMany, int blockinessCoeff, int emptinessCoeff)
	{
    	var graph = GenerateFloorGraph(howMany, blockinessCoeff);

    	RoomNode start = graph[0];
    	CalculateRoomDepths(start);

    	Queue<RoomNode> queuedRooms = new();
    	HashSet<Vector2I> spawnedRooms = new();
		List<RoomNode> availableRooms = new();

    	Vector2I[] directions =
    	{
        	Vector2I.Up,
        	Vector2I.Down,
        	Vector2I.Right,
        	Vector2I.Left
    	};

    	start.Position = new Vector2I(0,0);
    	start.size = GetRoomSize(SEQUENCE_CHAIN);

    	GenerateRoom(start.Position, SEQUENCE_CHAIN);

    	queuedRooms.Enqueue(start);
    	spawnedRooms.Add(start.Position);

    	while(queuedRooms.Count > 0)
    	{
        	var currentRoom = queuedRooms.Dequeue();

        	foreach(var conn in currentRoom.Conns)
        	{
           		// room already placed
            	if(conn.Position != Vector2I.Zero || conn == start)
                	continue;

            	bool placed = false;

				Shuffle<Vector2I>(directions);
            	foreach(var dir in directions)
            	{
                	var chance = rand.Next(0, emptinessCoeff + 1);

                	PackedScene roomToGenerate;
					if(chance == emptinessCoeff)
					{roomToGenerate = SEQUENCE_CHAIN;}
					else
					{roomToGenerate = roomArray.PickRandom();}

                	Vector2I toGenSize = GetRoomSize(roomToGenerate);
                	Vector2I newPos = currentRoom.Position;

                	if (dir == Vector2I.Up || dir == Vector2I.Down)
                    	newPos.Y += (currentRoom.size.Y / 2 + toGenSize.Y / 2) * dir.Y;
                	else
                    	newPos.X += (currentRoom.size.X / 2 + toGenSize.X / 2) * dir.X;

					//adds half of the room's sizes in tiles in it's target generation direction so the rooms are right next to eachother

                	if(spawnedRooms.Contains(newPos)){continue;} //checks if a room is already spawned at that exact position, if not, continues

                	if(!GenerateRoom(newPos, roomToGenerate)){continue;} //checks if the room can generate at all, -"-
                    	
                	// success
                	conn.Position = newPos;
                	conn.size = toGenSize;
                	queuedRooms.Enqueue(conn);
                	spawnedRooms.Add(newPos);
					availableRooms.Add(conn);
                	placed = true;
                	break;
            }	
            	if(!placed)
            	{
                	var randomRoom = availableRooms[rand.Next(availableRooms.Count)];
    				queuedRooms.Enqueue(randomRoom);
            	}
        }
    }
		
	}
	public List<RoomNode> GenerateFloorGraph(int count, int blockinessCoeff)
	{
		//BlockinessCoeff is the chance to generate a connected room. The smaller it is - the more
		//linear the level will be(but also less rooms will gen, this should be fixed)
    	var rooms = new List<RoomNode>();

    	for(int i = 0; i < count; i++)
        	rooms.Add(new RoomNode{ Id = i });

    	for(int i = 1; i < rooms.Count; i++)
    	{
			var chance = rand.Next(0,blockinessCoeff+1);
			if(chance == blockinessCoeff)
			{
				int randomIndex = rand.Next(0, i);
				ConnectFloorRooms(rooms[i], rooms[randomIndex]);
			}
        	ConnectFloorRooms(rooms[i], rooms[i-1]);
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
		//lol idek if i needed a function for this shit but oh well
	}
	public bool GenerateRoom(Godot.Vector2I tilePos, PackedScene roomScene)
	{
		Node2D room = (Node2D)roomScene.Instantiate();
		room.Position = tileLayer.MapToLocal(tilePos);
		TileMapLayer roomTiles = (TileMapLayer)room.GetChild(0);
		var cells = roomTiles.GetUsedCells();
		foreach(var cell in cells)
		{
			if(usedCells.Contains(cell+tilePos))
			{
				//GD.Print("SPACE TAKEN UP. STOPPING GENERATION.");
				room.QueueFree();
				return false;
			}
		}

		//this checks if any cells that would get spawned by the room already exist in usedCells
		
		foreach (Vector2I cell in cells)
		{
			int tileId = roomTiles.GetCellSourceId(cell);
			Vector2I tileAtlas = roomTiles.GetCellAtlasCoords(cell);
			var tileAlt = roomTiles.GetCellAlternativeTile(cell);
			tileLayer.SetCell(cell+tilePos, tileId, tileAtlas, tileAlt);
			usedCells.Add(cell+tilePos);
		}

		//this just pastes the cells from the room onto the map. Could probably be done way quicker.

		var markers = room.GetNodeOrNull("LightMarkers");
    	if (markers != null)
        	foreach (Node2D marker in markers.GetChildren())
            	lightManager.lightPositions.Add(tileLayer.LocalToMap((Vector2I)marker.Position) + tilePos);

		//this gets the Marker2Ds of each light and adds them to a Hashset.

		markers = room.GetNodeOrNull("PropMarkers");
    	if (markers != null)
        	foreach (Node2D marker in markers.GetChildren())
            	objManager.propMarkers.Add(tileLayer.LocalToMap((Vector2I)marker.Position) + tilePos);

		markers = room.GetNodeOrNull("ItemMarkers");
    	if (markers != null)
        	foreach (Node2D marker in markers.GetChildren())
            	objManager.itemMarkers.Add(tileLayer.LocalToMap((Vector2I)marker.Position) + tilePos);

		//this gets the Marker2Ds of every prop spawn location and adds them to the ObjManager

		room.QueueFree();
		GeneratedRoomCounter++;
		return true;
	}
	Vector2I GetRoomSize(PackedScene scene)
	{	
    	if(roomSizeCache.TryGetValue(scene, out var size))
       		return size;
		//if that room's size alr exists, then this returns instead of instatiating and saving.

    	Node2D room = (Node2D)scene.Instantiate();
    	TileMapLayer tiles = (TileMapLayer)room.GetChild(0);
    	size = tiles.GetUsedRect().Size;

    	room.QueueFree();
    	roomSizeCache[scene] = size;
    	return size;
		//if it didn't exist this instantiates it and saves the size to the dictionary
	}
	public void LoadLevelRooms(int level)
	{
		string folderString = $"res://scenes/rooms/LVL{level}/";
		SEQUENCE_CHAIN = GD.Load<PackedScene>(folderString + "CHAIN.tscn");
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
	public void SolidifyOutlines()
	{
		Vector2I[] directions =
			{
				Vector2I.Up,
				Vector2I.Down,
				Vector2I.Right,
				Vector2I.Left,
				new(1,1),
				new(1,-1),
				new(-1,1),
				new(-1,-1),
			};
		foreach (var cell in usedCells)
    	{
        	foreach (var dir in directions)
        	{
            	if (!usedCells.Contains(cell + dir))
            	{
                	tileLayer.SetCell(cell, 0, new(0, 0), 0);
                	break;
            	}
        	}
		}

		//checks if any tiles adjacent to the one checked is empty. If so - makes it a wall cell.
	}
	public void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
		//got this off of stackOverflow LOL it does it's job and i feel like is more efficient than the previous method
    }
	public override void _Process(double delta)
	{
		
	}
}
