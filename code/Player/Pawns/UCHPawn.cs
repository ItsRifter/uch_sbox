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

	public float Stamina;

	public float MaxStamina;

	public TimeSince TimeLastSprinted;

	bool updateViewAngle;
	Angles updatedViewAngle;

	public enum TeamEnum
	{
		Ghost,
		Pigmask,
		Chimera
	}

	Sound curMusic;

	List<AnimatedEntity> ClothingModels;

	[Net]
	public TeamEnum Team { get; private set; } = TeamEnum.Ghost;

	ClothingContainer clothes = new();

	public UCHPawn()
	{

	}

	public UCHPawn(Client cl) : this()
	{
		clothes.LoadFromClient( cl );
		ClothingModels = new();
	}

	public void SwitchTeam(TeamEnum newTeam)
	{
		if(UCHGame.UCHCurrent.DebugMode)
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

	[ClientRpc]
	public void SetViewAngles( Angles angles )
	{
		updateViewAngle = true;
		updatedViewAngle = angles;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( updateViewAngle )
		{
			updateViewAngle = false;
			input.ViewAngles = updatedViewAngle;
		}
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
		{
			Transform = spawnpoints.Transform;
			SetViewAngles( To.Single(this), spawnpoints.Rotation.Angles() );
		}
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

	public void ClearClothing()
	{
		foreach ( var model in ClothingModels )
		{
			model.Delete();
		}

		ClothingModels.Clear();
	}

	public void DressPlayer()
	{
		ClearClothing();

		foreach ( var item in clothes.Clothing )
		{
			if ( item.SlotsUnder == Clothing.Slots.HeadTop )
			{
				var hat = new AnimatedEntity( item.Model );
				hat.SetParent( this, "hat", Transform.Zero );

				hat.LocalScale = 2.0f;
				hat.LocalPosition = GetAttachment( "hat", false ).Value.Position;
				hat.EnableHideInFirstPerson = true;

				ClothingModels.Add( hat );
			}

			if ( item.SlotsUnder == Clothing.Slots.Glasses )
			{
				var glasses = new AnimatedEntity( item.Model );
				glasses.SetParent( this, "glasses", Transform.Zero );

				glasses.LocalScale = 2.0f;
				glasses.LocalPosition = GetAttachment( "glasses", false ).Value.Position;
				glasses.EnableHideInFirstPerson = true;

				ClothingModels.Add( glasses );
			}
		}
	}

	public override void OnKilled()
	{
		LifeState = LifeState.Dead;
		
		ClearClothing();

		switch (Team)
		{
			case TeamEnum.Pigmask:
				PigRank = PigRankEnum.Ensign;
				PlaySoundOnClient("round_timer_add");
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

		if( TimeLastSprinted >= 3.5f && Stamina < MaxStamina )
		{
			Stamina += 0.25f;
			Stamina = Stamina.Clamp( 0, MaxStamina );
		}

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
