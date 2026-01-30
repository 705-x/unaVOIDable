using Godot;
using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

public partial class Player : CharacterBody2D
{


	[Signal]
	delegate void OpenedInventoryEventHandler();

	[Signal]
	delegate void ClosedInventoryEventHandler();

	[Signal]
	delegate void HealthChangedEventHandler(int newHealth);


	public enum slideState
	{
		Sliding,
		OnCooldown
	}

	//--Player Input--//
	public Vector2 movementInput;
	public bool isRunning;
	public bool isCrouched;
	public bool slidePressed;
	public bool slideHeld;

	//--Stats--//
	private float hp = 100;

	//--Movement stats / Physics--//
	public float Friction = 2500.0f;
	public float Acceleration = 2500.0f;
	public float maxSpeed = 400.0f;
	public float PushStrength = 500.0f;
	public bool isSliding = false;

	//--References/Inventory--//
	public EquipmentInventory playerInventory;
	private Camera2D playerCam;

	public override void _Ready()
	{
    	playerCam = GetNode<Camera2D>("Camera");
	}

	public override void _Process(double delta)
	{
		movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		isRunning = Input.IsActionPressed("run");
		isCrouched = Input.IsActionPressed("crouch");
		slideHeld = Input.IsActionPressed("slide");

		Vector2 mousePos = GetGlobalMousePosition();
		Rotation = (GlobalPosition - mousePos).Angle();
		Rotation -= Mathf.Pi/2;
		playerCam.GlobalPosition = (GlobalPosition * 0.8f+ mousePos * 0.2f);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("slide"))
		slidePressed = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		
		ResolveMovement(delta);
		slidePressed = false;
		MoveAndSlide();
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
    		var col = GetSlideCollision(i);
    		var body = col.GetCollider() as RigidBody2D;
    		if (body != null){

   				Vector2 pushDir = -col.GetNormal();
    			Vector2 localPoint = body.ToLocal(col.GetPosition());

    			body.ApplyImpulse(pushDir * PushStrength, localPoint);
			}
		}
	}

	private void ResolveMovement(double delta)
	{
		float calcMaxSpeed = maxSpeed;
		float calcAccel = Acceleration;
		float calcFriction = Friction;
		if (isRunning)
		{
			calcAccel *=2f;
			calcMaxSpeed *= 2f;
		}
		if (isCrouched && !isSliding)
		{
			calcMaxSpeed *= 0.5f;
		}else if (isSliding)
		{
			calcMaxSpeed *= 2.5f;
			calcFriction *= 0.3f;
			if( Velocity.Length() < 400 || !slideHeld)
			{
				isSliding = false;
			}
			movementInput = Vector2.Zero;
		}
		
		float frictionDelta = (float)delta * calcFriction;
		if(movementInput != Vector2.Zero){
			Vector2 goal = movementInput.Normalized() * calcMaxSpeed;
			float accelTime = calcAccel * (float)delta;
   			Velocity = Velocity.MoveToward(goal, accelTime);
		}else
		{
			float velLen = Velocity.Length();
			if (velLen > 0f)
			{
 				if (velLen > frictionDelta){
				Velocity = Velocity.MoveToward(Vector2.Zero, calcFriction * (float)delta);
				}
				else{
				Velocity = Vector2.Zero;
				}
			}
		}

		if (slidePressed)
		{
			Velocity *= 1.5f;
			isSliding = true;
		}

	}
	public void Damaged(int damage)
	{
		hp -= damage;
		DrawDamage();
		EmitSignal(SignalName.HealthChanged, hp);
	}
	public void Damaged(int damage, Vector2 force)
	{
		hp -= damage;
		DrawDamage();
		EmitSignal(SignalName.HealthChanged, hp);
		Velocity += force;
	}
	public void DrawDamage()
	{
		Sprite2D bloodSprite = (Sprite2D)GetNode("../Hud/Control/Sprite2D");	
		GD.Print(bloodSprite.SelfModulate);
		var material = (ShaderMaterial)GetNode<ColorRect>("../Hud/ColorRect").Material;

		material.SetShaderParameter("saturation", hp/100);
		GD.Print(material.GetShaderParameter("saturation"));
		bloodSprite.SelfModulate = bloodSprite.SelfModulate with {A = (100/hp)/100};
	}

	public void DropItem(Item item, Vector2 position, float angle)
	{
		var scene = GD.Load<PackedScene>("res://scenes/ItemNode2d.tscn");
		var worldItem = scene.Instantiate<ItemNode2D>();

		worldItem.ItemData = item.Duplicate() as Item;
		worldItem.GlobalPosition = position;

		GetTree().CurrentScene.AddChild(worldItem);

		worldItem.ApplyDropImpulse(angle, 300f);
	}

}

	
