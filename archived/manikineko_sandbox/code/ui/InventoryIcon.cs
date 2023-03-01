
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class InventoryIcon : Panel
{
	public Entity TargetEnt;
	public Label Label;
	public Label Number;
	public Image Icon;

	public InventoryIcon( int i, Panel parent )
	{
		Parent = parent;
		Icon = Add.Image( "/textures/ui/ent/default.png","item-icon" );
		if(FileSystem.Mounted.FileExists( $"/textures/ui/ent/default.png" )){
	
		Icon.Texture = Texture.Load( FileSystem.Mounted, $"/textures/ui/ent/default.png", true );
		}
		Label = Add.Label( "empty", "item-name" );
		Number = Add.Label( $"{i}", "slot-number" );
	}
	public void SetEmpty(bool empty)
	{
		SetClass( "active", !empty);
		SetClass( "empty", empty );
	}
	public void Clear()
	{
		
	//	Icon.Style.Opacity = 0;
		Label.Text = "";
		SetEmpty(true);
	}
}
