using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	public void SetUpPigmask()
	{
		SetUpPlayer();

		SetModel( "models/player/pigmasks/pigmask.vmdl" );

		Animator = new UCHAnim();
		Controller = new WalkController();
	}
}
