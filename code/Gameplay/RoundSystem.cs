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

	[Net]
    public int CurRound { get; private set; }

    [Net]
    public int MaxRounds { get; set; } = 15;

	[Net]
	public float RoundTimer { get; set; }

	[Net]
	public TimeSince TimeSinceGameplay { get; set; }

	[Net]
	public RoundStatus CurRoundStatus { get; private set; }

	bool mrSaturnSpawn;

	[Event.Tick.Server]
    public void TickGameplay()
	{
		if( CurRoundStatus == RoundStatus.Active)
		{
			if ( TimeSinceGameplay >= (RoundTimer / 2) && !mrSaturnSpawn )
				SpawnMrSaturn();
		}

        if ( TimeSinceGameplay >= RoundTimer )
		{
            switch(CurRoundStatus)
			{
                case RoundStatus.Starting:
                    StartRound();
                    break;
                case RoundStatus.Active:
                    EndRound( WinnerEnum.Draw );
                    break;
                case RoundStatus.Post:
                    StartRound();
                    break;
            }
		}
	}

	public void SpawnMrSaturn()
	{
		var mrSaturn = new MrSaturn();

		mrSaturn.Transform = All.OfType<MrSaturnSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault().Transform;

		All.OfType<UCHPawn>().ToList().ForEach( x =>
		{
			x.PlaySoundOnClient( To.Single( x ), "saturn_spawn" );
		} );

		mrSaturn.Spawn();
		mrSaturnSpawn = true;
	}

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

		foreach ( Client cl in Client.All )
		{
            if ( cl.Pawn is UCHPawn player )
            {
                player.StopMusic();
                player.PlaySoundOnClient( To.Single( player ), "start" );
            }
		}

        RoundTimer = 7.5f;
        TimeSinceGameplay = 0;
    }

	public void StartRound()
	{
		if ( CurRoundStatus == RoundStatus.Active )
			return;

        if( !CanStart() )
		{
            CurRoundStatus = RoundStatus.Idle;
            return;
		}

        Map.Reset( DefaultCleanupFilter );
		mrSaturnSpawn = false;

		RoundTimer = 180.0f;
        TimeSinceGameplay = 0;

        CurRoundStatus = RoundStatus.Active;

		SelectChimera();

		foreach ( Client cl in Client.All )
		{
			if( cl.Pawn is UCHPawn player )
			{
                player.PlayMusic( To.Single(player), "gameplay_music" );

                if ( player.Team == UCHPawn.TeamEnum.Chimera )
                    continue;

				player.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
				player.SetUpPigmask();
			}
		}

        CurRound++;
    }

	public void CheckRoundStatus()
	{
        if ( CurRoundStatus != RoundStatus.Active )
            return;

		if ( GetTeamMembers( UCHPawn.TeamEnum.Pigmask ).Count <= 0 )
			EndRound( WinnerEnum.Chimera );
	}

	public void EndRound(WinnerEnum winningTeam = WinnerEnum.Draw)
    {
		CurRoundStatus = RoundStatus.Post;

        if ( CurRound >= MaxRounds )
        {
            foreach ( Client cl in Client.All )
            {
                if ( cl.Pawn is UCHPawn player )
                {
                    player.StopMusic( To.Single( player ) );
                    player.PlayMusic( To.Single( player ), "gameplay_end" );
                }
            }

			CurRoundStatus = RoundStatus.MapChange;

			_ = new MapVoteEntity();

		}
        else
        {
            foreach ( Client cl in Client.All )
            {
                if ( cl.Pawn is UCHPawn player )
                {
                    player.StopMusic( To.Single( player ) );

                    if ( player.Team == UCHPawn.TeamEnum.Chimera )
                    {
                        player.isDeactivated = true;

                        switch ( winningTeam )
                        {
                            case WinnerEnum.Draw:
                                player.PlaySoundOnClient( To.Single( player ), "draw" );
                                break;

                            case WinnerEnum.Chimera:
                                player.PlaySoundOnClient( To.Single( player ), "chimera_win" );
                                break;

                            case WinnerEnum.Pigmask:
                                player.PlaySoundOnClient( To.Single( player ), "chimera_lose" );
                                break;
                        }
                    }

                    if ( player.Team == UCHPawn.TeamEnum.Ghost || player.Team == UCHPawn.TeamEnum.Pigmask )
                    {
                        switch ( winningTeam )
                        {
                            case WinnerEnum.Draw:
                                player.PlaySoundOnClient( To.Single( player ), "draw" );
                                break;

                            case WinnerEnum.Chimera:
                                player.PlaySoundOnClient( To.Single( player ), "pigmask_lose" );
                                break;

                            case WinnerEnum.Pigmask:
                                player.PlaySoundOnClient( To.Single( player ), "pigmask_win" );
                                break;
                        }
                    }
                }
            }
            RoundTimer = 10.0f;
            TimeSinceGameplay = 0;
        }
	}

	public void SelectChimera()
	{
		var players = GetEveryoneButChimera();

		var pastChimera = GetChimera();

		var randPlayer = players[Rand.Int( 0, players.Count - 1 )];

		randPlayer.SwitchTeam( UCHPawn.TeamEnum.Chimera );
		randPlayer.SetUpChimera();

		if ( pastChimera != null && pastChimera is UCHPawn ply )
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

