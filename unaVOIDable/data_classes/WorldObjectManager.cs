using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[GlobalClass]
public partial class WorldObjectManager : RefCounted
{
    private Node parent;
    private TileMapLayer tileLayer;

    public HashSet<Vector2I> propMarkers = new();
    public HashSet<Vector2I> itemMarkers = new();

    Array<Prop> propsArray = new();
    Array<Item> itemsArray = new();

    Random rand = new();
    PackedScene propScene = GD.Load<PackedScene>("res://scenes/prop/prop_node_2d.tscn");
    PackedScene itemScene = GD.Load<PackedScene>("res://scenes/groundItem/item_node_2d.tscn");

    public WorldObjectManager(Node _parent, TileMapLayer _tileLayer)
    {
        parent = _parent;
        tileLayer = _tileLayer;
        
    }
    
    public void spawnWorldObjects()
    {
        foreach(var marker in propMarkers)
        {
            int chance = rand.Next(1000);
            Prop propToSpawn = propsArray.PickRandom();
            if(chance < propToSpawn.spawnChance)
            {
                SpawnProp(marker, propToSpawn);
            }
        }
        foreach(var marker in itemMarkers)
        {
            int chance = rand.Next(1000);
            Item itemToSpawn = itemsArray.PickRandom();
            if(chance < itemToSpawn.spawnChance)
            {
                SpawnItem(marker, itemToSpawn);
            }
        }
    }
    public void loadObjects()
    {
        string folderString = $"res://resources/props/";
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
			propsArray.Add(GD.Load<Prop>(folderString + fileName));
        }
        folderString = $"res://resources/items/";
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
			itemsArray.Add(GD.Load<Item>(folderString + fileName));
        }
    }
    public void SpawnProp(Vector2I pos, Prop propData)
    {
        var worldPropNode = propScene.Instantiate();
        var worldProp = worldPropNode as PropNode2D;
        worldProp.propData = propData.Duplicate() as Prop;
        worldProp.GlobalPosition = tileLayer.MapToLocal(pos);
        parent.AddChild(worldProp);
    }
    public void SpawnItem(Vector2I pos, Item itemData)
    {
        var worldItemNode = itemScene.Instantiate();
        if (worldItemNode is not ItemNode2D worldItem)
        {
            GD.PrintErr("cannot cast root to RigidBody2D");
            return;
        }
        worldItem.ItemData = itemData.Duplicate() as Item;
        worldItem.GlobalPosition = tileLayer.MapToLocal(pos);
        parent.AddChild(worldItem);
    }

    async public void SpawnItem(Vector2I pos, Item itemData, float angle, float force)
    {
        var worldItemNode = itemScene.Instantiate();
        if (worldItemNode is not ItemNode2D worldItem)
        {
            GD.PrintErr("cannot cast root to RigidBody2D");
            return;
        }
        worldItem.ItemData = itemData.Duplicate() as Item;
        parent.AddChild(worldItem);

        worldItem.GlobalPosition = tileLayer.MapToLocal(pos);
        Vector2 direction = Vector2.Down.Rotated(angle);

        await worldItem.GetTree().ToSignal(worldItem.GetTree(), "physics_frame");
        worldItem.LockRotation = false;
        worldItem.ApplyCentralImpulse(direction * force);
        worldItem.AngularVelocity = force/10;
        //these apply the position and impulse changes AFTER the item is spawned to avoid unreliable behaviors.
    }
}