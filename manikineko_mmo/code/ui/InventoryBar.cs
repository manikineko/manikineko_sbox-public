﻿using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

public class InventoryBar : Panel
{
	readonly List<InventoryIcon> slots = new();

	public InventoryBar()
	{
		for ( int i = 0; i < 9; i++ )
		{
			var icon = new InventoryIcon( i + 1, this );
			if(icon != null){
			slots.Add( icon );
			}
		}
	}

	public override void Tick()
	{
		base.Tick();

		var player = Game.LocalPawn as MMOPlayer;
		if ( player == null ) return;
		if ( player.Inventory == null ) return;

		for ( int i = 0; i < slots.Count; i++ )
		{
			UpdateIcon( player.Inventory.GetSlot( i ), slots[i], i );
		}
	}

	private static void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
	{
		var player = Game.LocalPawn as MMOPlayer;

		if ( ent == null )
		{
			
			inventoryIcon.Clear();
			return;
		}

		var di = DisplayInfo.For( ent );
		inventoryIcon.SetEmpty( false );
		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = di.Name;
		var IconPath = $"/textures/ui/ent/"+ent.ClassName + ".png";
		if ( FileSystem.Mounted.FileExists( IconPath ) ) {
			inventoryIcon.Icon.Texture = Texture.Load( FileSystem.Mounted, IconPath, true );
		}
		else
		{
			inventoryIcon.Icon.Texture = Texture.Load( FileSystem.Mounted, $"/textures/ui/default.png", true );
		}
		inventoryIcon.SetClass( "active", player.ActiveChild == ent );
	}

	[Event.Client.BuildInput]
	public void ProcessClientInput()
	{
		var player = Game.LocalPawn as MMOPlayer;
		if ( player == null )
			return;

		var inventory = player.Inventory;
		if ( inventory == null )
			return;

		if ( player.ActiveChild is PhysGun physgun && physgun.BeamActive )
		{
			return;
		}

		if ( Input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( inventory, 0 );
		if ( Input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( inventory, 1 );
		if ( Input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( inventory, 2 );
		if ( Input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( inventory, 3 );
		if ( Input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( inventory, 4 );
		if ( Input.Pressed( InputButton.Slot6 ) ) SetActiveSlot( inventory, 5 );
		if ( Input.Pressed( InputButton.Slot7 ) ) SetActiveSlot( inventory, 6 );
		if ( Input.Pressed( InputButton.Slot8 ) ) SetActiveSlot( inventory, 7 );
		if ( Input.Pressed( InputButton.Slot9 ) ) SetActiveSlot( inventory, 8 );

		if ( Input.MouseWheel != 0 ) SwitchActiveSlot( inventory, -Input.MouseWheel );
	}

	private static void SetActiveSlot( IBaseInventory inventory, int i )
	{
		var player = Game.LocalPawn as MMOPlayer;

		if ( player == null )
			return;

		var ent = inventory.GetSlot( i );
		if ( player.ActiveChild == ent )
			return;

		if ( ent == null )
			return;

		player.ActiveChildInput = ent;
	}

	private static void SwitchActiveSlot( IBaseInventory inventory, int idelta )
	{
		var count = inventory.Count();
		if ( count == 0 ) return;

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while ( nextSlot < 0 ) nextSlot += count;
		while ( nextSlot >= count ) nextSlot -= count;

		SetActiveSlot( inventory, nextSlot );
	}
}
