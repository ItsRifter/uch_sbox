using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public class MrSaturn : AnimatedEntity
{
	public TimeSince timeThrown;

	UCHPawn lastPigmask;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/entity/mrsaturn.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		Tags.Add( "living" );
	}

	public MrSaturn PickUp(UCHPawn pigmask)
	{
		pigmask.PlaySound( "saturn_pickup" );
		SetParent( pigmask, "bip01_r_hand", Transform.Zero );
		EnableHideInFirstPerson = true;

		lastPigmask = pigmask;

		return this;
	}

	[Event.Tick.Server]
	public void SimulateSaturn()
	{
		if ( timeThrown > 0.75f )
			return;

		DebugOverlay.Sphere(Position, 16, Color.Green);

		var ents = FindInSphere( Position, 16 );

		foreach ( var ent in ents )
		{
			if (ent is UCHPawn pawn && pawn.Team == UCHPawn.TeamEnum.Chimera)
			{
				pawn.OnKilled();
				lastPigmask.RankUp();

				All.OfType<UCHPawn>().ToList().ForEach( x =>
				{
					x.PlaySoundOnClient( To.Single( x ), "saturn_win" );
				} );

			}
		}
	}

	public void Throw(UCHPawn pigmask)
	{
		EnableHideInFirstPerson = false;
		SetParent( null );
		Position = pigmask.EyePosition + pigmask.EyeRotation.Forward * 15;
		Velocity = pigmask.EyeRotation.Forward * 500;

		timeThrown = 0;
	}

	public void Kill()
	{
		DeleteAsync(3.0f);
	}
}

