using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	TimeSince timeSinceBite;
	float tailStamina;
	TimeSince timeLastTail;
	TimeSince timeLastRoar;
	TimeSince timeLanded;

	public bool isDeactivated;
	public void SetUpChimera()
	{
		if ( HoldingSaturn.IsValid() )
		{
			HoldingSaturn.Delete();
			HoldingSaturn = null;
		}

		SetUpPlayer();

		isDeactivated = false;

		SetModel( "models/player/chimera/chimera.vmdl" );

		Animator = new UCHAnim();
		Controller = new LivingControl( this );
		CameraMode = new BehindPawnCam();

		EnableDrawing = true;
		EnableAllCollisions = true;
		EnableHitboxes = true;

		Tags.Add( "player", "living" );

		SetupPhysicsFromOrientedCapsule( PhysicsMotionType.Keyframed, new Capsule( new Vector3( 12, 0, 28 ), new Vector3( 48, 0, 42 ), 32 ) );

		PlaySoundOnClient( To.Single( this ), "chimera_spawn" );
		
		tailStamina = 100.0f;
		MaxStamina = 150.0f;
		Stamina = MaxStamina;

		ClearClothing();
	}

	public void ChimeraRagdoll()
	{
		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		CameraMode = new SpectateRagdollCamera();

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

		CanMove = timeSinceBite > 0.85f && timeLastRoar > 2.75f;

		if ( Velocity.z < -425.0f && GroundEntity == null && IsServer )
			timeLanded = 0;

		if ( timeLanded < 0.1f && GroundEntity is UCHPawn pig && pig.Team == TeamEnum.Pigmask && IsServer)
			pig.OnKilled();

		if ( isDeactivated )
			return;

		if( tailStamina < 100.0f && timeLastTail > 5.0f)
		{
			tailStamina += 1.0f;
			tailStamina = tailStamina.Clamp( 0, 100.0f );
		}

		if(Input.Pressed(InputButton.Reload) && timeLastTail > 1.5f)
		{
			if ( tailStamina <= 0 )
				return;

			tailStamina -= 50.0f;
			tailStamina = tailStamina.Clamp( 0, 100.0f );

			var ents = FindInSphere( Position + Rotation.Backward * 65 + Vector3.Up * 25, 64.0f );

			foreach ( var ent in ents )
			{
				if ( ent is UCHPawn player )
				{
					if ( player == this )
						continue;

					if ( player.Team == TeamEnum.Pigmask && IsServer )
					{
						player.Velocity = player.Position + player.Rotation.Up * 75;
						player.TimeLastWhipped = 0;
					}
				}
			}

			SetAnimParameter( "b_tail", true );
			timeLastTail = 0;
		}

		if(Input.Pressed(InputButton.SecondaryAttack) && timeLastRoar >= 12.5f)
		{
			PlaySound( "roar" );
			SetAnimParameter( "b_roar", true );
			timeLastRoar = 0;
		}

		if ( timeLastRoar <= 2.75f )
		{
			var ents = FindInSphere( Position, 250.0f );

			foreach ( var ent in ents )
			{
				if ( ent is UCHPawn player )
				{
					if ( player == this )
						continue;

					if ( player.Team == TeamEnum.Pigmask && player.TimeScared > 8.5f )
					{
						player.PlaySound( "pigmask_scream" );
						player.TimeScared = 0;
					}
				}
			}
		}

		if (Input.Pressed(InputButton.PrimaryAttack) && GroundEntity != null && !isDeactivated )
		{
			if ( timeLastRoar < 2.75f || timeSinceBite < 0.85f )
				return;

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

				if(ent is MrSaturn saturn && IsServer)
				{
					saturn.Kill();
				}

				if ( ent is UCHPawn player && player.Team == TeamEnum.Pigmask && IsServer )
				{
					player.OnKilled();
					player.PlaySound( "pigmask_death_scream" );

					if (UCHGame.UCHCurrent.CurRoundStatus == UCHGame.RoundStatus.Active)
						UCHGame.UCHCurrent.TimeSinceGameplay -= 30.0f;
				}
			}

			timeSinceBite = 0;
		}
	}
}
