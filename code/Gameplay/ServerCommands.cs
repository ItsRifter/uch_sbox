using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHGame
{
	[ConCmd.Admin("uch_debug")]
	public static void DebugToggle(bool toggle)
	{
		Debug = toggle;
	}

	[ConCmd.Admin( "uch_vote_start" )]
	public static void StartMapVote()
	{
		_ = new MapVoteEntity();
	}

	[ConCmd.Admin("uch_game_start")]
	public static void RoundStartCMD()
	{
		if ( !Debug )
			return;

		UCHCurrent.CurRoundStatus = RoundStatus.Starting;

		UCHCurrent.StartRound();
	}

	[ConCmd.Server("uch_setteam")]
	public static void DebugTeams(int teamIndex, string target = "")
	{
		if ( !Debug )
			return;

		var player = ConsoleSystem.Caller.Pawn as UCHPawn;
		
		if ( player == null )
			return;

		if ( !string.IsNullOrEmpty( target ) )
		{
			UCHPawn targetPlayer = null;

			foreach ( Client cl in Client.All )
			{
				if (cl.Name.ToLower() == target.ToLower() )
				{
					targetPlayer = cl.Pawn as UCHPawn;
					break;
				}
			}

			if ( targetPlayer == null ) return;
			
			switch ( teamIndex )
			{
				case 0:
					targetPlayer.SwitchTeam( UCHPawn.TeamEnum.Ghost );
					targetPlayer.SetUpGhost();
					break;
				case 1:
					targetPlayer.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
					targetPlayer.SetUpPigmask();
					break;
				case 2:
					targetPlayer.SwitchTeam( UCHPawn.TeamEnum.Chimera );
					targetPlayer.SetUpChimera();
					break;
				default:
					Log.Error( "Invalid team index to set" );
					break;
			}
		} 
		else
		{
			switch ( teamIndex )
			{
				case 0:
					player.SwitchTeam(UCHPawn.TeamEnum.Ghost);
					player.SetUpGhost();
					break;
				case 1:
					player.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
					player.SetUpPigmask();
					break;
				case 2:
					player.SwitchTeam( UCHPawn.TeamEnum.Chimera );
					player.SetUpChimera();
					break;
				default:
					Log.Error( "Invalid team index to set" );
					break;
			}
		}

	}
}
