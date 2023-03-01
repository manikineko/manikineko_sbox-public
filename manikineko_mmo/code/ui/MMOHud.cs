using Manikineko.MMO.Core;
using Sandbox;
using Sandbox.UI;

[Library]
public partial class MMOHud : HudEntity<RootPanel>
{
	public MMOHud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.StyleSheet.Load( "/Styles/Game.scss" );

		RootPanel.AddChild<Chat>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<VoiceSpeaker>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<AmmoHud>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<Crosshair>();
		RootPanel.AddChild<InventoryMenu>();
	}
}
