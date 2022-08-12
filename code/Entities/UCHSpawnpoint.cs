using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;

[Library( "uch_spawnpoint_pigmask" )]
[Title( "Pigmask Spawn" ), Description( "The pigmasks spawnpoint" ), Category( "Spawnpoints" )]
[EditorModel( "models/player/pigmasks/pigmask.vmdl" )]
[HammerEntity]
public class PigmaskSpawnpoint : SpawnPoint
{

}

[Library( "uch_spawnpoint_chimera" )]
[Title( "Chimera Spawn" ), Description( "The chimera's spawnpoint" ), Category( "Spawnpoints" )]
[EditorModel( "models/player/chimera/chimera.vmdl" )]
[HammerEntity]
public class ChimeraSpawnpoint : SpawnPoint
{

}


[Library( "uch_spawnpoint_mrsaturn" )]
[Title( "Mr Saturn Spawn" ), Description( "Mr Saturn's spawnpoint" ), Category( "Spawnpoints" )]
[HammerEntity]
public class MrSaturnSpawnpoint : SpawnPoint
{

}

