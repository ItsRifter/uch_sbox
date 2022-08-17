using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Crosshair : Panel
{
	Image crosshairImage;
	Image mrsaturn;
	public Crosshair()
	{
		StyleSheet.Load( "ui/crosshair.scss" );

		crosshairImage = Add.Image();
		mrsaturn = Add.Image(null, "mrsaturn");
	}

	public override void Tick()
	{
		base.Tick();

		var player = Local.Pawn as UCHPawn;

		if ( player == null )
			return;

		SetClass( "pigmask", player.Team != UCHPawn.TeamEnum.Chimera );
		mrsaturn.SetClass( "hassaturn", player.HoldingSaturn != null );

		if (player.Team == UCHPawn.TeamEnum.Chimera)
		{
			crosshairImage.SetTexture("");
			return;
		}

		switch (player.PigRank)
		{
			case UCHPawn.PigRankEnum.Ensign:
				crosshairImage.SetTexture( "ui/crosshair_ensign.png" );
				break;
			case UCHPawn.PigRankEnum.Captain:
				crosshairImage.SetTexture( "ui/crosshair_captain.png" );
				break;
			case UCHPawn.PigRankEnum.Major:
				crosshairImage.SetTexture( "ui/crosshair_major.png" );
				break;
			case UCHPawn.PigRankEnum.Colonel:
				crosshairImage.SetTexture( "ui/crosshair_colonel.png" );
				break;
		}
	}
}
