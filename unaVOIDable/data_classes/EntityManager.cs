using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class EntityManager : RefCounted
{
    private Node parent;
    private TileMapLayer tileLayer;
    Godot.Collections.Array<PackedScene> entities = new();
    Random rnd = new();

    public EntityManager(Node _parent, TileMapLayer _tileLayer, PackedScene _smiler, PackedScene _voideye)
    {
        parent = _parent;
        tileLayer = _tileLayer;
        entities.Add(_voideye);
        entities.Add(_smiler);
    }

    public void SpawnNearPlayer(Vector2I playerPos, float radius, HashSet<Vector2I> occupiedTiles)
    {
        Vector2I playerWorld = tileLayer.LocalToMap(playerPos);
        //converts the player's globalpos (pix) to tilemapPos
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
        GD.Print("spawned");
        var enemyNode = enemy.Instantiate();
        var worldEnemy = enemyNode as CharacterBody2D;
        worldEnemy.GlobalPosition = tileLayer.ToGlobal(pos);
        parent.AddChild(worldEnemy);
    }
}