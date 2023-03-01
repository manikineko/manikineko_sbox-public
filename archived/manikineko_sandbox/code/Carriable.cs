using Sandbox;

public partial class Carriable : BaseCarriable, IUse
{
	public override void CreateViewModel()
	{
		if(Owner is Player )
		{
			SandboxPlayer player = Owner as SandboxPlayer;
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
