using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn
{
	public void SetUpPigmask()
	{
		SetUpPlayer();

		SetModel( "models/player/pigmasks/pigmask.vmdl" );

		Animator = new UCHAnim();
		Controller = new WalkController();

		EnableAllCollisions = true;

		SetupPhysicsFromCapsule( PhysicsMotionType.Keyframed, new Capsule( new Vector3(0, 0, 8), new Vector3(0, 0, 48), 24 ) );

		Tags.Add( "player", "living" );

		//var colBoundMax = Model.Bounds.Maxs;
		//colBoundMax.y /= 2;

		//SetupPhysicsFromOBB( PhysicsMotionType.Keyframed, Model.Bounds.Mins / 2, colBoundMax );
	}

	private void PigmaskRagdoll( Vector3 velocity )
	{
		var ent = new ModelEntity();
		ent.Tags.Add( "ragdoll" );
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;

		//ent.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		ent.SetModel( GetModelName() );

		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.CopyMaterialOverrides( this );
		ent.TakeDecalsFrom( this );
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		//ent.PhysicsGroup.Velocity = velocity;
		ent.PhysicsEnabled = true;

		Corpse = ent;

		ent.DeleteAsync( 10.0f );
	}
}
