using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	public enum PigRankEnum
	{
		Ensign,
		Captain,
		Major,
		Colonel
	}

	public PigRankEnum PigRank = PigRankEnum.Ensign;

	public void SetUpPigmask()
	{
		SetUpPlayer();

		Animator = new UCHAnim();
		Controller = new WalkController();

		EnableDrawing = true;
		EnableAllCollisions = true;
		
		Tags.Add( "player", "living" );

		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( new Vector3(0, 0, 8), new Vector3(0, 0, 42), 24 ) );

		switch( PigRank )
		{
			case PigRankEnum.Ensign:
				SetModel( "models/player/pigmasks/pigmask.vmdl" );
				PlaySoundOnClient( To.Single(this), "ensign_spawn" );
				break;
			case PigRankEnum.Captain:
				SetModel( "models/player/pigmasks/pigmask_captain.vmdl" );
				PlaySoundOnClient( To.Single( this ), "captain_spawn" );
				break;
			case PigRankEnum.Major:
				SetModel( "models/player/pigmasks/pigmask_major.vmdl" );
				PlaySoundOnClient( To.Single( this ), "major_spawn" );
				break;
			case PigRankEnum.Colonel:
				SetModel( "models/player/pigmasks/pigmask_colonel.vmdl" );
				PlaySoundOnClient( To.Single( this ), "colonel_spawn" );
				break;
		}
	}

	public void SimulatePigmask()
	{
		if ( !IsServer )
			return;

		if ( Input.Pressed( InputButton.PrimaryAttack ) || Input.Pressed(InputButton.Use) )
		{
			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 75 )
				.Ignore(this)
				.WithoutTags("ghost")
				.UseHitboxes(true)
				.Run();

			if(tr.Entity is UCHPawn player )
			{
				if ( player.Team == TeamEnum.Chimera && tr.HitboxIndex == 11 && !player.isDeactivated )
				{
					player.OnKilled();

					switch( PigRank )
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
			}

		}
	}

	private void PigmaskRagdoll( Vector3 eyeRot )
	{
		PlaySoundOnClient(To.Single(this), "pig_die" );

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

		ent.ApplyLocalImpulse( -eyeRot * 1000 + Vector3.Up * 999 );

		Corpse = ent;

		ent.DeleteAsync( 10.0f );
	}
}
