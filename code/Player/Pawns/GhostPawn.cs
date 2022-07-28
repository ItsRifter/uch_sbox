using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class UCHPawn
{
	public void SetUpGhost()
	{
		SetUpPlayer();

		SetModel( "models/player/ghost/standard_ghost.vmdl" );

		Animator = new UCHAnim();
		Controller = new GhostController();
	}

	public void SetUpFancyGhost()
	{
		SetUpPlayer();

		SetModel( "models/player/ghost/fancy_ghost.vmdl" );

		Animator = new UCHAnim();
		Controller = new GhostController();
	}
}
