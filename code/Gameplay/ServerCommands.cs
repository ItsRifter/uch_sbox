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

	[ConCmd.Admin("uch_setteam")]
	public static void DebugTeams(int teamIndex, string target = "")
	{
		var player = ConsoleSystem.Caller.Pawn as UCHPawn;
		
		if ( player == null )
			return;

		bool canRespawn = false;

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
					canRespawn = true;
					break;
				case 1:
					targetPlayer.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
					canRespawn = true;
					break;
				case 2:
					targetPlayer.SwitchTeam( UCHPawn.TeamEnum.Chimera );
					canRespawn = true;
					break;
				default:
					Log.Error( "Invalid team index to set" );
					break;
			}

			if ( canRespawn )
				targetPlayer.Respawn();
		} 
		else
		{
			switch ( teamIndex )
			{
				case 0:
					player.SwitchTeam(UCHPawn.TeamEnum.Ghost);
					canRespawn = true;
					break;
				case 1:
					player.SwitchTeam( UCHPawn.TeamEnum.Pigmask );
					canRespawn = true;
					break;
				case 2:
					player.SwitchTeam( UCHPawn.TeamEnum.Chimera );
					canRespawn = true;
					break;
				default:
					Log.Error( "Invalid team index to set" );
					break;
			}

			if (canRespawn)
				player.Respawn();
		}

	}
}
