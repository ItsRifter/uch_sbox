﻿using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class UCHGame : Sandbox.Game
{
	public static UCHGame UCHCurrent => Current as UCHGame;

	public static bool Debug = UCHCurrent?.DebugMode ?? false;

	public bool DebugMode;

	public UCHGame()
	{

	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );
		
		var pawn = new UCHPawn();
		pawn.SetUpGhost();
		client.Pawn = pawn;
	}

	public override void DoPlayerSuicide( Client cl )
	{
		var player = cl.Pawn as UCHPawn;

		if ( player.Team == UCHPawn.TeamEnum.Ghost )
			return;

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
		base.ClientDisconnect( cl, reason );
	}
}
