using Godot;
using System;


public enum FireMode
{
    SemiAuto,
    Burst,
    FullAuto
}

[GlobalClass]
public partial class Firearm : Item
{
    [Export]
    public float damage = 10f;
    [Export]
    public float range = 9999f;
    [Export]
    public float zoomLevel = 1.0f;

    [Export]
    public float fireRate = 0.2f;

    [Export]
    public int ammo = 10;
    [Export]
    public int maxAmmo = 10;


    //--not basic types--//
    [Export]
    public FireMode fireMode = FireMode.SemiAuto;
    [Export]
    public AudioStream shotSound;

    [Export]
    public float baseSpread = 5f;      //in degrees
    [Export]
    public float adsSpreadMultiplier = 0.3f;

    [Export]
    public int burstCount = 3;
    private int _burstShotsLeft = 0;

    //--muzzle flash logic--//
    private double flashTime = 0f;
    private float flashDuration = 0.05f;
    [Export]
    float muzzleFlashEnergy = 1.5f;
    [Export]
    float muzzleFlashScale = 15.0f;

    //--shooting logic--//
    private double _lastShotTime = 0;
    private bool triggerHeld = false;
    private bool adsHeld = false;

    public override void Use(Player player)
    {
        triggerHeld = player.isHoldingUse;

        if (fireMode == FireMode.FullAuto)
            TryShoot(player);
        else if (fireMode == FireMode.SemiAuto)
            TryShoot(player);
        else if (fireMode == FireMode.Burst)
        {
            if (_burstShotsLeft <= 0)
                _burstShotsLeft = burstCount;

            TryShoot(player);
        }
    }
    public override void SecondaryUse(Player player)
    {
        adsHeld = true;
    }
    public override void Refill(Player player)
    {
        ammo = maxAmmo;
    }
    public override void Tick(Player player, double delta)
    {
        triggerHeld = player.isHoldingUse;
        adsHeld = player.isHoldingSecondaryUse;
        if (fireMode == FireMode.FullAuto && triggerHeld)
        {
            TryShoot(player);
        }
        if (fireMode == FireMode.Burst && triggerHeld && _burstShotsLeft > 0)
        {
            TryShoot(player);
        }       

        flashTime -= delta;
        GD.Print(flashTime);
        if(flashTime <= 0)
        {
            player.flashLight.Energy = 1.0f;
            player.flashLight.TextureScale = 10.0f;
        }
    }
    private void TryShoot(Player player)
    {
        if (ammo <= 0)
        {
            _burstShotsLeft = 0;
            return;
        }

        double now = Time.GetTicksMsec() / 1000.0;
        if (now - _lastShotTime < fireRate)
            return;

        if (fireMode == FireMode.Burst && _burstShotsLeft <= 0)
            return;

        _lastShotTime = now;
        ammo--;

        if (fireMode == FireMode.Burst)
            _burstShotsLeft--;

        Shoot(player);
    }
    private void Shoot(Player player)
    {
        flashTime = flashDuration;
        player.flashLight.Energy = muzzleFlashEnergy;
        player.flashLight.TextureScale = muzzleFlashScale;
        var spaceState = player.GetWorld2D().DirectSpaceState;

        Vector2 origin = player.GlobalPosition;
        Vector2 dir = player.GetAimDirection().Normalized();

        float spread = GetSpread();
        dir = ApplySpread(dir, spread);

        Vector2 end = origin + dir * range;

        var query = PhysicsRayQueryParameters2D.Create(origin, end);
        query.CollideWithBodies = true;
        query.CollideWithAreas = true;
        query.Exclude = new Godot.Collections.Array<Rid> { player.GetRid() };

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            Node collider = result["collider"].As<Node>();
            Vector2 hit = result["position"].As<Vector2>();

            if (collider.HasMethod("TakeDamage"))
                collider.Call("TakeDamage", damage);

            DrawDebugLine(origin, hit, player);
        }
        else
        {
            DrawDebugLine(origin, end, player);
        }
        PlayShot(player);
    }
    public void PlayShot(Player player)
    {
        player.PlayShot(-5.0f, shotSound);  
    }
    private float GetSpread()
    {
        float spread = baseSpread;

        if (adsHeld)
            spread *= adsSpreadMultiplier;

        return spread;
    }
    private Vector2 ApplySpread(Vector2 dir, float spreadDegrees)
    {
        float angle = (float)Mathf.DegToRad(GD.RandRange(-spreadDegrees, spreadDegrees));
        return dir.Rotated(angle);
        //this rotates the shot by a random spread
    }
    private void DrawDebugLine(Vector2 from, Vector2 to, Player player)
    {
        var line = new Line2D();
        line.Width = 2;
        line.DefaultColor = Colors.White;

        line.AddPoint(from);
        line.AddPoint(to);

        player.GetTree().CurrentScene.AddChild(line);

        var timer = new Timer();
        timer.WaitTime = 0.05f;
        timer.OneShot = true;
        timer.Timeout += () => line.QueueFree();

        line.AddChild(timer);
        timer.Start();
        //this just draws the line between the player and their target
    }
}