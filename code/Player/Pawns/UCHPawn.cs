using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class UCHPawn : Player
{
	[Net]
	public bool CanMove { get; protected set; }

	public enum TeamEnum
	{
		Ghost,
		Pigmask,
		Chimera
	}

	Sound curMusic;

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

	[ClientRpc]
	public void PlayMusic( string soundName )
	{
		curMusic = PlaySound( soundName ).SetVolume( 0.5f );
	}

	[ClientRpc]
	public void PlaySoundOnClient(string soundName)
	{
		PlaySound( soundName ).SetVolume( 0.75f );					
	}

	[ClientRpc]
	public void StopMusic()
	{
		curMusic.Stop();
	}

	public void SetUpPlayer()
	{
		Tags.Clear();

		LifeState = LifeState.Alive;
		CanMove = true;

		Entity spawnpoints = null;

		switch ( Team )
		{
			case TeamEnum.Ghost:
				spawnpoints = All.OfType<PigmaskSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
				break;
			case TeamEnum.Pigmask:
				spawnpoints = All.OfType<PigmaskSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
				break;
			case TeamEnum.Chimera:
				spawnpoints = All.OfType<ChimeraSpawnpoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();
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

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		LifeState = LifeState.Dead;

		switch (Team)
		{
			case TeamEnum.Pigmask:
				PigRank = PigRankEnum.Ensign;
				PigmaskRagdoll(EyeRotation.Forward);
				SetUpGhostPos( Position );
				break;
			case TeamEnum.Chimera:
				PlaySound( "button_pressed" );
				ChimeraRagdoll();
				isDeactivated = true;
				PhysicsClear();
				CanMove = false;
				UCHGame.UCHCurrent.EndRound( UCHGame.WinnerEnum.Pigmask );
				return;
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

		TickPlayerUse();

		switch (Team)
		{
			case TeamEnum.Pigmask:
				SimulatePigmask();
				break;

			case TeamEnum.Chimera:
				SimulateChimera();
				break;
		}

		if ( !CanMove )
			return;

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

	}
	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );
	}
}
