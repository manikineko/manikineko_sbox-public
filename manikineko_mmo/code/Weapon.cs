using Manikineko.MMO.Core.AnimHelpers.Sandbox;
using Sandbox;
using System.Collections.Generic;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual WeaponConfig Config { get; }
	public virtual float ReloadTime => 3.0f;
	public virtual float Ammo { get; set; } = 0;
	public virtual float MaxAmmo { get; set; } = 0;
	public virtual float Reloads_MaxAmmo { get; set; } = 0;
	public virtual bool reloadinfinite { get; set; } = false;
	public virtual float Reloads { get; set; } = 0;
	public virtual bool IsMelee { get; } = false;
	public AzuruAnimationHelper animationHelper;
	public virtual AzuruAnimationHelper.HoldTypes HoldType { get; set; } = AzuruAnimationHelper.HoldTypes.None;
	public PickupTrigger PickupTrigger { get;set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

	public void SimulateAnimator( AzuruAnimationHelper anim )
	{
		animationHelper = anim;
		anim.HoldType = HoldType;
		anim.Handedness = AzuruAnimationHelper.Hand.Both;
		anim.AimBodyWeight = 1.0f;
	}
	public void SetAmmo( float amount )
	{
		Ammo = amount;
	}
	public void SetMaxAmmo( float amount )
	{
		MaxAmmo = amount;
	}
	public void SetReloadInfinite( bool amount )
	{
		reloadinfinite = true;
	}
	public float GetAmmo()
	{
		return Ammo;
	}
	public float GetReloadAmmo()
	{
		if ( reloadinfinite )
		{
			return float.PositiveInfinity;
		}
		else
		{
			return Reloads;
		}
	}
	public void AddAmmo( float ammount )
	{
		Ammo += ammount;
	}
	public void AddReloadAmmo( float ammount )
	{
		Reloads += ammount;
	}
	public  void SetReloadAmmo( float ammount )
	{
		Reloads = ammount;
	}
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
	public override void AttackPrimary()
	{
		animationHelper.IsAttacking = true;
	}
	public override async void AttackSecondary()
	{
		animationHelper.IsAttacking = true;
		Reload();

	}
	public override void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;
		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );

		StartReloadEffects();
		if ( Reloads > 0 )
		{

			while ( IsReloading && TimeSinceSecondaryAttack > 0 )
			{

			}
			if ( !reloadinfinite )
			{
				Reloads--;
			}
			

			Ammo = MaxAmmo;
		}
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
		if ( Owner is Player )
		{
			MMOPlayer player = Owner as MMOPlayer;
			if ( player != null && player.ThirdPersonCamera )
			{
				return;
			}
		}
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
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		user.StartTouch( this );

		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		var player = user as Player;
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

	public override IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	public IEnumerable<TraceResult> TraceMelee( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc", "glass" )
				.Ignore( this );

		var tr = trace.Run();

		if ( tr.Hit )
		{
			yield return tr;
		}
		else
		{
			trace = trace.Size( radius );

			tr = trace.Run();

			if ( tr.Hit )
			{
				yield return tr;
			}
		}
	}
	
	/// <summary>
	/// Shoot a single bullet
	/// </summary>
	public virtual void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		var forward = dir;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
		forward = forward.Normal;

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + forward * 5000, bulletSize ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so any exploding effects don't get culled etc
			//
			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100 * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	/// <summary>
	/// Shoot a single bullet from owners view point
	/// </summary>
	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize )
	{
		if(Ammo < 0 )
		{
			return;
		}
		Game.SetRandomSeed( Time.Tick );
		Ammo--;
		var ray = Owner.AimRay;
		ShootBullet( ray.Position, ray.Forward, spread, force, damage, bulletSize );
	}

	/// <summary>
	/// Shoot a multiple bullets from owners view point
	/// </summary>
	public virtual void ShootBullets( int numBullets, float spread, float force, float damage, float bulletSize )
	{
		if ( Ammo < 0 )
		{
			return;
		}
		var ray = Owner.AimRay;
		Ammo--;
		for ( int i = 0; i < numBullets; i++ )
		{
			ShootBullet( ray.Position, ray.Forward, spread, force / numBullets, damage, bulletSize );
		}
	}
}
