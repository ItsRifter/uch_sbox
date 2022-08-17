using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	public TimeSince TimeScared;
	public TimeSince TimeLastWhipped;

	public enum PigRankEnum
	{
		Ensign,
		Captain,
		Major,
		Colonel
	}

	[Net]
	public PigRankEnum PigRank { get; private set; }

	[Net]
	public MrSaturn HoldingSaturn { get; protected set; }

	public void SetUpPigmask()
	{
		if( HoldingSaturn.IsValid() )
		{
			HoldingSaturn.Delete();
			HoldingSaturn = null;
		}

		SetUpPlayer();

		TimeScared = 10.0f;

		Animator = new UCHAnim();
		Controller = new LivingControl(this);

		EnableDrawing = true;
		EnableAllCollisions = true;
		
		Tags.Add( "player", "living" );

		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( new Vector3(0, 0, 8), new Vector3(0, 0, 42), 24 ) );

		switch( PigRank )
		{
			case PigRankEnum.Ensign:
				SetModel( "models/player/pigmasks/pigmask.vmdl" );
				PlaySoundOnClient( To.Single(this), "ensign_spawn" );
				MaxStamina = 45.0f;
				break;
			case PigRankEnum.Captain:
				SetModel( "models/player/pigmasks/pigmask_captain.vmdl" );
				PlaySoundOnClient( To.Single( this ), "captain_spawn" );
				MaxStamina = 65.0f;
				break;
			case PigRankEnum.Major:
				SetModel( "models/player/pigmasks/pigmask_major.vmdl" );
				PlaySoundOnClient( To.Single( this ), "major_spawn" );
				MaxStamina = 85.0f;
				break;
			case PigRankEnum.Colonel:
				SetModel( "models/player/pigmasks/pigmask_colonel.vmdl" );
				PlaySoundOnClient( To.Single( this ), "colonel_spawn" );
				MaxStamina = 100.0f;
				break;
		}

		Stamina = MaxStamina;

		DressPlayer();
	}

	public void RankUp()
	{
		switch ( PigRank )
		{
			case PigRankEnum.Ensign:
				PigRank = PigRankEnum.Captain;
				break;

			case PigRankEnum.Captain:
				PigRank = PigRankEnum.Major;
				break;

			case PigRankEnum.Major:
				PigRank = PigRankEnum.Colonel;
				break;
		}
	}

	public void SimulatePigmask()
	{
		if ( !IsServer )
			return;

		SetAnimParameter( "b_scared", TimeScared < 8.5f );

		if ( TimeScared < 8.5f )
		{
			if ( HoldingSaturn.IsValid() )
			{
				HoldingSaturn.Drop();
				HoldingSaturn = null;
				return;
			}

			if (CameraMode is not BehindPawnCam )
			{
				CameraMode = new BehindPawnCam();
			}
			return;
		} 
		else
		{
			if ( CameraMode is not FirstPersonCamera )
				CameraMode = new FirstPersonCamera();
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) || Input.Pressed(InputButton.Use) )
		{
			if ( TimeLastWhipped < 1.0f )
				return;

			if( HoldingSaturn.IsValid() )
			{
				HoldingSaturn.Throw(this);
				HoldingSaturn = null;
				return;
			}

			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 75 )
				.Ignore(this)
				.WithoutTags("ghost")
				.UseHitboxes(true)
				.Run();

			if ( tr.Entity is MrSaturn saturn )
				HoldingSaturn = saturn.PickUp( this );

			if(tr.Entity is UCHPawn player )
			{
				if ( player.Team == TeamEnum.Chimera && tr.HitboxIndex == 11 && !player.isDeactivated )
				{
					player.OnKilled();
					RankUp();
				}
			}

		}
	}

	private void PigmaskRagdoll( Vector3 eyeRot )
	{
		if ( HoldingSaturn.IsValid() )
		{
			HoldingSaturn.Drop();
			HoldingSaturn = null;
			return;
		}

		if ( CameraMode is not FirstPersonCamera )
			CameraMode = new FirstPersonCamera();

		PlaySoundOnClient(To.Single(this), "pig_die" );

		TimeScared = 10.0f;

		var ent = new ModelEntity();
		ent.Tags.Add( "ragdoll" );
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;

		ent.SetModel( GetModelName() );

		ent.CopyBonesFrom( this );
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsEnabled = true;

		Corpse = ent;

		ent.DeleteAsync( 10.0f );
	}
}
