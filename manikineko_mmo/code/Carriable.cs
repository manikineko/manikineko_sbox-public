using Manikineko.MMO.Core.AnimHelpers.Sandbox;
using Sandbox;

public partial class Carriable : BaseCarriable, IUse
{
	public void SimulateAnimator( AzuruAnimationHelper anim )
	{
		anim.HoldType = AzuruAnimationHelper.HoldTypes.Pistol;
		anim.Handedness = AzuruAnimationHelper.Hand.Both;
		anim.AimBodyWeight = 1.0f;
	}
	public override void CreateViewModel()
	{
		if(Owner is Player )
		{
			MMOPlayer player = Owner as MMOPlayer;
			if( player != null && player.ThirdPersonCamera )
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
		return false;
	}

	public virtual bool IsUsable( Entity user )
	{
		return Owner == null;
	}
}
