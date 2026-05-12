using Godot;

//! Main player controller class. Handles player movement, stamina, sliding, inventory interaction, item usage, audio playback, aiming, health management, and world interaction.
public partial class Player : CharacterBody2D
{
	//-- Signals --//

	[Signal]
	public delegate void OpenedInventoryEventHandler(); //!< Emitted when the inventory is opened.

	[Signal]
	public delegate void ClosedInventoryEventHandler(); //!< Emitted when the inventory is closed.

	[Signal]
	public delegate void SelectedSlotEventHandler(); //!< Emitted when the active inventory slot changes.

	[Signal]
	public delegate void HealthChangedEventHandler(int newHealth); //!< Emitted whenever the player's health changes.

	[Signal]
	public delegate void StaminaChangedEventHandler(float stamina); //!< Emitted whenever the player's stamina changes.


	/// <summary>
	/// Represents the current slide state of the player.
	/// </summary>
	public enum slideState
	{
		Sliding,   //!< Player is currently sliding.
		OnCooldown //!< Slide is unavailable due to cooldown.
	}


	//-- Player Input --//

	public Vector2 movementInput; //!< Current movement direction input.

	public bool isRunning; //!< Whether the player is sprinting.

	public bool isCrouched; //!< Whether the player is crouched.

	public bool slidePressed; //!< Whether the slide button was pressed this frame.

	public bool slideHeld; //!< Whether the slide button is currently held.

	public bool isHoldingUse; //!< Whether the primary use button is held.

	public bool isHoldingSecondaryUse; //!< Whether the secondary use button is held.


	//-- Stats --//

	private float hp = 100; //!< Current player health.

	AnimatedSprite2D playerSprite = new(); //!< Reference to the player sprite.


	//-- Movement stats / Physics --//

	public float Friction = 2500.0f; //!< Base friction applied to movement.

	public float Acceleration = 2500.0f; //!< Base acceleration value.

	public float maxSpeed = 400.0f; //!< Base movement speed.

	public float PushStrength = 500.0f; //!< Force applied to pushed rigid bodies.

	public bool isSliding = false; //!< Whether the player is currently sliding.

	private float slideCooldown = 2f; //!< Delay before sliding can be used again.

	private float slideCooldownTimer = 0f; //!< Current slide cooldown timer.


	//-- Stamina --//

	private float stamina = 100f; //!< Current stamina value.

	private float maxStamina = 100f; //!< Maximum stamina.

	private float staminaDrain = 40f; //!< Stamina drained per second while sprinting.

	private float staminaRegen = 25f; //!< Stamina regenerated per second.

	private float staminaRegenDelay = 0.5f; //!< Delay before stamina regeneration begins.

	private float staminaRegenTimer = 0f; //!< Current stamina regeneration timer.


	//-- Footsteps --//

	private float footstepTimer = 0f; //!< Timer controlling footstep playback.

	private float baseStepInterval = 0.5f; //!< Base interval between footsteps.


	//-- References / Inventory --//

	public EquipmentInventory playerInventory; //!< Player inventory instance.

	public AudioStreamPlayer2D playerSounds; //!< Audio player used for footsteps.

	public AudioStreamPlayer2D shotSounds; //!< Audio player used for weapon sounds.

	public AudioStreamPlayer2D animationSounds; //!< Audio player used for animation sounds.

	public WorldObjectManager worldObjectManager; //!< Handles spawning world items.

	public TileMapLayer tileLayer; //!< Tilemap layer reference used for coordinate conversion.

	private Camera2D playerCam; //!< Main player camera.

	public PointLight2D flashLight; //!< Player flashlight / muzzle flash light.

	public Node2D handPivot; //!< Pivot point used for held item rotation.

	public AnimatedSprite2D itemSprite; //!< Sprite displaying the currently equipped item.


		/*the player needs to know the WorldObjectManager and tileMapLayer in order to kinda connect those.
		WorldObjectManager needs to have the position and angle at which to drop an item and having
		the Inventory know those about the player just ruins the division of tasks between them, so
		this goes here and Player will be like a relay between all this shit.
		*/
		public override void _Ready()
		{
			//! Initializes references, inventory setup, signals, flashlight setup, and default active inventory slot.
			playerCam = GetNode<Camera2D>("Camera");
			flashLight = GetNode<PointLight2D>("PointLight2D");
			playerSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			playerSounds = GetNode<AudioStreamPlayer2D>("Footsteps");
			shotSounds = GetNode<AudioStreamPlayer2D>("ShotSounds");
			animationSounds = GetNode<AudioStreamPlayer2D>("AnimationSounds");
			handPivot = GetNode<Node2D>("HandPivot");
			itemSprite = GetNode<AnimatedSprite2D>("HandPivot/itemsprite");
			itemSprite.Scale = new(2.0f,2.0f);
			playerInventory = new();
			playerInventory.DropItem += OnDropItem;
			flashLight.Position = this.Position - new Vector2(25,0);
			isSliding = false;
			playerInventory.activeSlot = new(EquipmentType.LargeItem, 0);
			EmitSignal(SignalName.SelectedSlot);
			EmitSignal(SignalName.StaminaChanged, stamina);
		}
		//! Handles player input, item interaction, inventory switching, camera movement, aiming, and item usage logic.
		public override void _Process(double delta)
		{
			//--inputs--//
			movementInput = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			isRunning = Input.IsActionPressed("run");
			isCrouched = Input.IsActionPressed("crouch");
			slideHeld = Input.IsActionPressed("slide");
			slidePressed = Input.IsActionJustPressed("slide");
			var activeItem = playerInventory.getActiveItem();
			
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

			if (activeItem == null)
			{
				itemSprite.Visible = false;
			}
			else
			{
				itemSprite.Visible = true;
				itemSprite.SpriteFrames = activeItem.useAnimation;
			}

			if (Input.IsKeyPressed(Key.Key0))
			{
				GetTree().ChangeSceneToFile("res://scenes/endingScenes/escapedScene.tscn");
			}
			if (Input.IsKeyPressed(Key.Key9))
			{
				GetTree().ChangeSceneToFile("res://scenes/endingScenes/deadScene.tscn");
			}
			//--Mouse position and rotation--//
			Vector2 mousePos = GetViewport().GetMousePosition() - GetViewport().GetVisibleRect().Size * 0.5f;
			//this is the mousePosition minus Viewport/2, so the mouse's 0,0 coords are in the center.
			GlobalRotation = (GetGlobalMousePosition() - GlobalPosition).Angle();
	
			playerCam.GlobalPosition = (GlobalPosition + mousePos * 0.2f);


			/*if (camToggle)
			{
				playerCam.Zoom = new((float)0.01,(float)0.01);
			}
			else
			{
				playerCam.Zoom = new((float)0.85,(float)0.85);
			}*/

			//--Item use logic--//
			if (Input.IsActionJustPressed("primary_action") && activeItem != null)
			{
				activeItem.Use(this);
				this.isHoldingUse = true;
			}else if(Input.IsActionJustReleased("primary_action") && activeItem != null)
			{
				this.isHoldingUse = false;
			}
			if (Input.IsActionJustPressed("secondary_action") && activeItem != null)
			{
				activeItem.SecondaryUse(this);
				this.isHoldingSecondaryUse = true;
			}else if(Input.IsActionJustReleased("secondary_action") && activeItem != null)
			{
				this.isHoldingSecondaryUse = false;
			}
			if (Input.IsActionJustPressed("refill") && activeItem != null)
			{
				activeItem.Refill(this);
			}


			if(activeItem is Firearm firearm && this.isHoldingSecondaryUse)
			{
				playerCam.GlobalPosition = (GlobalPosition + (mousePos * (firearm.zoomLevel-0.8f)));
			}
			if(activeItem != null)
			{
				activeItem.Tick(this, delta);
			}	
			//this is an ugly and downright tedious way to do this. I'll figure out how to do it better soon

			//i added some more shi, absolute if hell XDDD but idgaf i don't have much time
			
				
		}
		//! Handles movement physics, stamina updates, footsteps, collision pushing, and slide reset logic.
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
		//! Calculates and applies movement velocity based on movement state, sprinting, crouching, sliding, and friction.
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

			if(isHoldingUse || isHoldingSecondaryUse)
			{
				calcMaxSpeed *= 0.5f;
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
		//! Handles stamina draining while sprinting and regeneration while resting.
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
		//! Plays footstep sounds depending on player movement state.
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
		//! Applies damage to the player and triggers death if health reaches zero.
		public void Damaged(int damage)
		{
			
			hp -= damage;
			EmitSignal(SignalName.HealthChanged, hp);
			if(hp <= 0)
			{
				GetTree().ChangeSceneToFile("res://scenes/endingScenes/deadScene.tscn");
			}
		}
		//! Applies damage and knockback force to the player.
		public void Damaged(int damage, Vector2 force)
		{
			
			hp -= damage;
			Velocity += force;
			EmitSignal(SignalName.HealthChanged, hp);

			if(hp <= 0)
			{
				GetTree().ChangeSceneToFile("res://scenes/endingScenes/deadScene.tscn");
			}
			//thought it'd be funny if the player could be flung around by strong attacks.
		}
		//! Restores player health up to the maximum value.
		public void Heal(int amount)
		{
			
			hp += amount;

			if (hp > 100)
				hp = 100;

			EmitSignal(SignalName.HealthChanged, hp);
		}
		//! Restores player stamina up to the maximum value.
		public void AddStamina(float amount)
		{
			
			stamina += amount;
			if (stamina > maxStamina)
				stamina = maxStamina;

			EmitSignal(SignalName.StaminaChanged, stamina);
		}
		//these three methods are for items to be able to interact with hp and stamina of the player

		//! Spawns a dropped item into the world in front of the player.
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
				GlobalRotation -= Mathf.Pi/2,
				force
			);
		}
		//! Plays a randomized footstep sound at the specified volume.
		private void PlayFootstep(float volumeDb)
		{
			
			GD.Print("footstep");
			//debug
			if (playerSounds.Stream == null)
				return;

			playerSounds.VolumeDb = volumeDb;
			playerSounds.PitchScale = (float)GD.RandRange(0.8f, 1.2f); 
			playerSounds.Play();
		}
		//! Plays a weapon firing sound.
		public void PlayShot(float volumeDb, AudioStream stream)
		{
			
			shotSounds.Stream = stream;
			shotSounds.VolumeDb = volumeDb;
			shotSounds.PitchScale = (float)GD.RandRange(0.8f, 1.2f); 
			shotSounds.Play();
		}
		//! Plays a generic animation-related sound effect.
		public void PlayAnimationSound(float volumeDb, AudioStream stream)
		{
			
			shotSounds.Stream = stream;
			shotSounds.VolumeDb = volumeDb;
			shotSounds.PitchScale = (float)GD.RandRange(0.95f, 1.1f); 
			shotSounds.Play();
		}
		//!this is just a wrapper which connects this so the Item can cast this from within itself. for entire logic, see playerInventory.RemoveActiveItem
		public void RemoveActiveItem()
		{
			
			playerInventory.RemoveActiveItem();
			//this is just a wrapper which connects this so the Item can cast this from within itself.
			//for entire logic, see playerInventory.RemoveActiveItem
		}
		
		public async void PlayItemAnimation(string animationName = "use")
		{
			
			if (itemSprite == null || itemSprite.SpriteFrames == null)
				return;
			//if item doesn't have anim it just skips this function
			if (!itemSprite.SpriteFrames.HasAnimation(animationName))
			{
				GD.Print($"Animation '{animationName}' not found!");
				return;
			}
			//incase i forget, just so i know which item doesn't have an animation
			itemSprite.Play(animationName);

			await ToSignal(itemSprite, AnimatedSprite2D.SignalName.AnimationFinished);

			itemSprite.Stop();

			//plays, then waits for the end of the animation, then stops. Self-explanatory
		}
		//!Plays the item's designated animation.
		public Vector2 GetAimDirection()
		{
			//! Returns a normalized direction vector from the player toward the mouse cursor.
			return (GetGlobalMousePosition() - GlobalPosition).Normalized();
		}
	}

		
