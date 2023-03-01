using Sandbox;
using System.Collections.Generic;
[Library( "ammo", Title = "Ammo(right click to add)" )]
public partial class AmmoItem : Carriable, IUse
{


	public virtual float ReloadTime => 3.0f;

	public PickupTrigger PickupTrigger { get; protected set; }
	public Weapon weapon { get; protected set; }
	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false
		};

		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}



	public override void Simulate( IClient owner )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if ( !IsReloading )
		{
			base.Simulate( owner );
		}

		if ( IsReloading && TimeSinceReload > ReloadTime )
		{
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		// TODO - player third person model reload
	}

	public override void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	public bool OnUse( Entity user )
	{
		base.OnUse( user );
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;
		if ( user is Player )
		{

			user.StartTouch( this );
			var pawnuser = user as Player;
			if ( pawnuser != null )
			{

				if ( pawnuser.Inventory.Contains( weapon ) )
				{
					pawnuser.ActiveChild.Delete();
					weapon.AddAmmo( weapon.MaxAmmo );
					weapon.AddReloadAmmo( 1 );
					return true;
				}
				else
				{
					return false;
				}
			}
			return false;
		}
		else
		{
			return false;
		}

	}

	public virtual bool IsUsable( Entity user )
	{
		var player = user as Sandbox.Player;
		if ( Owner != null ) return false;

		if ( player.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}

	public void Remove()
	{
		Delete();
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}




}
