using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;
using System.Collections.Generic;

[Library]
public partial class MainInventory : Panel
{
	VirtualScrollPanel Canvas;
	public MainInventory()
	{
		AddClass( "inventorypage" );
		AddChild( out Canvas, "canvas" );

		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemWidth = 100;
		Canvas.Layout.ItemHeight = 100;

		Canvas.OnCreateCell = ( cell, data ) =>
		{
			var entname = (string)data;
			var panel = cell.Add.Panel( "icon" );
			panel.AddEventListener( "onclick",  () =>
			{
				ConsoleSystem.Run( "give", entname );
			} );
			

			if  (FileSystem.Mounted.FileExists( $"/textures/ui/ent/spawnable/{entname}.png" ))
			{
				
				panel.Style.BackgroundImage = Texture.Load( FileSystem.Mounted, $"/textures/ui/ent/spawnable/{entname}.png", false );
			}
			else
			{
				panel.Style.BackgroundImage = Texture.Load( FileSystem.Mounted, $"/textures/ui/ent/default.png", false );
			}
             panel.Tooltip = entname;

		};

		foreach ( var file in FileSystem.Mounted.FindFile( "textures/ui/ent/spawnable", "*.png", true ) )
		{
			if ( string.IsNullOrWhiteSpace( file ) ) continue;
			if ( file.Contains( "_lod0" ) ) continue;
			if ( file.Contains( "clothes" ) ) continue;
			var weaponname = file.Replace( ".png", "" );
			

			
			
				Canvas.AddItem( weaponname );
			
		}
	}
}
