using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn : Player
{
	public enum TeamEnum
	{
		Ghost,
		Pigmask,
		Chimera
	}

	[Net]
	public TeamEnum Team { get; private set; } = TeamEnum.Ghost;

	public void SwitchTeam(TeamEnum newTeam)
	{
		Log.Info( $"{Client.Name} swapped from {Team} to {newTeam}" );
		
		Team = newTeam;

		UpdateTeamClient( newTeam );
	}

	[ClientRpc]
	public void UpdateTeamClient(TeamEnum clTeam)
	{
		Team = clTeam;
	}

	public virtual void SetUpPlayer()
	{
		var spawnpoints = Entity.All.OfType<SpawnPoint>();
		var point = spawnpoints.OrderBy( x => new Guid() ).FirstOrDefault();

		if ( point != null )
			Transform = point.Transform;

		CameraMode = new FirstPersonCamera();
	}

	public override void Spawn()
	{
		base.Spawn();
		PhysicsClear();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		SetUpPlayer();

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public override void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		
		Game.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();

		switch (Team)
		{
			case TeamEnum.Ghost:
				SetUpGhost();
				break;
			case TeamEnum.Pigmask:
				SetUpPigmask();
				break;
			case TeamEnum.Chimera:
				//SetUpChimera();
				break;
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	TimeSince timeSinceLastFootstep = 0;

	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;
		Log.Info( tr.Surface.ResourceName );

		if (tr.Surface.ResourceName == "concrete")
		{
			Sound.FromWorld( "footstep_concrete", tr.EndPosition );
		}
	}

	public override void Simulate( Client cl )
	{
		//base.Simulate( cl );

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

	}
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}
}
