using System;
using Sandbox;
public class GhostController : WalkController
{
	public GhostController()
	{

	}

	public override void Simulate()
	{
		base.Simulate();

		var vel = (Input.Rotation.Forward * Input.Forward) + (Input.Rotation.Left * Input.Left);

		if ( Input.Down( InputButton.Jump ) )
		{
			vel += Vector3.Up * 1;
		}

		vel = vel.Normal * 2000;

		Velocity += vel * Time.Delta;

		Velocity = Velocity.Approach( 0, Velocity.Length * Time.Delta * 5.0f );
		WishVelocity = Velocity;
		BaseVelocity = Vector3.Zero;

		Vector3 flyUp = 0;

		if ( Input.Down( InputButton.Jump ) )
			flyUp = 135 * Vector3.Up;

		if ( Input.Down( InputButton.Run ) )
			Velocity = new Vector3( Velocity.x, Velocity.y, flyUp.z );
	}
}
