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
		[Signal]
		public delegate void StaminaChangedEventHandler(float stamina);


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
		private float slideCooldown = 2f; 
		private float slideCooldownTimer = 0f;

		//--Stamina--//
		private float stamina = 100f;
		private float maxStamina = 100f;
		private float staminaDrain = 40f;    
		private float staminaRegen = 25f;    
		private float staminaRegenDelay = 0.5f; 
		private float staminaRegenTimer = 0f;


		//--Footsteps--//
		private float footstepTimer = 0f;
		private float baseStepInterval = 0.5f; 

		

		//--References/Inventory--//
		public EquipmentInventory playerInventory;
		public AudioStreamPlayer2D playerSounds;
		public WorldObjectManager worldObjectManager;
		public TileMapLayer tileLayer;
		private Camera2D playerCam;
		private PointLight2D flashLight;	
		public Node2D handPivot;
		public AnimatedSprite2D itemSprite;

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
			handPivot = GetNode<Node2D>("HandPivot");
			itemSprite = GetNode<AnimatedSprite2D>("HandPivot/itemsprite");
			playerInventory = new();
			playerInventory.DropItem += OnDropItem;
			flashLight.Position = this.Position;
			isSliding = false;
			playerInventory.activeSlot = new(EquipmentType.LargeItem, 0);
			EmitSignal(SignalName.SelectedSlot);
			EmitSignal(SignalName.StaminaChanged, stamina);
		}
		public override void _Process(double delta)
		{
			movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			isRunning = Input.IsActionPressed("run");
			isCrouched = Input.IsActionPressed("crouch");
			slideHeld = Input.IsActionPressed("slide");
			slidePressed = Input.IsActionJustPressed("slide");

			if (Input.IsActionJustPressed("pickup"))
			{
				var spaceState = GetWorld2D().DirectSpaceState;

				var circle = new CircleShape2D();
				circle.Radius = 128f; 

				var query = new PhysicsShapeQueryParameters2D
				{
					Shape = circle,
					Transform = new Transform2D(0, GlobalPosition),
					CollideWithAreas = true,
				};

				var results = spaceState.IntersectShape(query);

				ItemNode2D closestItem = null;
				float closestDist = float.MaxValue;

				foreach (var hit in results)
				{
					Node node = hit["collider"].As<Node>();

					if (node is ItemNode2D item)
					{
						float dist = GlobalPosition.DistanceTo(item.GlobalPosition);

						if (dist < closestDist)
						{
							closestDist = dist;
							closestItem = item;
						}
					}
				}

				if (closestItem != null)
				{
					if (playerInventory.Equip(closestItem.ItemData.equipmentType, closestItem.ItemData))
					{
						closestItem.QueueFree();
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
				var item = playerInventory.getActiveItem();

				if (item == null)
				{
					itemSprite.Visible = false; 
					return;
				}

				itemSprite.Visible = true;
				itemSprite.SpriteFrames = item.useAnimation;
			}
			if (Input.IsActionJustPressed("slot_small_1"))
			{
				playerInventory.activeSlot = new(EquipmentType.SmallItem, 0);
				EmitSignal(SignalName.SelectedSlot);
				var item = playerInventory.getActiveItem();

				if (item == null)
				{
					itemSprite.Visible = false; 
					return;
				}

				itemSprite.Visible = true;
				itemSprite.SpriteFrames = item.useAnimation;
			}
			if (Input.IsActionJustPressed("slot_small_2"))
			{
				playerInventory.activeSlot = new(EquipmentType.SmallItem, 1);
				EmitSignal(SignalName.SelectedSlot);
				var item = playerInventory.getActiveItem();

				if (item == null)
				{
					itemSprite.Visible = false; 
					return;
				}

				itemSprite.Visible = true;
				itemSprite.SpriteFrames = item.useAnimation;
			}
			if (Input.IsActionJustPressed("slot_consumable_1"))
			{
				playerInventory.activeSlot = new(EquipmentType.Consumable, 0);
				EmitSignal(SignalName.SelectedSlot);
				var item = playerInventory.getActiveItem();

				if (item == null)
				{
					itemSprite.Visible = false; 
					return;
				}

				itemSprite.Visible = true;
				itemSprite.SpriteFrames = item.useAnimation;
			}
			if (Input.IsActionJustPressed("slot_consumable_2"))	
			{
				playerInventory.activeSlot = new(EquipmentType.Consumable, 1);
				EmitSignal(SignalName.SelectedSlot);
				var item = playerInventory.getActiveItem();

				if (item == null)
				{
					itemSprite.Visible = false;
					return;
				}

				itemSprite.Visible = true;
				itemSprite.SpriteFrames = item.useAnimation;
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
			HandleStamina(delta);
			HandleFootsteps(delta);
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

			if (isRunning && stamina > 0f)
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
		private void HandleStamina(double delta)
		{
			if (isRunning && movementInput != Vector2.Zero && stamina > 0f)
			{
				stamina -= staminaDrain * (float)delta;
				
				staminaRegenTimer = staminaRegenDelay;

				if (stamina <= 0f)
				{
					stamina = 0f;
				}
				EmitSignal(SignalName.StaminaChanged, stamina);
			}
			else
			{
				if (staminaRegenTimer > 0f)
				{
					staminaRegenTimer -= (float)delta;
					EmitSignal(SignalName.StaminaChanged, stamina);
				}
				else
				{
					stamina += staminaRegen * (float)delta;
					EmitSignal(SignalName.StaminaChanged, stamina);
					if (stamina > maxStamina)
						stamina = maxStamina;
				}
			}
			
		}
		private void HandleFootsteps(double delta)
		{
			if (movementInput == Vector2.Zero && Velocity.Length() < 20f || isSliding)
			{
				playerSounds.Stop();
				return;
			}
			float stepInterval = baseStepInterval;
			float volume = 0f;

			if (isRunning && stamina > 0f)
			{
				stepInterval *= 0.6f;   
				volume = -7f;            
			}
			else if (isCrouched)
			{
				stepInterval *= 1.5f;   
				volume = -15f;          
			}
			else
			{
				volume = -10f;          
			}

			footstepTimer -= (float)delta;

			if (footstepTimer <= 0f)
			{
				PlayFootstep(volume);
				footstepTimer = stepInterval;
			}
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
		private void PlayFootstep(float volumeDb)
		{
			GD.Print("footstep");
			if (playerSounds.Stream == null)
				return;

			playerSounds.VolumeDb = volumeDb;
			playerSounds.PitchScale = (float)GD.RandRange(0.8f, 1.2f); 
			playerSounds.Play();
		}

	}

		
