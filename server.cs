//+=========================================================================================================+\\
//|         Made by..                                                                                       |\\
//|        ____   ____  _                __           _                                                     |\\
//|       |_  _| |_  _|(_)              [  |        / |_                                                    |\\
//|         \ \   / /  __   .--.   .--.  | |  ,--. `| |-' .--.   _ .--.                                     |\\
//|          \ \ / /  [  | ( (`\]/ .'`\ \| | `'_\ : | | / .'`\ \[ `/'`\]                                    |\\
//|           \ ' /    | |  `'.'.| \__. || | // | |,| |,| \__. | | |                                        |\\
//|            \_/    [___][\__) )'.__.'[___]\'-;__/\__/ '.__.' [___]                                       |\\
//|                             BL_ID: 20490 | BL_ID: 48980                                                 |\\
//|             Forum Profile(48980): http://forum.blockland.us/index.php?action=profile;u=144888;          |\\
//|                                                                                                         |\\
//+=========================================================================================================+\\

//This is stupid do this since ForceRequiredAddOn WILL disconnect you if you are hosting non-dedicated for a missing/disabled add-on
//This is the most right way to exec required add-ons
$doReturn = 0;
if($GameModeArg $= "Add-Ons/GameMode_Custom/gamemode.txt" || $GameModeArg $= "")
{
	$error = ForceRequiredAddOn("Support_NewHealth");
	if($error == $Error::AddOn_NotFound)
	{
		warn("ERROR: GameMode_FortWars - required add-on Support_NewHealth not found");
		return;
	}

	//--------------------------------------

	$error = ForceRequiredAddOn("Support_FindItemByName");
	if($error == $Error::AddOn_NotFound)
	{
		warn("ERROR: GameMode_FortWars - required add-on Support_FindItemByName not found");
		return;
	}

	//--------------------------------------

	$error = ForceRequiredAddOn("Support_CodeLibrary");
	if($error == $Error::AddOn_NotFound)
	{
		warn("ERROR: GameMode_FortWars - required add-on Support_CodeLibrary not found");
		return;
	}

	//--------------------------------------

	$error = ForceRequiredAddOn("Event_DisablePaintBuild");
	if($error == $Error::AddOn_NotFound)
	{
		warn("ERROR: GameMode_FortWars - required add-on Event_DisablePaintBuild not found");
		return;
	}

	//--------------------------------------

	$error = ForceRequiredAddOn("Gamemode_Slayer");
	if($error == $Error::AddOn_NotFound)
	{
		warn("ERROR: GameMode_FortWars - required add-on Gamemode_Slayer not found");
		return;
	}
}
else
{
	//I am hoping other add-ons that require stuff would use this kind of method
	if($GameMode::LastAddOnCount $= "" || $GameMode::AddOnCount != $GameMode::LastAddOnCount)
	{
		$GameMode::LastAddOnCount = $GameMode::AddOnCount;
		deleteVariables("$GameMode::NameAddOn*");
		for($FC = 0; $FC < $GameMode::AddOnCount; $FC++)
		{
			$addonName = $GameMode::AddOn[$FC];
			$GameMode::NameAddOn[$addonName] = 1;
		}
	}

	if(!isFile("Add-Ons/Support_NewHealth/server.cs") || !$GameMode::NameAddOn["Support_NewHealth"])
	{
		$doReturn = 1;
		warn("ERROR: GameMode_FortWars - required add-on Support_NewHealth not found");
	}

	if(!isFile("Add-Ons/Support_FindItemByName/server.cs") || !$GameMode::NameAddOn["Support_FindItemByName"])
	{
		$doReturn = 1;
		warn("ERROR: GameMode_FortWars - required add-on Support_FindItemByName not found");
	}

	if(!isFile("Add-Ons/Support_CodeLibrary/server.cs") || !$GameMode::NameAddOn["Support_CodeLibrary"])
	{
		$doReturn = 1;
		warn("ERROR: GameMode_FortWars - required add-on Support_CodeLibrary not found");
	}

	if(!isFile("Add-Ons/Event_DisablePaintBuild/server.cs") || !$GameMode::NameAddOn["Event_DisablePaintBuild"])
	{
		$doReturn = 1;
		warn("ERROR: GameMode_FortWars - required add-on Event_DisablePaintBuild not found");
	}

	if(!isFile("Add-Ons/Gamemode_Slayer/server.cs") || !$GameMode::NameAddOn["Gamemode_Slayer"])
	{
		$doReturn = 1;
		warn("ERROR: GameMode_FortWars - required add-on Gamemode_Slayer not found");
	}

	if($doReturn)
		return;
}

function FortWars_loadFilePath(%path)
{
	if(strPos(%path,"*") == -1)
		%path = %path @ "*";

	if(getFileCount(%path) <= 0)
		return -1;

	for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path))
	{
		%fileExt = fileExt(%file);
		if(%fileExt !$= ".cs")
			continue;

		exec(%file);
	}

	return 1;
}

function FortWars_LoadCore()
{
	exec("add-ons/Gamemode_FortWars/server.cs");
}

function FortWars_LoadAll()
{
	FortWars_LoadCore();
	FortWars_LoadSupport();
	FortWars_LoadCommon();
}

function FortWars_LoadSupport()
{
	FortWars_loadFilePath("add-ons/Gamemode_FortWars/Support/*");
}

function FortWars_LoadCommon()
{
	FortWars_loadFilePath("add-ons/Gamemode_FortWars/Common/*");
}

if(!$FortWars::LoadedSupport)
{
	$FortWars::LoadedSupport = 1;
	FortWars_LoadSupport();
}

if(!$FortWars::LoadedCommon)
{
	$FortWars::LoadedCommon = 1;
	FortWars_LoadCommon();
}

function FW_SetQuota(%type, %num)
{
	%num = mFloor(%num);

	switch$(%type)
	{
		case "Bot":
			$Pref::Server::Quota::Player = %num;
			$Server::Quota::Player = %num;
			for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
			{
				%bg = MainBrickgroup.getObject(%a);
				for(%i = %bg.getCount() - 1; %i > 0; %i--)
				{
					%b = %bg.getObject(%i);
					if(isObject(%q = %b.quotaObject))
						%q.delete();

					getQuotaObjectFromBrickGroup(%b);
				}
			}
			talk("Max bots per player set to: " @ %num);

		case "Vehicle":
			$Pref::Server::Quota::Vehicle = %num;
			$Server::Quota::Vehicle = %num;
			for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
			{
				%bg = MainBrickgroup.getObject(%a);
				for(%i = %bg.getCount() - 1; %i > 0; %i--)
				{
					%b = %bg.getObject(%i);
					if(isObject(%q = %b.quotaObject))
						%q.delete();

					getQuotaObjectFromBrickGroup(%b);
				}
			}
			talk("Max vehicles per player set to: " @ %num);

		case "Event":
			$Pref::Server::Quota::Schedules = %num;
			$Server::Quota::Schedules = %num;
			$Max::Quota::Schedules = %num;
			for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
			{
				%bg = MainBrickgroup.getObject(%a);
				for(%i = %bg.getCount() - 1; %i > 0; %i--)
				{
					%b = %bg.getObject(%i);
					if(isObject(%q = %b.quotaObject))
						%q.delete();

					getQuotaObjectFromBrickGroup(%b);
				}
			}
			talk("Max events per player set to: " @ %num);

		case "Item":
			$Pref::Server::Quota::Item = %num;
			$Server::Quota::Item = %num;
			for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
			{
				%bg = MainBrickgroup.getObject(%a);
				for(%i = %bg.getCount() - 1; %i > 0; %i--)
				{
					%b = %bg.getObject(%i);
					if(isObject(%q = %b.quotaObject))
						%q.delete();

					getQuotaObjectFromBrickGroup(%b);
				}
			}
			talk("Max items per player set to: " @ %num);

		case "Environment":
			$Pref::Server::Quota::Environment = %num;
			$Server::Quota::Environment = %num;
			for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
			{
				%bg = MainBrickgroup.getObject(%a);
				for(%i = %bg.getCount() - 1; %i > 0; %i--)
				{
					%b = %bg.getObject(%i);
					if(isObject(%q = %b.quotaObject))
						%q.delete();

					getQuotaObjectFromBrickGroup(%b);
				}
			}
			talk("Max environment per player set to: " @ %num);
	}
}

function FortWars_Init()
{
	PlayerNoJet.maxForwardSpeed = 10;
	PlayerNoJet.maxSideSpeed = 8;
	PlayerNoJet.maxBackwardSpeed = 7;
	PlayerNoJet.jumpForce = 1240;

	if(isObject(Slayer) && !$Server::FortWars::InitSlayer)
	{
		$Server::FortWars::InitSlayer = 1;
		if(mFloor($Slayer::Version) <= 3)
			Slayer.Gamemodes.addMode("Fort Wars", "FortWars", 0, 0);
		else
		{
			new ScriptGroup(Slayer_GameModeTemplateSG)
			{
				// Game mode settings
				className = "FortWars";
				uiName = "Fort Wars";
				useTeams = false;

				// Default minigame settings
				default_lives = 0;
				default_brickDamage = false;

				// Locked minigame settings
				locked_title = "Fort Wars";
				locked_weaponDamage = true;
				locked_playerDatablock = nameToID("PlayerNoJet");
			};
		}
	}
}
schedule(0, 0, "FortWars_Init");

$Server::FortWars::Version = 0.2;

//-------------------------------------------------------------------------------------------------------
//Fort wars! - Yay
//-------------------------------------------------------------------------------------------------------

datablock StaticShapeData(StaticShield)
{
	shapeFile = "./Shapes/Shield.dts";
};

if($Pref::Server::FortWars_ShieldTime <= 0)
	$Pref::Server::FortWars_ShieldTime = 0.5;

if($Pref::Server::FortWars_MinigameDamage $= "")
	$Pref::Server::FortWars_MinigameDamage = 1;

if($Pref::Server::FortWars_AllowTeams $= "")
	$Pref::Server::FortWars_AllowTeams = 1;

if(isPackage("FortWars_Main"))
	deactivatePackage("FortWars_Main");

package FortWars_Main
{
	function Player::ChangeDataBlock(%player, %data, %client)
	{
		if(isObject(%client))
		{
			if(!%client.isBuilder && %data.canJet)
			{
				%client.centerPrint("You are not allowed to change into\n\c3" @ %data.uiName, 3);
				return;
			}
		}
		else if(isObject(%cl = %player.client))
		{
			if(!%client.isBuilder && %data.canJet)
			{
				%client.centerPrint("You are not allowed to change into\n\c3" @ %data.uiName, 3);
				return;
			}
		}

		return Parent::ChangeDataBlock(%player, %data, %client);
	}

	function GameConnection::createPlayer(%this, %transform)
	{
		if(isObject(%minigame = %this.minigame))
			if(!%minigame.isSlayerMinigame)
				if(%minigame.minigameMode $= "FortWars" || %minigame.mode $= "FortWars")
				{
					cancel(%this.FortWarsApplySch);
					%this.FortWarsApplySch = %this.schedule(33, "FortWars_Apply");
				}

		return Parent::createPlayer(%this, %transform);
	}

	function GameConnection::onClientEnterGame(%this)
	{
		%this.isBuilder = true;
		return Parent::onClientEnterGame(%this);
	}

	function Player::Damage(%this, %source, %position, %damage, %damageType)
	{
		if(!isObject(%source))
		{
			Parent::Damage(%this, %source, %position, %damage, %damageType);
			return;
		}

		switch$(%targetClass = %source.getClassName())
		{
			case "Player" or "AIPlayer":
				%targetObj = %source;
				%targetClient = %source.client;
				%targetDead = (%targetObj.getState() $= "dead");

			case "Projectile":
				%targetObj = %source.sourceObject;
				%targetClient = %source.client;

			case "GameConnection":
				%targetObj = %source.player;
				%targetClient = %source;
		}

		if(!isObject(%this) || !isObject(%targetObj))
		{
			Parent::Damage(%this, %source, %position, %damage, %damageType);
			return;
		}

		if(%this.getClassName() !$= "Player" || %targetObj.getClassName() !$= "Player")
		{
			Parent::Damage(%this, %source, %position, %damage, %damageType);
			return;
		}

		if(%this.getState() $= "dead")
		{
			Parent::Damage(%this, %source, %position, %damage, %damageType);
			return;
		}

		if(%damageType == $DamageType::HammerDirect && %targetObj.hammerHits[%this] < 3)
		{
			%targetObj.hammerHits[%this]++;
			cancel(%targetObj.hammerHitSch[%this]);
			%targetObj.hammerHitSch[%this] = %targetObj.schedule(5000, "FortWars_ResetHammer", %this);

			%damage = 0;
			Parent::Damage(%this, %source, %position, %damage, %damageType);
			return;
		}
		else if(%damageType == $DamageType::HammerDirect)
			%damage = 0;

		Parent::Damage(%this, %source, %position, %damage, %damageType);

		%client = %this.client;

		//Make the target no longer invincible, as a builder or not. Reflect the damage.
		if(%targetObj != %this)
		{
			if(%this.isInvincible || %client.isBuilder)
			{
				%targetObj.setInvulnerbility(false);
				%targetObj.damage(%targetObj, %targetObj.getPosition(), %targetObj.getMaxHealth() * 0.1); //Do 10% damage back
				if(isObject(%targetClient) && $Sim::Time - %targetObj.lastDamage[%this] > 3)
				{
					%targetClient.lastBuilderSwitch = $Sim::Time;
					%targetObj.lastDamage[%this] = $Sim::Time;
					messageClient(%targetClient, '', "\c6You damaged a builder! (\c3" @ %targetClient.getPlayerName() @ "\c6) \c7- \c010\c6% reflect damage");
				}
			}

			if(%targetObj.isInvincible || %targetClient.isBuilder)
			{
				%targetObj.setInvulnerbility(false);
				if(isObject(%targetClient) && %targetClient.isBuilder)
				{
					%targetClient.isBuilder = 0;
					%targetClient.lastBuilderSwitch = $Sim::Time;
					%targetClient.FortWars_Apply(true, true);
					%targetClient.chatMessage("\c6Builder is now \c0OFF\c6. Tools cleared due to hurting people in builder mode.");
				}
			}

			if(%client.isBuilder)
				%this.spawnShield($Pref::Server::FortWars_ShieldTime);

			if(!%targetClient.isBuilder)
			{
				if($Sim::Time - %targetClient.lastBuilderSwitch < 5)
					%targetClient.lastBuilderSwitch = $Sim::Time - 10;
			}
		}
	}

	function fxDTSBrick::onPlant(%brick)
	{
		Parent::onPlant(%brick);
		%brick.schedule(0, "FortWars_Check");
	}

	function fxDTSBrick::onLoadPlant(%brick)
	{       
		Parent::onLoadPlant(%brick);
		%brick.schedule(0, "FortWars_Check", 1);
	}

	function GameConnection::pickSpawnPoint(%client)
	{
		if(!isObject(%client.minigame))
		{
			if(isObject(%team = %client.FortWars_getTeam()))
				if(isObject(%spawn = FortWars_FindSpawnBrick(%team.ownerBL_ID)))
					return %spawn.getSpawnPoint();

			if(isObject(%spawn = FortWars_FindSpawnBrick(%client.getBLID())))
				return %spawn.getSpawnPoint();
		}

		return Parent::pickSpawnPoint(%client);
	}

	function MinigameSO::onDefaultMinigameInit(%this)
	{
		Parent::onDefaultMinigameInit(%this);
		%this.minigameMode = "FortWars";
		%this.FortWars_Loop();
	}

	function MiniGameSO::pickSpawnPoint(%mini, %client)
	{
		if(!%mini.isSlayerMinigame)
		{
			if(isObject(%team = %client.FortWars_getTeam()))
				if(isObject(%spawn = FortWars_FindSpawnBrick(%team.ownerBL_ID)))
					return %spawn.getSpawnPoint();

			if(isObject(%spawn = FortWars_FindSpawnBrick(%client.getBLID())))
				return %spawn.getSpawnPoint();
		}

		return Parent::pickSpawnPoint(%mini, %client);
	}

	function minigameCanUse(%objA, %objB)
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

		return parent::minigameCanUse(%objA, %objB);
	}

	function fxDTSBrick::setItem(%this, %item, %client)
	{
		if(!%this.getDatablock().isBotHole && isObject(%client) && %client.getClassName() $= "GameConnection" && isObject(%item))
		{
			%msg = "\c6Sorry, please use events\n\c6This is to prevent client lag\n\n\c3[X] [0] [onActivate] [Player] [addNewItem] [ItemName]";
			%client.chatMessage(%msg);
			%client.centerPrint(%msg, 3);
			%item = 0;
		}

		Parent::setItem(%this, %item, %client);
	}
};
activatePackage("FortWars_Main");

function MinigameSO::FortWars_Loop(%this)
{
	if(!isObject(%this))
		return;

	cancel(%this.FortWars_LoopSch);
	for(%i = 0; %i < %this.numMembers; %i++)
	{
		%member = %this.member[%i];
		if(isObject(%member) && %member.getClassName() $= "GameConnection" && isObject(%player = %member.player))
		{
			%hp = mClampF(%player.getHealth() / %player.getMaxHealth(), 0, 1);
			%hpPer = %hp * 100;
			%invincible = (%player.isInvincible ? "\c7" : "");

			%bottomprint[%i] = "\c6Health: <color:" @ rgbToHex(greenToRed(%hp)) @ ">" @ %invincible @ %hpPer @ "\c6%";
			%bottomprint[%i] = %bottomprint[%i] @ "<just:right>\c6Mode: " @ (%member.isBuilder ? "\c5Builder" : "\c0Combat") @ " \c6(\c3/ToggleBuild\c6) ";
			%bottomprint[%i] = %bottomprint[%i] @ "<br>\c3" @ (%member.isBuilder ? "Cannot kill/ride vehicles " : "Cannot build/kill builders ");

			if(%member.FortWarsData["Bottomprint"] !$= %bottomprint[%i] || $Sim::Time - %member.FortWarsData["BottomprintTime"] > 5)
			{
				%member.FortWarsData["Bottomprint"] = %bottomprint[%i];
				%member.FortWarsData["BottomprintTime"] = $Sim::Time;

				%member.bottomPrint(%bottomprint[%i], 6, 1);
			}
		}
	}

	%this.FortWars_LoopSch = %this.schedule(100, "FortWars_Loop");
}

function fxDTSBrick::FortWars_Check(%brick, %bypass)
{
	%client = %brick.client;
	if(!isObject(%client))
		%client = %brick.getGroup().client;

	if(!isObject(%client))
		%bypass = 1;

	%id = %brick.getGroup().bl_id;

	if(%bypass)
		%adminLevel = 3;

	if(isObject(%client))
		%adminLevel = %client.isAdmin + %client.isSuperAdmin + (%brick.getGroup().bl_id == getNumKeyID() ? 1 : 0);

	if(%brick.getDatablock() == nameToID(brickSpawnPointData) && %adminLevel < 2 && !%bypass)
	{
		if(isObject(%client))
			%client.centerPrint("Sorry, you must be admin to plant this brick.<br>\c6Please use \c3" @ brickFortWarsTeamData.uiName @ "\c6 in \c3SPECIALS \c6if you want a personal spawn.", 4);

		%brick.schedule(0, delete);
		return 0;
	}

	if(hasFieldOnList($Pref::Server::Fortwars_IgnoreCategory, %brick.getDatablock().category) && %adminLevel < 2 && !%bypass)
	{
		if(isObject(%client))
			%client.centerPrint("Sorry, this brick cannot be planted due to it being on an anti-category list.<br>Blocked category: " @ %brick.getDatablock().category, 4);

		%brick.schedule(0, delete);
		return 0;
	}

	if(hasFieldOnList($Pref::Server::Fortwars_IgnoreSubCategory, %brick.getDatablock().subcategory) && %adminLevel < 2 && !%bypass)
	{
		if(isObject(%client))
			%client.centerPrint("Sorry, this brick cannot be planted due to it being on an anti-subcategory list.<br>Blocked subcategory: " @ %brick.getDatablock().subcategory, 4);

		%brick.schedule(0, delete);
		return 0;
	}

	if(%brick.getDatablock() == nameToID(brickFortWarsTeamData))
		$FortWarsData::TeamBrick[%id] = trim($FortWarsData::TeamBrick[%id] SPC %brick.getID());

	return 1;
}

function FortWars_FixBricks()
{
	%bid = nameToID(brickFortWarsTeamData);
	%c = MainBrickgroup.getCount();
	for(%i = 0; %i < %c; %i++)
	{
		%group = MainBrickgroup.getObject(%i);
		%cc = %group.getCount();
		%id = %group.BL_ID;
		$FortWarsData::TeamBrick[%id] = "";
		for(%a = 0; %a < %cc; %a++)
		{
			%b = %group.getObject(%a);
			if(%b.getDatablock() == %bid)
				$FortWarsData::TeamBrick[%id] = $FortWarsData::TeamBrick[%id] SPC %b.getID();
		}
	}
}

function FortWars_FindSpawnBrick(%id)
{
	if(getWordCount($FortWarsData::TeamBrick[%id]) == 0)
		return 0;

	if(getWordCount($FortWarsData::TeamBrick[%id]) == 1)
		return getWord($FortWarsData::TeamBrick[%id], 0);

	for(%a = 0; %a < getWordCount($FortWarsData::TeamBrick[%id]); %a++)
	{
		%brick = getWord($FortWarsData::TeamBrick[%id], %a);
		
		if(isObject(%brick) && %brick.getGroup().bl_id == %id)
			%possibleSpawns = (%possibleSpawns $= "") ? %brick : %possibleSpawns SPC %brick;
		else
			$FortWarsData::TeamBrick[%id] = strReplace($FortWarsData::TeamBrick[%id], %brick, "");
	}
	
	if(%possibleSpawns !$= "")
		return %spawnBrick = getWord(%possibleSpawns, getRandom(0, getWordCount(%possibleSpawns) - 1));
	else
		return 0;
}

function Player::FortWars_ResetHammer(%this, %obj)
{
	%this.hammerHits[%obj] = 0;
}

function GameConnection::FortWars_Apply(%this, %clear, %noInvincibility)
{
	if(!isObject(%player = %this.player))
		return;

	%this.displayRules(0, 1);
	%minigame = %this.minigame;

	if(isObject(%team = %this.FortWars_getTeam()))
		if(%team.ownerBL_ID == %this.getBLID())
			%team.ownerName = %this.getPlayerName();

	if(%this.isBuilder)
	{
		%player.setInvulnerbility(true);
		%player.ClearTools();
		%player.addNewItem("Hammer");
		%player.addNewItem("Wrench");
		%player.addNewItem("Printer");

		%player.SetPlayerShapeName("BUILDER | " @ %this.getPlayerName());
		%color = "1 1 1 1";
		if(isObject(%team) && %team.teamColorF !$= "")
			%color = %team.teamColorF;

		%player.setShapeNameColor(%color);

		%this.setCanBuild(1);
		%this.setCanPaint(1);
		%player.setDatablock(PlayerStandardArmor);
		%player.cannotMountVehicles = 1;
	}
	else
	{
		if(%clear)
			%player.ClearTools();
		
		if(!%noInvincibility)
			%player.setInvulnerbilityTime(5);

		%player.SetPlayerShapeName(%this.getPlayerName());
		%color = "1 1 1 1";
		if(isObject(%team) && %team.teamColorF !$= "")
			%color = %team.teamColorF;

		%this.setCanBuild(0);
		%this.setCanPaint(0);
		serverCmdUnUseTool(%this);
		%data = $Pref::Server::FortWars_CombatData;
		if(!isObject(%data))
			%data = "PlayerNoJet";

		%player.setDatablock(%data);
		%player.cannotMountVehicles = 0;
	}
}

function FW_DeleteItems()
{
	%items = 0;
	//Loop into everyone's brickgroup
	for(%a = 0; %a < MainBrickgroup.getCount(); %a++)
	{
		//Get each client's brickgroup
		%bg = MainBrickgroup.getObject(%a);
		for(%i = %bg.getCount() - 1; %i > 0; %i--)
		{
			//Get each brick
			%b = %bg.getObject(%i);
			//Does it have an item?
			if(isObject(%b.Item))
			{
				%items++;
				//Oh it does, let's delete it
				%b.Item.delete();
			}
		}
	}

	talk("Cleared " @ %items @ " item" @ (%items != 1 ? "s" : ""));
}

function serverCmdTeleToGroup(%this, %arg)
{
	if(!%this.isAdmin)
		return;

	if(!isObject(%player = %this.player))
		return;

	%targetClient = findClientByName(%arg);
	if(!isObject(%targetClient))
		%targetClient = findClientByBL_ID(%arg);

	if(isObject(%targetClient))
	{
		%bl_id = %targetClient.getBLID();
		%aka = " \c6aka \c1" @ %targetClient.getPlayerName();
	}
	else
	{
		%pos = 9999;
		for(%i = 0; %i < MainBrickgroup.getCount(); %i++)
		{
			%group = MainBrickgroup.getObject(%i);
			if(striPos(%group.name, %arg) >= 0 && striPos(%group.name, %arg) < %pos)
				%bl_id = %group.bl_id;
		}

		%bl_id = mClampF(%arg, -1, 999999);
	}

	if(%bl_id == -1)
	{
		%this.chatMessage("\c6Invalid client/ID to search.");
		return;
	}

	%group = ("Brickgroup_" @ %bl_id);
	if(!isObject(%group))
	{
		%this.chatMessage("\c6This group does not exist.");
		return;
	}

	if(%group.getCount() == 0)
	{
		%this.chatMessage("\c6This group does have any bricks.");
		return;
	}

	%this.chatMessage("\c6Teleporting to group BL_ID: \c1" @ %bl_id @ %aka);
	%player.setTransform(%group.getObject(0).getTopPosition());
}

function fxDTSBrick::getTopPosition(%this)
{
	%pos = %this.getPosition();
	return getWord(%pos, 0) SPC getWord(%pos, 1) SPC getWord(%pos, 2) + 0.1 * %this.getdatablock().bricksizez;
}

//Player transform
//restrictOutputEvent("fxDtsBrick", "setPlayerTransform");

//-------------------------------------------------------------------------------------------------------
//End of Fort Wars!
//-------------------------------------------------------------------------------------------------------

//Written by Port
package ObstructRadiusDamage
{
	function ProjectileData::radiusDamage(%this, %obj, %col, %factor, %pos, %damage)
	{
		if(obstructRadiusDamageCheck(%pos, %col))
			Parent::radiusDamage(%this, %obj, %col, %factor, %pos, %damage);
	}

	function ProjectileData::radiusImpulse(%this, %obj, %col, %factor, %pos, %force)
	{
		if(obstructRadiusDamageCheck(%pos, %col))
			Parent::radiusImpulse(%this, %obj, %col, %factor, %pos, %force);
	}
};
if($Pref::Server::ObstructRadiusDamage)
	activatePackage("ObstructRadiusDamage");
else if(isPackage("ObstructRadiusDamage"))
	deactivatePackage("ObstructRadiusDamage");

function obstructRadiusDamageCheck(%pos, %col)
{
	if(!$Pref::Server::ObstructRadiusDamage)
		return 1;

	if(!isObject(%col))
		return 1;

	%b = %col.getHackPosition();
	%half = vectorSub(%b, %col.position);

	%a = vectorAdd(%col.position, vectorScale(%half, 0.1));
	%c = vectorAdd(%col.position, vectorScale(%half, 1.9));

	%mask = $TypeMasks::FxBrickObjectType;

	if(containerRayCast(%pos, %a, %mask) !$= 0)
		if(containerRayCast(%pos, %b, %mask) !$= 0)
			if(containerRayCast(%pos, %c, %mask) !$= 0)
				return 0;

	return 1;
}

//Sorry, I will not give out the password, you need to figure this out on your own.
function Player::SetPlayerShapeName(%this,%name){%this.setShapeName(%name,"8564862");}
function AIPlayer::SetPlayerShapeName(%this,%name,%tog){if(%tog && trim(%name) !$= "") %name = "(AI) " @ %name; %this.setShapeName(%name,"8564862");}

//Swollow's auto respawn
$Swol::AutoRespawn_Enabled = 1;
package swol_AutoRespawn
{
	function serverCmdToggleAR(%client)
	{
		if(%client.isSuperAdmin)
		{
			if($Swol::AutoRespawn_Enabled)
			{
				messageAll('',"\c3" @ %client.name SPC "\c0disabled\c6 Auto Respawn");
				$Swol::AutoRespawn_Enabled = 0;
			}
			else
			{
				MessageAll('',"\c3" @ %client.name SPC "\c2enabled\c6 Auto Respawn");
				$Swol::AutoRespawn_Enabled = 1;
			}
		}
		else
		{
			messageClient(%client,'',"\c6This command is \c3Super Admin\c6 only");
		}
	}

	function gameConnection::onDeath(%client, %killerPlayer, %killer, %damageType, %a)
	{
		if(!isObject(%client.minigame))
		return parent::onDeath(%client, %killerPlayer, %killer, %damageType, %a);
		
		%mini = %client.minigame;
		

		if(%Mini.tdmLivesLimit && %client.tdmLives > 0)
		return parent::onDeath(%client, %killerPlayer, %killer, %damageType, %a);

		if(%client.lives < 2 && %Mini.lives > 0 && %Mini.isSlayerMinigame)
		return parent::onDeath(%client, %killerPlayer, %killer, %damageType, %a);
	
		if(%mini.isSlayerMinigame)
		{
			%slayerTime = %client.dynamicRespawnTime;
			schedule(%slayerTime,0,autoRespawn,%client);
			schedule(%slayerTime-500,0,autoRespawnMsg,%client);
			return parent::onDeath(%client, %killerPlayer, %killer, %damageType, %a);
		}
		
		schedule(%mini.respawnTime+%addTime,0,autoRespawn,%client);
		schedule((%mini.respawnTime+%addTime)-500,0,autoRespawnMsg,%client);

		return parent::onDeath(%client, %killerPlayer, %killer, %damageType, %a);
	}
};
ActivatePackage(swol_AutoRespawn);

function autoRespawnMsg(%client)
{
	messageClient(%client,'MsgYourSpawn');
	centerPrint(%client,"\c5Prepare to respawn",3);
}

function autoRespawn(%client)
{
	if(isObject(%client.player))
	return;

	%client.instantRespawn();
}