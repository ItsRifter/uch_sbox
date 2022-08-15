﻿using System;
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

	MrSaturn holdingSaturn;

	public void SetUpPigmask()
	{
		if( holdingSaturn.IsValid() )
		{
			holdingSaturn.Delete();
			holdingSaturn = null;
		}

		SetUpPlayer();

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

		if ( Input.Pressed( InputButton.PrimaryAttack ) || Input.Pressed(InputButton.Use) )
		{
			if( holdingSaturn.IsValid() )
			{
				holdingSaturn.Throw(this);
				holdingSaturn = null;
				return;
			}

			var tr = Trace.Ray( EyePosition, EyePosition + EyeRotation.Forward * 75 )
				.Ignore(this)
				.WithoutTags("ghost")
				.UseHitboxes(true)
				.Run();

			if ( tr.Entity is MrSaturn saturn )
				holdingSaturn = saturn.PickUp( this );

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
