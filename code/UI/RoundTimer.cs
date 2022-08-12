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
	public Label TimeLbl;

	public RoundTimer()
	{
		StyleSheet.Load( "UI/RoundTimer.scss" );
		TimeLbl = Add.Label( "Waiting for players", "text" );
	}

	public override void Tick()
	{
		base.Tick();

		TimeSpan timeDuration = TimeSpan.FromSeconds( UCHGame.UCHCurrent.RoundTimer - UCHGame.UCHCurrent.TimeSinceGameplay );

		switch(UCHGame.UCHCurrent.CurRoundStatus)
		{
			case UCHGame.RoundStatus.Starting:
				TimeLbl.Text = $"Starting in {timeDuration.ToString( @"m\:ss" )}";
				break;
			case UCHGame.RoundStatus.Active:
				TimeLbl.Text = $"Time left: {timeDuration.ToString( @"m\:ss" )}";
				break;
			case UCHGame.RoundStatus.Post:
				TimeLbl.Text = $"Restarting in: {timeDuration.ToString( @"m\:ss" )}";
				break;
			case UCHGame.RoundStatus.MapChange:
				TimeLbl.Text = "GAME OVER";
				break;
		}

	}
}

