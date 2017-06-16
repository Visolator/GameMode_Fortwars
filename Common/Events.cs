function fxDtsBrick::NoBuilderZone(%this, %client)
{
	if(!isObject(%client))
		return;

	if(isObject(%brickClient = getBrickgroupFromObject(%this).client) && %brickClient.getClassName() $= "GameConnection" && isObject(%brickClient.minigame))
	{
		if((%client.FortWars_getTeam() == 0 && %brickClient.FortWars_getTeam() == 0 || %brickClient.FortWars_getTeam() != %client.FortWars_getTeam()) && %client != %brickClient)
		{
			if(%client.isBuilder)
			{
				if(isObject(%player = %client.player))
					%player.setInvulnerbility(false);

				%client.isBuilder = 0;
				%client.lastBuilderSwitch = $Sim::Time;
				%client.FortWars_Apply(true, true);
				%client.chatMessage("\c6Builder is now \c0OFF\c6. You have entered a no builder zone.");
			}
		}
	}
	else if(isObject(%team = FortWars_TeamGroup.findExactTeamByBLID(getBrickgroupFromObject(%this).bl_id)))
	{
		if(%client.FortWars_getTeam() == 0 && %team != %client.FortWars_getTeam())
		{
			if(%client.isBuilder)
			{
				if(isObject(%player = %client.player))
					%player.setInvulnerbility(false);

				%client.isBuilder = 0;
				%client.lastBuilderSwitch = $Sim::Time;
				%client.FortWars_Apply(true, true);
				%client.chatMessage("\c6Builder is now \c0OFF\c6. You have entered a no builder zone.");
			}
		}
	}
}
registerOutputEvent("fxDTSBrick", "NoBuilderZone", "", 1);

function fxDTSBrick::FW_isOnTeam(%this, %client)
{
	%state = true;
	if(!isObject(%client))
		return;

	%team = FortWars_TeamGroup.findExactTeamByBLID($FortWars::Team[%this.getGroup().bl_id]);
	if(!isObject(%team))
		%team = FortWars_TeamGroup.findExactTeam($FortWars::Team[%this.getGroup().bl_id]);

	if(isObject(%team))
	{
		if(!%team.FortWars_isAllowed(%client.getBLID()) || %client.FortWars_getTeam() != %team || !isObject(%client.FortWars_getTeam()))
			%state = false;
	}
	else if(%this.getGroup().bl_id != %client.getBLID())
		%state = false;

	%this.FortWars_ProcessEvent(%client, %state);
}
registerOutputEvent("fxDTSBrick", "FW_isOnTeam", "", 1);

function fxDtsBrick::FortWars_ProcessEvent(%this, %client, %state)
{
	%state = (%state == true ? "onFortWarsTrue" : "onFortWarsFalse");
	%player = %client.player;

	$InputTarget_["Self"] = %this;      //The brick wich was sworded
	$InputTarget_["Player"] = %player;//The dude who sworded the brick
	$InputTarget_["Client"] = %client;       //The client of ^
	if($Server::LAN)
		$InputTarget_["MiniGame"] = getMiniGameFromObject(%client);
	else
	{
		if(getMiniGameFromObject(%this) == getMiniGameFromObject(%client))
			$InputTarget_["MiniGame"] = getMiniGameFromObject(%this);
		else
			$InputTarget_["MiniGame"] = 0;
	}

	%this.processInputEvent(%state, %client);
}

registerInputEvent("fxDTSBrick", "onFortWarsTrue", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "Minigame Minigame");
registerInputEvent("fxDTSBrick", "onFortWarsFalse", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "Minigame Minigame");

restrictInputEvent("fxDTSBrick", "onRelay");

restrictOutputEvent("fxDTSBrick", "RadiusImpulse");
//restrictOutputEvent("fxDTSBrick", "SetItem");
//restrictOutputEvent("fxDTSBrick", "SetItemDirection");
//restrictOutputEvent("fxDTSBrick", "SetItemPosition");
//restrictOutputEvent("fxDTSBrick", "SetVehicle");
//restrictOutputEvent("fxDTSBrick", "RespawnVehicle");
restrictOutputEvent("fxDTSBrick", "SpawnExplosion");
restrictOutputEvent("fxDTSBrick", "SpawnItem");
restrictOutputEvent("fxDTSBrick", "SetItem");
restrictOutputEvent("fxDTSBrick", "SpawnProjectile");

//restrictOutputEvent("Player", "AddHealth");
//restrictOutputEvent("Player", "AddVelocity");
//restrictOutputEvent("Player", "BurnPlayer");
//restrictOutputEvent("Player", "ChangeDatablock");
//restrictOutputEvent("Player", "ClearBurn");
//restrictOutputEvent("Player", "ClearTools");
//restrictOutputEvent("Player", "Dismount");
restrictOutputEvent("Player", "InstantRespawn");
restrictOutputEvent("Player", "Kill");
restrictOutputEvent("Player", "SetHealth");
restrictOutputEvent("Player", "SetPlayerScale");
//restrictOutputEvent("Player", "SetVelocity");
restrictOutputEvent("Player", "SpawnExplosion");
restrictOutputEvent("Player", "SpawnProjectile");
restrictOutputEvent("Player", "playSound");
restrictOutputEvent("Player", "setPlayerScaleFull");
restrictOutputEvent("Player", "setInvulnerbility");

//restrictOutputEvent("Bot", "AddHealth");
//restrictOutputEvent("Bot", "AddVelocity");
//restrictOutputEvent("Bot", "BurnPlayer");
//restrictOutputEvent("Bot", "ChangeDatablock");
//restrictOutputEvent("Bot", "ClearBurn");
//restrictOutputEvent("Bot", "ClearTools");
//restrictOutputEvent("Bot", "Dismount");
//restrictOutputEvent("Bot", "InstantRespawn");
//restrictOutputEvent("Bot", "Kill");
restrictOutputEvent("Bot", "SetHealth");
restrictOutputEvent("Bot", "SetPlayerScale");
//restrictOutputEvent("Bot", "SetVelocity");
restrictOutputEvent("Bot", "SpawnExplosion");
restrictOutputEvent("Bot", "SpawnProjectile");
restrictOutputEvent("Bot", "playSound");
restrictOutputEvent("Bot", "dropItem");
restrictOutputEvent("Bot", "setFriendlyFire");
restrictOutputEvent("Bot", "setInvulnerbility");

restrictOutputEvent("GameConnection", "IncScore");

restrictOutputEvent("MiniGame", "BottomPrintAll");
restrictOutputEvent("MiniGame", "CenterPrintAll");
restrictOutputEvent("MiniGame", "ChatMsgAll");
restrictOutputEvent("MiniGame", "Reset");
restrictOutputEvent("MiniGame", "RespawnAll");
restrictOutputEvent("MiniGame", "Win");

restrictOutputEvent("MiniGame", "setCameraBrick");
restrictOutputEvent("MiniGame", "setCameraNormal");
restrictOutputEvent("MiniGame", "playSound");
restrictOutputEvent("MiniGame", "setMusic");
restrictOutputEvent("MiniGame", "incTimeRemaining");

restrictOutputEvent("Player", "mountEmitter");

//For future events:

//VCE
restrictOutputEvent("fxDtsBrick","VCE_modVariable");
restrictOutputEvent("fxDtsBrick","VCE_ifValue");
restrictOutputEvent("fxDtsBrick","VCE_retroCheck");
restrictOutputEvent("fxDtsBrick","VCE_ifVariable");
restrictOutputEvent("fxDtsBrick","VCE_stateFunction");
restrictOutputEvent("fxDtsBrick","VCE_callFunction");
restrictOutputEvent("fxDtsBrick","VCE_relayCallFunction");
restrictOutputEvent("fxDtsBrick","VCE_saveVariable");
restrictOutputEvent("fxDtsBrick","VCE_loadVariable");

restrictOutputEvent("fxDtsBrick","spawnHomingProjectile");

restrictInputEvent("fxDtsBrick","onVariableTrue");
restrictInputEvent("fxDtsBrick","onVariableFalse");
restrictInputEvent("fxDtsBrick","onVariableFunction");
restrictInputEvent("fxDtsBrick","onVariableUpdate");
restrictInputEvent("fxDtsBrick","onMinigameJoin");

restrictOutputEvent("Player","VCE_ifVariable");
restrictOutputEvent("Player","VCE_modVariable");
restrictOutputEvent("GameConnection","VCE_ifVariable");
restrictOutputEvent("GameConnection","VCE_modVariable");
restrictOutputEvent("Minigame","VCE_ifVariable");
restrictOutputEvent("Minigame","VCE_modVariable");
restrictOutputEvent("Vehicle","VCE_ifVariable");
restrictOutputEvent("Vehicle","VCE_modVariable");

restrictOutputEvent("Player", "setMaxHealth");
restrictOutputEvent("Bot", "setMaxHealth");
restrictOutputEvent("Player", "addMaxHealth");
restrictOutputEvent("Bot", "addMaxHealth");
restrictOutputEvent("Player", "setInvulnerbilityTime");
restrictOutputEvent("Bot", "setInvulnerbilityTime");
restrictOutputEvent("Player", "setFInvulnerbilityTime");
restrictOutputEvent("Bot", "setFInvulnerbilityTime");

restrictOutputEvent("Player", "setFullScale");
restrictOutputEvent("Bot", "setFullScale");

restrictOutputEvent("fxDTSBrick", "BR_SetDist", 2);

restrictInputEvent("fxDtsBrick", "onMinigameSpawn");
restrictInputEvent("fxDtsBrick", "onMinigameJoin");
restrictInputEvent("fxDtsBrick", "onMinigameLeave");
restrictInputEvent("fxDtsBrick", "onMinigameDeath");
restrictInputEvent("fxDtsBrick", "onMinigameReset");