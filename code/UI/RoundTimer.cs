using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
public class RoundTimer : Panel
{
	public Panel RoundPanel;
	public Label TimeLbl;
	public Label RoundLbl;

	public RoundTimer()
	{
		StyleSheet.Load( "UI/RoundTimer.scss" );
		RoundPanel = Add.Panel( "panel" );
		TimeLbl = RoundPanel.Add.Label( "Idle", "timer" );
		RoundLbl = RoundPanel.Add.Label( "", "round" );
	}

	public override void Tick()
	{
		base.Tick();

		if(UCHGame.UCHCurrent.CurRoundStatus == UCHGame.RoundStatus.MapChange)
		{
			if ( RoundPanel != null )
			{
				RoundPanel?.DeleteChildren();
				RoundPanel?.Delete();
			}

			return;
		}

		if ( UCHGame.UCHCurrent.CurRoundStatus == UCHGame.RoundStatus.Idle )
		{
			RoundPanel.SetClass( "colonel", true );
			return;
		}

		if ( UCHGame.UCHCurrent.CurRoundStatus != UCHGame.RoundStatus.Active )
			return;

		var player = Local.Pawn as UCHPawn;
		
		if ( player == null )
			return;

		TimeSpan timeDuration = TimeSpan.FromSeconds( UCHGame.UCHCurrent.RoundTimer - UCHGame.UCHCurrent.TimeSinceGameplay );

		if( player.Team == UCHPawn.TeamEnum.Chimera)
		{
			RoundPanel.SetClass("chimera", true);
		} 
		else
		{
			RoundPanel.SetClass( "chimera", false );
			RoundPanel.SetClass( player.PigRank.ToString().ToLower() ?? "ensign", true );
		}

		switch (UCHGame.UCHCurrent.CurRoundStatus)
		{
			case UCHGame.RoundStatus.Starting:
				TimeLbl.Text = $"{timeDuration.ToString( @"m\:ss" )}";
				break;

			case UCHGame.RoundStatus.Active:
				TimeLbl.Text = $"{timeDuration.ToString( @"m\:ss" )}";
				break;

			case UCHGame.RoundStatus.MapChange:
				TimeLbl.Text = "GAME OVER";
				break;
		}

		RoundLbl.SetText( $"{UCHGame.UCHCurrent.CurRound}/{UCHGame.UCHCurrent.MaxRounds}" );

	}
}

