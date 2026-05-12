using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

 //!Class used for spawning and loading all props and items.

[GlobalClass]
public partial class WorldObjectManager : RefCounted
{
   
    private Node parent; //!<Parent node to which lights will be added.
    private TileMapLayer tileLayer; //!<tileLayer TileLayer of the parent node. Needed for map to global conversion.

    public HashSet<Vector2I> propMarkers = new(); //!<Hashset of positions in which the prop objects should be placed. Taken from Marker2D positions defined in Room scenes. Expressed in cell coordinates, gets converted into global position.
    public HashSet<Vector2I> itemMarkers = new(); //!<Hashset of positions in which the item objects should be placed. Taken from Marker2D positions defined in Room scenes. Expressed in cell coordinates, gets converted into global position.

    Array<Prop> propsArray = new(); //!<Contains prop resources loaded by loadObjects.
    Array<Item> itemsArray = new(); //!<Contains item resources loaded by loadObjects.

    Random rand = new();
    PackedScene propScene = GD.Load<PackedScene>("res://scenes/prop/prop_node_2d.tscn"); //!<Blueprint for data inside of Prop resource files. See Prop and PropNode2D.
    PackedScene itemScene = GD.Load<PackedScene>("res://scenes/groundItem/item_node_2d.tscn"); //!<Blueprint for data inside of Item resource files. See Item and ItemNode2D.

    public WorldObjectManager(Node _parent, TileMapLayer _tileLayer)
    {
        parent = _parent;
        tileLayer = _tileLayer;
        
    }
    
    public void spawnWorldObjects()
    {
        //!Iterates over propMarker positions, picking a random prop from propsArray. Afterwards, rolls a chance to spawn it according to that Item/Prop's spawnChance value.
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
        //! Loads objects from the directory tree.
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
        //! Instantiates the propScene using passed propData, then spawns it at given position.
        //!@param pos Position to spawn the prop. Passed as global coordinates.
        //!@param propData See Prop.
        var worldPropNode = propScene.Instantiate();
        var worldProp = worldPropNode as PropNode2D;
        worldProp.propData = propData.Duplicate() as Prop;
        worldProp.GlobalPosition = tileLayer.MapToLocal(pos);
        parent.AddChild(worldProp);
    }
    public void SpawnItem(Vector2I pos, Item itemData)
    {
        //! Instantiates the itemScene using passed itemData, then spawns it at given position.
        //!@param pos Position to spawn the item. Passed as global coordinates.
        //!@param itemData See Item.
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
        //! Overloaded function that adds dropping items with directed force.
        //!@param pos Position to spawn the item. Passed as global coordinates.
        //!@param itemData See Item.
        //!@param angle Throw direction.
        //!@param force Throw force.
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