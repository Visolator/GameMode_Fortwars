function FortWars::minigameCanUse(%mode, %objA, %objB)
{
	if((%objA.getClassName() $= "Player" || %objA.getClassName() $= "AIPlayer") && (%objB.getClassName() $= "WheeledVehicle" || %objB.getClassName() $= "FlyingVehicle" || %objB.getClassName() $= "AIPlayer" && %objB.getDataBlock().rideable))
	{
		if(%objA.cannotMountVehicles)
		{
			return 0;
		}
	}

	if((%objB.getClassName() $= "Player" || %objB.getClassName() $= "AIPlayer") && (%objA.getClassName() $= "WheeledVehicle" || %objA.getClassName() $= "FlyingVehicle" || %objA.getClassName() $= "AIPlayer" && %objA.getDataBlock().rideable))
	{
		if(%objB.cannotMountVehicles)
		{
			return 0;
		}
	}

	return 1;
}

function FortWars::pickPlayerSpawnPoint(%mode, %client)
{
	if(isObject(%team = %client.FortWars_getTeam()))
		if(isObject(%spawn = FortWars_FindSpawnBrick(%team.ownerBL_ID)))
			return %spawn.getSpawnPoint();

	if(isObject(%spawn = FortWars_FindSpawnBrick(%client.getBLID())))
		return %spawn.getSpawnPoint();

	%brickgroupCount = 0;
	for(%i = 0; %i < MainBrickgroup.getCount(); %i++)
	{
		%brickgroup = MainBrickgroup.getObject(%i);
		if(%brickgroup.spawnBrickCount > 0)
		{
			%brickgroup[%brickgroupCount] = %brickgroup;
			%brickgroupCount++;
		}
	}

	if(%brickgroupCount > 0)
	{
		%selectBrickgroup = %brickgroup[getRandom(0, %brickgroupCount-1)];
		return %selectBrickgroup.getBrickSpawnPoint();
	}

	return "";
}

function FortWars::preMiniGameReset(%mode, %client)
{
	if(!isObject(%mini = %mode.minigame))
		return;

	%mini.FortWars_Loop();
}

function FortWars::onPlayerSpawn(%mode, %client)
{
	if(!isObject(%mini = %mode.minigame))
		return;

	if(!isObject(%player = %client.player))
		return;

	%client.FortWars_Apply(1);
}

function FortWars::miniGameCanDamage(%mode, %objA, %objB)
{
	return %objA.FortWars_CanDamage(%objB, $FortWars::Debug);
}

//v3 Support
function Slayer_FortWars_canUse(%miniA, %objA, %objB)
{
	if((%objA.getClassName() $= "Player" || %objA.getClassName() $= "AIPlayer") && (%objB.getClassName() $= "WheeledVehicle" || %objB.getClassName() $= "FlyingVehicle" || %objB.getClassName() $= "AIPlayer" && %objB.getDataBlock().rideable))
	{
		if(%objA.cannotMountVehicles)
		{
			return 0;
		}
	}

	if((%objB.getClassName() $= "Player" || %objB.getClassName() $= "AIPlayer") && (%objA.getClassName() $= "WheeledVehicle" || %objA.getClassName() $= "FlyingVehicle" || %objA.getClassName() $= "AIPlayer" && %objA.getDataBlock().rideable))
	{
		if(%objB.cannotMountVehicles)
		{
			return 0;
		}
	}
	
	return 1;
}

function Slayer_FortWars_pickSpawnPoint(%mini, %client)
{
	if(isObject(%team = %client.FortWars_getTeam()))
		if(isObject(%spawn = FortWars_FindSpawnBrick(%team.ownerBL_ID)))
			return %spawn.getSpawnPoint();

	if(isObject(%spawn = FortWars_FindSpawnBrick(%client.getBLID())))
		return %spawn.getSpawnPoint();

	%brickgroupCount = 0;
	for(%i = 0; %i < MainBrickgroup.getCount(); %i++)
	{
		%brickgroup = MainBrickgroup.getObject(%i);
		if(%brickgroup.spawnBrickCount > 0)
		{
			%brickgroup[%brickgroupCount] = %brickgroup;
			%brickgroupCount++;
		}
	}

	if(%brickgroupCount > 0)
	{
		%selectBrickgroup = %brickgroup[getRandom(0, %brickgroupCount-1)];
		return %selectBrickgroup.getBrickSpawnPoint();
	}

	return pickSpawnPoint();
}

function Slayer_FortWars_preMiniGameReset(%mini, %client)
{
	%mini.FortWars_Loop();
}

function Slayer_FortWars_onSpawn(%mini, %client)
{
	if(!isObject(%mini))
		return;

	if(!isObject(%player = %client.player))
		return;

	%client.FortWars_Apply(1);
}

function Slayer_FortWars_canDamage(%mini, %objA, %objB)
{
	return %objA.FortWars_CanDamage(%objB, $FortWars::Debug);
}

//OTHER STUFF

package FortWars_MinigameCanDamage
{
	function minigameCanDamage(%objA, %objB)
	{
		if(!isObject(%objA) || !isObject(%objB))
			return false;
		
		if(!getMinigameFromObject(%objB).isSlayerMinigame && (getMinigameFromObject(%objB).minigameMode $= "FortWars" || getMinigameFromObject(%objB).mode $= "FortWars"))
			return %objA.FortWars_CanDamage(%objB, %objA.damageDebug);

		return Parent::minigameCanDamage(%objA, %objB);
	}
};
if(!isPackage("FortWars_MinigameCanDamage"))
	schedule(0, 0, "activatePackage", "FortWars_MinigameCanDamage");

function SimObject::FortWars_CanDamage(%this, %target, %debug)
{
	if(!isObject(%this) || !isObject(%target))
		return false;

	switch$(%mainClass = %this.getClassName())
	{
		case "Player" or "AIPlayer":
			%mainObj = %this;
			%mainClient = %this.client;

		case "Projectile":
			%projectile = %this;
			%mainObj = %this.sourceObject;
			%mainClient = %this.client;

		case "GameConnection":
			%mainObj = %this.player;
			%mainClient = %this;
	}

	switch$(%targetClass = %target.getClassName())
	{
		case "Player" or "AIPlayer":
			%targetObj = %target;
			%targetClient = %target.client;
			%targetDead = (%targetObj.getState() $= "dead");

		case "Projectile":
			%targetObj = %target.sourceObject;
			%targetClient = %target.client;

		case "GameConnection":
			%targetObj = %target.player;
			%targetClient = %target;
	}

	if(!isObject(%mainObj) || !isObject(%targetObj))
	{
		if(isObject(%mainObj))
			if(%target.getClassName() $= "WheeledVehicle" || %target.getClassName() $= "FlyingWheeledVehicle")
			{
				if(isObject(%mainClient))
					if(isObject(%brickClient = getBrickgroupFromObject(%target).client) && %brickClient.getClassName() $= "GameConnection" && isObject(%brickClient.minigame))
					{
						if(%brickClient.FortWars_getTeam() == %mainClient.FortWars_getTeam() && %brickClient.FortWars_getTeam() != 0)
						{
							if(%debug)
								talk("SimObject::FortWars_CanDamage(false) - Vehicle brickgroup has same team");

							return false;
						}
					}
					else if(isObject(%brickGroup = getBrickgroupFromObject(%target)))
					{
						%team = FortWars_TeamGroup.findExactTeamByBLID($FortWars::Team[%brickGroup.bl_id]);
						if(!isObject(%team))
							%team = FortWars_TeamGroup.findExactTeam($FortWars::Team[%brickGroup.bl_id]);

						if(%team == %mainClient.FortWars_getTeam() && %mainClient.FortWars_getTeam() != 0)
							return false;
					}

				return true;
			}

		if(%debug)
			talk("SimObject::FortWars_CanDamage(false) - Missing object");

		return false;
	}

	if(%targetDead)
	{
		if(%debug)
			talk("SimObject::FortWars_CanDamage(false) - Target is dead");

		return false;
	}

	if($Pref::Server::FortWars_DoMinigameCheck)
		if(%mainClient.minigame != %targetClient.minigame)
		{
			if(%debug)
				talk("SimObject::FortWars_CanDamage(false) - Different minigames");

			return false;
		}

	if(%targetObj == %mainObj)
	{
		if(%debug)
			talk("SimObject::FortWars_CanDamage(true) - Same objects");

		return true;
	}

	if(%mainObj.isHoleBot)
	{
		if(%targetClient.isBuilder)
		{
			if(%debug)
				talk("SimObject::FortWars_CanDamage(false) - Bot wants to attack a builder");

			return false;
		}

		if(isObject(%targetClient))
		{
			if(isObject(%brickClient = getBrickgroupFromObject(%mainObj).client) && %brickClient.getClassName() $= "GameConnection" && isObject(%brickClient.minigame))
			{
				if(%brickClient.FortWars_getTeam() == %targetClient.FortWars_getTeam() && %targetClient.FortWars_getTeam() != 0)
				{
					if(%debug)
						talk("SimObject::FortWars_CanDamage(false) - Brickgroups have same team (main is hole bot)");

					return false;
				}
			}
			else if(isObject(%brickGroup = getBrickgroupFromObject(%mainObj)))
			{
				%team = FortWars_TeamGroup.findExactTeamByBLID($FortWars::Team[%brickGroup.bl_id]);
				if(!isObject(%team))
					%team = FortWars_TeamGroup.findExactTeam($FortWars::Team[%brickGroup.bl_id]);

				if(isObject(%team) && %team == %targetClient.FortWars_getTeam() && %targetClient.FortWars_getTeam() != 0)
				{
					if(%debug)
						talk("SimObject::FortWars_CanDamage(false) - Same team (main is hole bot) (Owner is not online)");

					return false;
				}
			}
		}
		else if(%targetObj.isHoleBot)
		{
			if(%debug)
				talk("SimObject::FortWars_CanDamage(?) - Checking hole bot teams (both are hole bots)");

			return !checkHoleBotTeams(%mainObj, %targetObj);
		}
	}

	if(%targetObj.isHoleBot)
	{
		if(isObject(%mainClient))
		{
			if(isObject(%brickClient = getBrickgroupFromObject(%mainClient).client) && %brickClient.getClassName() $= "GameConnection" && isObject(%brickClient.minigame))
			{
				if(%brickClient.FortWars_getTeam() == %mainClient.FortWars_getTeam() && %mainClient.FortWars_getTeam() != 0)
				{
					if(%debug)
						talk("SimObject::FortWars_CanDamage(false) - Brickgroups have same team (target is hole bot)");

					return false;
				}
			}
			else if(isObject(%brickGroup = getBrickgroupFromObject(%targetObj)))
			{
				%team = FortWars_TeamGroup.findExactTeamByBLID($FortWars::Team[%brickGroup.bl_id]);
				if(!isObject(%team))
					%team = FortWars_TeamGroup.findExactTeam($FortWars::Team[%brickGroup.bl_id]);

				if(isObject(%team) && %team == %mainClient.FortWars_getTeam() && %mainClient.FortWars_getTeam() != 0)
				{
					if(%debug)
						talk("SimObject::FortWars_CanDamage(false) - Brickgroups have same team (target is hole bot) (Owner is offline)");

					return false;
				}
			}

		}
		else if(%mainObj.isHoleBot)
		{
			if(%debug)
				talk("SimObject::FortWars_CanDamage(?) - Checking hole bot teams (both are hole bots)");

			return !checkHoleBotTeams(%mainObj, %targetObj);
		}
	}

	if(%targetClient == %mainClient)
	{
		if(%debug)
			talk("SimObject::FortWars_CanDamage(true) - Same client");

		return true;
	}

	if(isObject(%targetClient) && isObject(%mainClient))
	{
		if(%targetClient.FortWars_getTeam() == %mainClient.FortWars_getTeam() && %targetClient.FortWars_getTeam() != 0 && !%mainClient.FortWars_getTeam().FortWarsPref["FriendlyFire"])
		{
			if(%debug)
				talk("SimObject::FortWars_CanDamage(false) - Clients have same team");

			return false;
		}
	}

	return true;
}