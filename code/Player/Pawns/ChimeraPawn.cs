using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	TimeSince timeSinceBite;

	public bool isDeactivated;

	public void SetUpChimera()
	{
		SetUpPlayer();

		isDeactivated = false;

		SetModel( "models/player/chimera/chimera.vmdl" );

		Animator = new UCHAnim();
		Controller = new WalkController();
		CameraMode = new ChimeraCam();

		EnableDrawing = true;
		EnableAllCollisions = true;
		EnableHitboxes = true;

		Tags.Add( "player", "living" );

		SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, new Capsule( new Vector3( 12, 0, 28 ), new Vector3( 48, 0, 42 ), 32 ) );

		PlaySoundOnClient( To.Single( this ), "chimera_spawn" );
	}

	public void ChimeraRagdoll()
	{
		EnableDrawing = false;

		var ent = new ModelEntity();
		ent.Tags.Add( "ragdoll" );
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;

		ent.SetModel( "models/player/chimera/chimera_off.vmdl" );

		ent.SetupPhysicsFromModel(PhysicsMotionType.Keyframed);

		ent.CopyBonesFrom( this );
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsEnabled = true;

		var birdEnt = new ModelEntity();
		birdEnt.Tags.Add( "ragdoll" );
		birdEnt.Position = Position;
		birdEnt.Rotation = Rotation;
		birdEnt.UsePhysicsCollision = true;
		birdEnt.EnableAllCollisions = true;

		birdEnt.SetModel( "models/player/chimera/birdgib.vmdl" );

		birdEnt.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		birdEnt.CopyBonesFrom( this );
		birdEnt.EnableAllCollisions = true;
		birdEnt.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		birdEnt.RenderColor = RenderColor;
		birdEnt.PhysicsEnabled = true;

		Corpse = ent;
	}

	public void SimulateChimera()
	{
		if( CanMove )
			Rotation = Rotation.FromYaw( EyeRotation.Yaw() );
		
		CanMove = timeSinceBite > 0.85f;

		if ( isDeactivated )
			return;

		if (Input.Pressed(InputButton.PrimaryAttack) && timeSinceBite > 0.85f && GroundEntity != null && !isDeactivated )
		{
			SetAnimParameter( "b_bite", true );
			PlaySound( "bite" );

			var wallTr = Trace.Ray( Position, Position + Rotation.Forward * 65 + Vector3.Up * 32 )
				.Ignore( this )
				.Run();

			Vector3 endPos = wallTr.EndPosition;
			int radius = 64;

			if ( wallTr.Hit && wallTr.Entity is WorldEntity )
			{
				endPos -= Rotation.Forward * 12;
				radius /= 2;
			}

			if(UCHGame.UCHCurrent.DebugMode)
				DebugOverlay.Sphere( endPos, radius, Color.Red, 2.0f );

			foreach ( Entity ent in FindInSphere( endPos, radius ) )
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
