using Manikineko.MMO.Core.AnimHelpers.Sandbox;
using Sandbox;
using System.Collections.Generic;
using System.Numerics;

public partial class MMOPlayer : Sandbox.Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;
	[ConVar.ClientData( "allow_inventorymenu" )]
	public static bool Allow_inventorymenu { get; set; } = true;
	public bool isGM = false;

	private DamageInfo lastDamage;

	[Net, Predicted]
	public bool ThirdPersonCamera { get; set; }

	/// <summary>
	/// The clothing container is what dresses the citizen
	/// </summary>
	public ClothingContainer Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public MMOPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client
	/// </summary>
	public MMOPlayer( IClient cl ) : this()
	{
		// Load clothing from client data
		//Clothing.LoadFromClient( cl );
	}

	public override void Respawn()
	{

        SetModel( "models/human_base/human.vmdl" );
		SetAnimGraph( "animgraphs/azuru.vanmgrph" );
		
		Controller = new WalkController();

		if ( DevController is NoclipController )
		{
			DevController = null;
		}

		this.ClearWaterLevel();
		EnableAllCollisions = true;

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Clothing.DressEntity( this );
		Tags.Add( "thirdperson" );
		if ( Tags.Has( "thirdperson" ) )
		{
			ThirdPersonCamera = true;
			EnableViewmodelRendering = false;


		}
		if ( Tags.Has( "safezone" ) )
		{
			Inventory.DeleteContents();
		}
		else
		{
			Inventory.Add( new Fists(), true );
		}
		
		
		base.Respawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		if ( lastDamage.HasTag( "vehicle" ) )
		{
			Particles.Create( "particles/impact.flesh.bloodpuff-big.vpcf", lastDamage.Position );
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}


		Controller = null;

		EnableAllCollisions = false;
		EnableDrawing = false;

		foreach ( var child in Children )
		{
			child.EnableDrawing = false;
		}

		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( Tags.Has( "safezone" ) )
		{
			return;
		}
		if ( info.Attacker.IsValid() )
		{
			if ( info.Attacker.Tags.Has( "safezone" ) )
			{
				return;
			}
			if ( info.Attacker.Tags.Has( $"{PhysGun.GrabbedTag}{Client.SteamId}" ) )
				return;
		}

		if ( info.Hitbox.HasTag( "head" ) )
		{
			info.Damage *= 10.0f;
		}

		lastDamage = info;

		base.TakeDamage( info );
	}

	public override PawnController GetActiveController()
	{
		if ( DevController != null ) return DevController;

		return base.GetActiveController();
	}

	public override void Simulate( IClient cl )
	{
		if ( Tags.Has( "thirdperson" ) )
		{

			


		}
		
		base.Simulate( cl );

		if ( LifeState != LifeState.Alive )
			return;

		var controller = GetActiveController();
		if ( controller != null )
		{
			EnableSolidCollisions = !controller.HasTag( "noclip" );

			SimulateAnimation( controller );
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );

		if ( Input.Pressed( InputButton.View ) )
		{
			ThirdPersonCamera = !ThirdPersonCamera;
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{

			if ( timeSinceJumpReleased < 0.3f )
			{
				if ( DevController is NoclipController )
				{
					DevController = null;
				}
				else
				{
					DevController = new NoclipController();
				}
			}

			timeSinceJumpReleased = 0;
		}

		if ( InputDirection.y != 0 || InputDirection.x != 0f )
		{
			timeSinceJumpReleased = 1;
		}
	}

	[ConCmd.Admin( "noclip" )]
	static void DoPlayerNoclip()
	{
		if ( ConsoleSystem.Caller.Pawn is MMOPlayer basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				basePlayer.DevController = null;
			}
			else
			{
				basePlayer.DevController = new NoclipController();
			}
		}
	}
	[ConCmd.Server( "togglesafezone" )]
	public static void ToggleSafezone( )
	{
		if ( ConsoleSystem.Caller.Pawn is MMOPlayer basePlayer )
		{
			if ( basePlayer.Tags.Has( "safezone" ) )
			{
				basePlayer.Inventory.Add( new Fists() ,true);
				basePlayer.Tags.Remove( "safezone" );
			}
			else
			{
				basePlayer.Inventory.DeleteContents();
				basePlayer.Tags.Add( "safezone" );
			}
		}
	}
	[ConCmd.Server( "rmtag" )]
	public static void RMTag( string tag )
	{
		if ( ConsoleSystem.Caller.Pawn is MMOPlayer basePlayer )
		{
			basePlayer.Tags.Remove( tag );
		}
	}
	[ConCmd.Server( "addtag" )]
	public static void AddTag(string tag )
	{
		if ( ConsoleSystem.Caller.Pawn is MMOPlayer basePlayer )
		{
			basePlayer.Tags.Add( tag );
		}
	}
	[ConCmd.Admin( "kill" )]
	static void DoPlayerSuicide()
	{
		if ( ConsoleSystem.Caller.Pawn is MMOPlayer basePlayer )
		{
			basePlayer.TakeDamage( new DamageInfo { Damage = basePlayer.Health * 99 } );
		}
	}


	Entity lastWeapon;

	void SimulateAnimation( PawnController controller )
	{
		if ( controller == null )
			return;

		// where should we be rotated to
		var turnSpeed = 0.02f;

		Rotation rotation;

		// If we're a bot, spin us around 180 degrees.
		if ( Client.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		AzuruAnimationHelper animHelper = new AzuruAnimationHelper( this );
		if ( Input.Pressed( InputButton.Jump) )
		{
			animHelper.TriggerJump();
		}
		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsNoclipping = controller.HasTag( "noclip" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
		if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

		if ( ActiveChild is BaseCarriable carry )
		{
			if ( carry is Carriable )
			{
				var _carry = carry as Carriable;
				_carry.SimulateAnimator( animHelper );
			}
			else
			if ( carry is Weapon )
			{
				var weapon = carry as Weapon;
				weapon.SimulateAnimator( animHelper );

			}
		}
		else
		{
			animHelper.HoldType = AzuruAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}

	[ConCmd.Server( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller.Pawn as Sandbox.Player;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( slot.ClassName != entName )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	public override void FrameSimulate( IClient cl )
	{
		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( ThirdPersonCamera )
		{
			Camera.FirstPersonViewer = null;

			Vector3 targetPos;
			var center = Position + Vector3.Up * 64;

			var pos = center;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 130.0f * Scale;
			targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 32) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();

			Camera.Position = tr.EndPosition;
		}
		else if ( LifeState != LifeState.Alive && Corpse.IsValid() )
		{
			Corpse.EnableDrawing = true;

			var pos = Corpse.GetBoneTransform( 0 ).Position + Vector3.Up * 10;
			var targetPos = pos + Camera.Rotation.Backward * 100;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();

			Camera.Position = tr.EndPosition;
			Camera.FirstPersonViewer = null;
		}
		else
		{
			Camera.Position = EyePosition;
			Camera.FirstPersonViewer = this;
			Camera.Main.SetViewModelCamera( 90f );
		}
	}
}
