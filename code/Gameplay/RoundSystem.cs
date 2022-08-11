using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHGame
{
	public enum WinnerEnum
	{
		Draw,
		Pigmask,
		Chimera
	}

	public enum RoundStatus
	{
		Idle,
		Starting,
		Active,
		Post,
		MapChange
	}

	public RoundStatus CurRoundStatus;

	public bool CanStart()
	{
		if ( Client.All.Count >= 2 )
			return true;

		return false;
	}

	public void StartGame()
	{
		if ( CurRoundStatus != RoundStatus.Idle )
			return;

		CurRoundStatus = RoundStatus.Starting;
	}

	public void StartRound()
	{
		if ( CurRoundStatus == RoundStatus.Active )
			return;

		CurRoundStatus = RoundStatus.Active;

		SelectChimera();

		foreach ( Client cl in Client.All )
		{
			if( cl.Pawn is UCHPawn player )
			{
				if ( player.Team == UCHPawn.TeamEnum.Chimera )
					continue;

				player.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
				player.Respawn();
			}
		}
	}

	public void CheckRoundStatus()
	{
		Log.Info( "Checking" );

		if ( GetTeamMembers( UCHPawn.TeamEnum.Pigmask ).Count <= 0 )
			EndRound( WinnerEnum.Chimera );
	}

	public void EndRound(WinnerEnum winningTeam = WinnerEnum.Draw)
	{
		Log.Info( "End Round" );
		CurRoundStatus = RoundStatus.Post;

		switch(winningTeam)
		{
			case WinnerEnum.Draw:
				Log.Info( "DRAW" );
				break;
			case WinnerEnum.Pigmask:
				Log.Info( "Pigmasks has won!" );
				break;
			case WinnerEnum.Chimera:
				Log.Info( "Chimera has won!" );
				break;
		}

		StartRound();

	}

	public void SelectChimera()
	{
		var players = GetEveryoneButChimera();

		var pastChimera = GetChimera();

		var randPlayer = players[Rand.Int( 0, players.Count - 1 )];

		randPlayer.SwitchTeam( UCHPawn.TeamEnum.Chimera );
		randPlayer.Respawn();

		if ( pastChimera != null && pastChimera is UCHPawn ply)
			ply.SwitchTeam(UCHPawn.TeamEnum.Pigmask);

	}

	public IList<UCHPawn> GetEveryoneButChimera()
	{
		IList<UCHPawn> playerList = new List<UCHPawn>();

		foreach ( Client cl in Client.All )
		{
			if ( cl.Pawn is UCHPawn player && player.Team != UCHPawn.TeamEnum.Chimera )
				playerList.Add( player );
		}

		return playerList;
	}

	public UCHPawn GetChimera()
	{
		foreach ( Client cl in Client.All )
		{
			if ( cl.Pawn is UCHPawn player && player.Team == UCHPawn.TeamEnum.Chimera )
				return player;
		}

		return null;
	}
	public IList<UCHPawn> GetTeamMembers( UCHPawn.TeamEnum teamEnum )
	{
		List<UCHPawn> playerList = new List<UCHPawn>();

		foreach ( Client cl in Client.All )
		{
			if ( cl.Pawn is UCHPawn player && player.Team == teamEnum )
				playerList.Add( player );
		}

		return playerList;
	}
}

