using Godot;


public partial class Player : CharacterBody2D
{


	[Signal]
	public delegate void OpenedInventoryEventHandler();

	[Signal]
	public delegate void ClosedInventoryEventHandler();

	[Signal]
	public delegate void SelectedSlotEventHandler();

	[Signal]
	public delegate void HealthChangedEventHandler(int newHealth);


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
	AnimatedSprite2D playerSprite = new();

	//--Movement stats / Physics--//
	public float Friction = 2500.0f;
	public float Acceleration = 2500.0f;
	public float maxSpeed = 400.0f;
	public float PushStrength = 500.0f;
	public bool isSliding = false;

	//--Cooldowns and cooldown stats--//

	private float slideCooldown = 2f; 
	private float slideCooldownTimer = 0f;

	//--References/Inventory--//
	public EquipmentInventory playerInventory;
	public AudioStreamPlayer2D playerSounds;
	public WorldObjectManager worldObjectManager;
	public TileMapLayer tileLayer;
	private Camera2D playerCam;
	private PointLight2D flashLight;

	/*the player needs to know the WorldObjectManager and tileMapLayer in order to kinda connect those.
	WorldObjectManager needs to have the position and angle at which to drop an item and having
	the Inventory know those about the player just ruins the division of tasks between them, so
	this goes here and Player will be like a relay between all this shit.
	*/
	public override void _Ready()
	{
    	playerCam = GetNode<Camera2D>("Camera");
		flashLight = GetNode<PointLight2D>("PointLight2D");
		playerSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		playerSounds = GetNode<AudioStreamPlayer2D>("PlayerSounds");
		playerInventory = new();
		playerInventory.DropItem += OnDropItem;
		flashLight.Position = this.Position;
	}
	public override void _Process(double delta)
	{
		movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		isRunning = Input.IsActionPressed("run");
		isCrouched = Input.IsActionPressed("crouch");
		slideHeld = Input.IsActionPressed("slide");
		slidePressed = Input.IsActionJustPressed("slide");
		bool camToggle = Input.IsKeyPressed(Godot.Key.Z);
		if (Input.IsActionJustPressed("pickup"))
		{
    		var spaceState = GetWorld2D().DirectSpaceState;
    		var query = new PhysicsPointQueryParameters2D
    		{
        		Position = GetGlobalMousePosition(),
        		CollideWithAreas = true,
    		};
    		var results = spaceState.IntersectPoint(query);

    		foreach (var hit in results)
    		{
				Node node = hit["collider"].As<Node>();

        		if (node is ItemNode2D item)
        		{

					GD.Print("attemptedpickup");
            		if (playerInventory.Equip(item.ItemData.equipmentType, item.ItemData))
            		{
                		item.QueueFree();
            		}
            	break;
        		}
    		}
		}	
		if (Input.IsActionJustPressed("drop"))
		{
			playerInventory.Drop(playerInventory.activeSlot.Key, playerInventory.activeSlot.Value);
		}

		if (Input.IsActionJustPressed("slot_large"))
		{
			playerInventory.activeSlot = new(EquipmentType.LargeItem, 0);
			EmitSignal(SignalName.SelectedSlot);
		}

		if (Input.IsActionJustPressed("slot_small_1"))
		{
			playerInventory.activeSlot = new(EquipmentType.SmallItem, 0);
			EmitSignal(SignalName.SelectedSlot);
		}
		if (Input.IsActionJustPressed("slot_small_2"))
		{
			playerInventory.activeSlot = new(EquipmentType.SmallItem, 1);
			EmitSignal(SignalName.SelectedSlot);
		}

		if (Input.IsActionJustPressed("slot_consumable_1"))
		{
			playerInventory.activeSlot = new(EquipmentType.Consumable, 0);
			EmitSignal(SignalName.SelectedSlot);
		}

		if (Input.IsActionJustPressed("slot_consumable_2"))
		{
			playerInventory.activeSlot = new(EquipmentType.Consumable, 1);
			EmitSignal(SignalName.SelectedSlot);
		}

		//this is an ugly and downright tedious way to do this. I'll figure out how to do it better soon

		

		/*if (camToggle)
		{
			playerCam.Zoom = new((float)0.01,(float)0.01);
		}
		else
		{
			playerCam.Zoom = new((float)0.85,(float)0.85);
		}*/

		Vector2 mousePos = GetGlobalMousePosition();
		GlobalRotation = (mousePos - GlobalPosition).Angle();
		GlobalRotation -= Mathf.Pi/2;	
		playerCam.GlobalPosition = (GlobalPosition * 0.8f+ mousePos * 0.2f);


		
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
		
		//these three are just copied base stats for the sake of multiplying them without working on the base values. Wonky but i couldn't
		//think of a better way.

		if (slideCooldownTimer > 0f)
		{
    		slideCooldownTimer -= (float)delta; //decreases the cooldown with time
		}

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
				slideCooldownTimer = slideCooldown;
			}
			movementInput = Vector2.Zero;
		}

		//these Ifs modify the base values based on the player state, basically a really stupid way of changing max speed dependant on what the player is doing
		
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

		//this is a mess. BUT IT WORKS

		if (slidePressed && slideCooldownTimer <= 0f)
		{
			Velocity *= 1.5f;
			isSliding = true;
		}
		
		//pretty self explanatory

	}
	public void Damaged(int damage)
	{
		hp -= damage;
		EmitSignal(SignalName.HealthChanged, hp);
	}
	public void Damaged(int damage, Vector2 force)
	{
		hp -= damage;
		Velocity += force;
		EmitSignal(SignalName.HealthChanged, hp);

		//thought it'd be funny if the player could be flung around by strong attacks.
	}
	public void OnDropItem(Item item)
	{
		Vector2 size = playerSprite.SpriteFrames.GetFrameTexture(playerSprite.Animation, playerSprite.Frame).GetSize();
		Vector2 offset = new Vector2(0, size.Y*2);
		offset = offset.Rotated(GlobalRotation);

		Vector2 pos = GlobalPosition + offset;	
    	float force = 1000f;

    	worldObjectManager.SpawnItem(
        	tileLayer.LocalToMap(pos),
        	item,
        	GlobalRotation,
        	force
    	);
	}

}

	
