using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	TimeSince timeSinceBite;

	Angles rotAngle;
	public void SetUpChimera()
	{
		SetUpPlayer();

		SetModel( "models/player/chimera/chimera.vmdl" );

		Animator = new UCHAnim();
		Controller = new WalkController();
		CameraMode = new ChimeraCam();

		EnableAllCollisions = true;
		EnableHitboxes = true;

		Tags.Add( "player", "living" );

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		var colBoundMax = Model.Bounds.Maxs;
		colBoundMax.y /= 2;

		//SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins / 2, colBoundMax );
	}

	public void SimulateChimera()
	{
		Rotation = Rotation.FromYaw( EyeRotation.Yaw() );

		if (Input.Pressed(InputButton.PrimaryAttack) && timeSinceBite > 0.85f)
		{
			SetAnimParameter( "b_bite", true );

			DebugOverlay.Sphere( Position + Rotation.Forward * 95 + Vector3.Up * 32, 64, Color.Red, 2.0f );

			foreach ( Entity ent in FindInSphere( Position + Rotation.Forward * 75 + Vector3.Up * 32, 64 ) )
			{
				if ( ent == this )
					continue;

				if(ent is UCHPawn player && player.Team == TeamEnum.Pigmask && IsServer)
					player.OnKilled();
			}

			timeSinceBite = 0;
		}
	}
}
