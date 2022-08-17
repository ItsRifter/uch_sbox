using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class UCHGame : Game
{
	public static UCHGame UCHCurrent => Current as UCHGame;

	public static bool Debug = UCHCurrent?.DebugMode ?? false;

	public bool DebugMode;

	public UCHGame()
	{
		if(IsServer)
		{
			CurRound = 0;
			CurRoundStatus = RoundStatus.Idle;
		}

		if(IsClient)
		{
			_ = new UCHHud();
		}
	}

	[Event.Hotload()]
	public void HotloadGame()
	{
		if ( IsClient )
		{
			_ = new UCHHud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );
		
		var pawn = new UCHPawn(client);

		pawn.SetUpGhost();
		client.Pawn = pawn;

		if ( CanStart() )
			StartGame();
		else
			pawn.PlayMusic(To.Single(pawn), "intro");
	}

	public override void DoPlayerSuicide( Client cl )
	{
		if ( Debug )
			base.DoPlayerSuicide( cl );
	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( !Debug )
			return;

		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		var player = cl.Pawn as UCHPawn;

		if(player.Team == UCHPawn.TeamEnum.Chimera)
			UCHCurrent.EndRound( WinnerEnum.Pigmask );
		else
			CheckRoundStatus();

		base.ClientDisconnect( cl, reason );
	}
}
