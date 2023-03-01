using Sandbox.Tools;

namespace Sandbox
{
	[Library( "tool_remover", Title = "Forced Remover", Description = "Remove any entities", Group = "construction" )]
	public partial class ForcedRemoverTool : BaseTool
	{
		public override void Simulate()
		{
			if ( !Game.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.PrimaryAttack ) )
					return;

				var tr = DoTrace();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				

				CreateHitEffects( tr.EndPosition );

				if ( tr.Entity.IsWorld )
					return;

				tr.Entity.Delete();

				var particle = Particles.Create( "particles/physgun_freeze.vpcf" );
				particle.SetPosition( 0, tr.Entity.Position );
			}
		}
	}
}
