using Sandbox;

using Sandbox.UI;
using Sandbox.UI.Construct;
using Manikineko.MMO.Core.ui;
using Manikineko.MMO.Core.ui.Panels;

[Library]
public partial class InventoryMenu : Panel
{
	public static InventoryMenu Instance;
	readonly Panel toollist;

	public InventoryMenu()
	{
		Instance = this;
		if(!bool.Parse(ConsoleSystem.GetValue( "allow_inventorymenu" ) ) )
		{
			this.Delete();
			return;
		}
		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );

			{
				var maininv = body.AddChild<MainInventory>();
				var browser = body.AddChild<BrowserPanel>();
				browser.Style.Display = DisplayMode.None;

				tabs.AddButtonActive( "Click here to open a websurface", ( b ) => {

					browser.SetClass( "active", b );
					if ( b )
					{
						browser.Style.Display = DisplayMode.Flex;
					}
					else
					{
						browser.Style.Display = DisplayMode.None;

					}

				} );
				tabs.SelectedButton = tabs.AddButtonActive( "Player Inventory", ( b ) => { 
					maininv.SetClass( "active", b );

					
				} );
						
				
			}
		}
		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				
			}
		}
		/*
		var right = Add.Panel( "right" );
		{
			var tabs = right.Add.Panel( "tabs" );
			{
				tabs.Add.Button( "#inventorymenu.tools" ).AddClass( "active" );
				tabs.Add.Button( "#inventorymenu.utility" );
			}
			var body = right.Add.Panel( "body" );
			{
				toollist = body.Add.Panel( "toollist" );
				{
					
				}
				body.Add.Panel( "inspector" );
			}
		}
		*/

	}

	

	public override void Tick()
	{
		base.Tick();

		Parent.SetClass( "inventorymenuopen", Input.Down( InputButton.Menu ) );

	}

	

	public override void OnHotloaded()
	{
		base.OnHotloaded();

	}
}
