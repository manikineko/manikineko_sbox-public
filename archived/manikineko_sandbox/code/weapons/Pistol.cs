using Sandbox;

[Spawnable]
[Library( "weapon_pistol", Title = "Pistol" )]
partial class Pistol : Weapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override float PrimaryRate => 15.0f;
	public override float SecondaryRate => 1.0f;
	public TimeSince TimeSinceDischarge { get; set; }
	public override float Ammo { get; set; } = 10;
	public override float MaxAmmo { get; set; } = 10;
	public override float Reloads_MaxAmmo { get; set; } = 10;
	public override bool reloadinfinite { get; set; } = true ;
	public override float Reloads { get; set; } = 10;
	
	public override void Spawn()
	{
		base.Spawn();
		
		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.PrimaryAttack );
	}
	public override bool CanSecondaryAttack()
	{
		return base.CanSecondaryAttack() && Input.Pressed( InputButton.SecondaryAttack);
	}
	public override void AttackPrimary()
	{
		base.AttackPrimary();
		if ( Ammo > 0 )
		{
			
			TimeSincePrimaryAttack = 0;
			TimeSinceSecondaryAttack = 0;

			(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

			
			ShootEffects();
			PlaySound( "rust_pistol.shoot" );
			ShootBullet( 0.05f, 1.5f, 9.0f, 3.0f );
		}

	}

	
	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( pos, rot.Forward, 0.05f, 1.5f, 9.0f, 3.0f );

		ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f )
		{
			Discharge();
		}
	}

}
