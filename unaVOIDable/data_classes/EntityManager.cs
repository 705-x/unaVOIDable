using Godot;
using System;
using System.Collections.Generic;


//! Class for spawning and managing entities being spawned in the world.
[GlobalClass]
public partial class EntityManager : RefCounted
{
    
    
    
    
    private Node parent; //!< Parent node to which entities will be added.
    private TileMapLayer tileLayer; //!<tileLayer TileLayer of the parent node. Needed for map to global conversion.
    Godot.Collections.Array<PackedScene> entities = new(); //!<entities Array of all the PackedScenes of entities.
    Random rnd = new();

    public EntityManager(Node _parent, TileMapLayer _tileLayer, PackedScene _smiler, PackedScene _voideye)
    {
        //!Constructor for EntityManager.
        parent = _parent;
        tileLayer = _tileLayer;
        entities.Add(_voideye);

    }

    public void SpawnNearPlayer(Vector2I playerPos, float radius, HashSet<Vector2I> occupiedTiles)
    {
        //!
        //! This function is used to make sure enemies spawn somewhere near the player, not at random somewhere in the map.
        //! @param playerPos Passed as global position, which this function later converts to tiles.
        //! @param radius Expressed in tiles, radius of the area in which the entities can spawn.
        //! @param occupiedTiles Determines which tiles are within the radius in perspective to the player,
        //! from which a random tile is chosen to spawn an enemy onto.
        //!
        Vector2I playerWorld = tileLayer.LocalToMap(playerPos);
        
        Godot.Collections.Array<Vector2I> candidateTiles = new();
        

        //tiles which are near the player at that moment
        foreach(var tile in occupiedTiles)
        {
            if(tile.DistanceTo(playerWorld) < radius)
            {
                candidateTiles.Add(tile);
            }
        }

        int chance = rnd.Next(2);
        if(chance == 1)
        {
            Spawn(candidateTiles.PickRandom(), entities.PickRandom());
        }

    }

    public void Spawn(Vector2I pos, PackedScene enemy)
    {
        //! Spawns an entity at the given position.  
        //! @param pos Position in tiles.
        //!     @param enemy PackedScene of the enemy to spawn.
        GD.Print("spawned");
        var enemyNode = enemy.Instantiate();
        var worldEnemy = enemyNode as CharacterBody2D;
        worldEnemy.GlobalPosition = tileLayer.ToGlobal(pos);
        parent.AddChild(worldEnemy);
    }
}