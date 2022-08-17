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

		Tags.Add( "ghost" );

		PhysicsClear();

		DressPlayer();
	}

	public void SetUpGhostPos(Vector3 lastPos)
	{
		Tags.Clear();

		SetModel( "models/player/ghost/standard_ghost.vmdl" );

		Animator = new UCHAnim();
		Controller = new GhostController();

		Tags.Add( "ghost" );

		PhysicsClear();

		ResetInterpolation();
		Position = lastPos;

		DressPlayer();
	}

	public void SetUpFancyGhost()
	{
		SetUpPlayer();

		SetModel( "models/player/ghost/fancy_ghost.vmdl" );

		Animator = new UCHAnim();
		Controller = new GhostController();

		Tags.Add( "ghost" );

		PhysicsClear();
	}
}
