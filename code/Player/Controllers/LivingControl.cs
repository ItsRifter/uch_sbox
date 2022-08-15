using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class LivingControl : WalkController
{
	UCHPawn player;
	public LivingControl()
	{

	}
	public LivingControl(UCHPawn pawn)
	{
		player = pawn;
	}

	public override float GetWishSpeed()
	{	
		if ( player == null )
			return DefaultSpeed;

		if ( Input.Down(InputButton.Run) && player.Stamina > 0 )
		{
			player.Stamina -= 1.0f;
			player.Stamina = player.Stamina.Clamp( 0, player.MaxStamina );
			player.TimeLastSprinted = 0;

			return SprintSpeed;
		}

		return DefaultSpeed;
	}
}

