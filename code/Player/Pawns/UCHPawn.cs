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
		Tags.Clear();

		Entity spawnpoints = null;

		switch ( Team )
		{
			case TeamEnum.Pigmask:
				spawnpoints = All.OfType<PigmaskSpawnpoint>().OrderBy( x => new Guid() ).FirstOrDefault();
				break;
			case TeamEnum.Chimera:
				spawnpoints = All.OfType<ChimeraSpawnpoint>().OrderBy( x => new Guid() ).FirstOrDefault();
				break;

		}

		if ( spawnpoints != null && IsServer )
			Transform = spawnpoints.Transform;
		else if ( spawnpoints == null )
			Log.Error( "This map does not support Ultimate Chimera Hunt!" );

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
		EnableAllCollisions = true;

		//SetUpVisibility();
	}

	private void SetUpVisibility()
	{
		if ( !IsServer )
			return;

		foreach ( Client client in Client.All )
		{
			if ( Team == TeamEnum.Ghost )
			{
				if ( client.Pawn is UCHPawn ply )
				{
					if ( ply.Team == TeamEnum.Ghost )
						EnableVisibility( To.Single( ply ) );
					else if (ply.Team != TeamEnum.Ghost)
						DisableVisibility( To.Single( ply ) );
				}
			} 
			else
			{
				if ( client.Pawn is UCHPawn ply )
				{
					if ( ply.Team == TeamEnum.Ghost )
						DisableVisibility( To.Single( ply ) );
					else if (ply.Team != TeamEnum.Ghost)
						EnableVisibility( To.Single( ply ) );
				}
			}
		}
	}

	[ClientRpc]
	private void DisableVisibility()
	{
		EnableDrawing = false;
	}

	[ClientRpc]
	private void EnableVisibility()
	{
		EnableDrawing = true;
	}

	public override void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		
		Game.Current?.MoveToSpawnpoint( this );
		ResetInterpolation();

		Tags.Clear();

		switch (Team)
		{
			case TeamEnum.Ghost:
				SetUpGhost();
				break;
			case TeamEnum.Pigmask:
				SetUpPigmask();
				break;
			case TeamEnum.Chimera:
				SetUpChimera();
				break;
		}

		//SetUpVisibility();
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		switch (Team)
		{
			case TeamEnum.Pigmask:
				PigmaskRagdoll(Velocity);
				SetUpGhostPos( Position );
				break;
		}

		SwitchTeam( TeamEnum.Ghost );

		UCHGame.UCHCurrent.CheckRoundStatus();
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

		switch(Team)
		{
			case TeamEnum.Pigmask:
				Sound.FromWorld( "footstep_concrete", tr.EndPosition );
				break;
			case TeamEnum.Chimera:
				Sound.FromWorld( "chimera_step", tr.EndPosition );
				break;
		}
	}

	public override void Simulate( Client cl )
	{
		//base.Simulate( cl );

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

		TickPlayerUse();

		switch(Team)
		{
			case TeamEnum.Pigmask:

				break;

			case TeamEnum.Chimera:
				SimulateChimera();
				break;
		}

	}
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}
}
