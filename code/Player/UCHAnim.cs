using System;
using Sandbox;

public class UCHAnim : PawnAnimator
{
	TimeSince TimeSinceFootShuffle = 60;

	public override void Simulate()
	{
		var player = Pawn as UCHPawn;
		var idealRotation = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );

		DoRotation( idealRotation );
		DoWalk();

		SetAnimParameter( "b_grounded", GroundEntity != null );
		SetAnimParameter( "b_swim", Pawn.WaterLevel > 0.5f );
		
		SetAnimParameter( "b_voice", Input.Down(InputButton.Voice) );

		Vector3 aimPos = Pawn.EyePosition + Input.Rotation.Forward * 200;
		Vector3 lookPos = aimPos;

		//
		// Look in the direction what the player's input is facing
		//
		SetLookAt( "aim_eyes", lookPos );
		SetLookAt( "aim_head", lookPos );
		SetLookAt( "aim_body", aimPos );

		SetAnimParameter( "b_crouch", Input.Down( InputButton.Duck ) );

		if( player.Team == UCHPawn.TeamEnum.Chimera  )
		{
			SetAnimParameter( "b_running", Input.Down( InputButton.Run ) );
		}

		if ( player != null && player.ActiveChild is BaseCarriable carry )
		{
			carry.SimulateAnimator( this );
		}
		else
		{
			SetAnimParameter( "holdtype", 0 );
			SetAnimParameter( "aim_body_weight", 0.5f );
		}

	}

	public virtual void DoRotation( Rotation idealRotation )
	{
		var player = Pawn as Player;

		//
		// Our ideal player model rotation is the way we're facing
		//
		var allowYawDiff = player?.ActiveChild == null ? 90 : 50;

		float turnSpeed = 0.01f;
		if ( HasTag( "ducked" ) ) turnSpeed = 0.1f;

		//
		// If we're moving, rotate to our ideal rotation
		//
		Rotation = Rotation.Slerp( Rotation, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed );

		//
		// Clamp the foot rotation to within 120 degrees of the ideal rotation
		//
		Rotation = Rotation.Clamp( idealRotation, allowYawDiff, out var change );

		//
		// If we did restrict, and are standing still, add a foot shuffle
		//
		if ( change > 1 && WishVelocity.Length <= 1 ) TimeSinceFootShuffle = 0;

		SetAnimParameter( "b_shuffle", TimeSinceFootShuffle < 0.1 );
	}

	void DoWalk()
	{
		// Move Speed
		
		var dir = Velocity;
		var forward = Rotation.Forward.Dot( dir );
		var sideward = Rotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SetAnimParameter( "move_y", sideward );
		SetAnimParameter( "move_x", forward );
		SetAnimParameter( "move_z", Velocity.z );
	}

	public override void OnEvent( string name )
	{
		// DebugOverlay.Text( Pos + Vector3.Up * 100, name, 5.0f );

		if ( name == "jump" )
		{
			Trigger( "b_jump" );
		}

		base.OnEvent( name );
	}
}
