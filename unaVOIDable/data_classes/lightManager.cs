using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[GlobalClass]
public partial class LightManager : RefCounted
{

    private Node parent;
    private TileMapLayer tileLayer;
    private HashSet<PointLight2D> lights = new();
    public HashSet<Vector2I> lightPositions = new();

    public LightManager(Node _parent, TileMapLayer _tileLayer)
    {
        parent = _parent;
        tileLayer = _tileLayer;
    }
    public void CreateLights()
    {
        foreach(Vector2I pos in lightPositions)
        {
            var light = new PointLight2D();
            light.Position = tileLayer.MapToLocal(pos);
            light.Enabled = false;
            light.Texture = GD.Load<Texture2D>("res://scenes/common/assets/radiallight.tres"); 
            light.TextureScale = 36.0f;          
            light.Energy = 1.5f;                
            light.Color = new Color(1, 1, 1);

            // Shadow settings
            light.ShadowEnabled = true;
            light.ShadowFilter = PointLight2D.ShadowFilterEnum.None; // Cheapest filter
            light.ShadowFilterSmooth = 0f;                           // No smoothing cost


            parent.AddChild(light);
            lights.Add(light);
        }
    }
	public void UpdateNearPlayer(Vector2I playerPos, float radius)
    {
        Vector2I playerWorld = tileLayer.LocalToMap(playerPos);
        foreach (var light in lights)
        {
            var lightPos = tileLayer.LocalToMap(light.Position);
            light.Enabled = lightPos.DistanceTo(playerWorld) <= radius;
        }
    }
}