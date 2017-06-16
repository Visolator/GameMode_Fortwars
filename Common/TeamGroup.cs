datablock fxDTSBrickData(brickFortWarsTeamData : brickSpawnPointData)
{
	specialBrickType = "";
	uiName = "FortWars TeamSpawn";
	isFortWarsBrick = true;
	isFortWarsTeamSpawn = true;
};

if($Pref::Server::FortWars_TeamFolder $= "")
	$Pref::Server::FortWars_TeamFolder = "config/server/FortWars/Teams/";

if(isPackage("FortWars_Team"))
	deactivatePackage("FortWars_Team");

package FortWars_Team
{
	function GameConnection::onClientEnterGame(%this)
	{
		if(isObject(%team = %this.FortWars_getTeam()))
			%team.lastLogin = $Sim::Time;

		return Parent::onClientEnterGame(%this);
	}

	function GameConnection::onClientLeaveGame(%this)
	{
		if(isObject(%team = %this.FortWars_getTeam()))
			%team.lastLogin = $Sim::Time;

		return Parent::onClientLeaveGame(%this);
	}

	function fxDTSBrick::onDeath(%brick)
	{
		if((%id = %brick.getGroup().bl_id) >= 0)
			if(%brick.getDatablock().isFortWarsTeamSpawn)
				$FortWarsData::TeamBrick[%id] = trim(strReplace($FortWarsData::TeamBrick[%id], nameToID(%brick), ""));

		parent::onDeath(%brick);
	}

	function fxDTSBrick::onRemove(%brick)
	{
		if((%id = %brick.getGroup().bl_id) >= 0)
			if(%brick.getDatablock().isFortWarsTeamSpawn)
				$FortWarsData::TeamBrick[%id] = trim(strReplace($FortWarsData::TeamBrick[%id], nameToID(%brick), ""));

		parent::onRemove(%brick);
	}

	function GameConnection::AutoAdminCheck(%this)
	{
		%this.schedule(100, "FortWars_CheckTeam");
		return Parent::AutoAdminCheck(%this);
	}

	function serverCmdTeamMessageSent(%client, %msg)
	{
		if(isObject(%team = %client.FortWars_getTeam()))
		{
			serverCmdStopTalking(%client);

			%msg = stripMLControlChars(trim(%msg));

			%length = strLen(%msg);
			if(!%length)
				return;

			if(%client.isSuperAdmin)
			{
				if(getSubStr(%msg, 0, 1) $= "$")
					%name = getSubStr(getWord(%msg, 0), 1, strLen(getWord(%msg, 0))-1);

				if(isObject(%targetClient = findClientByName(%name)))
					if(isObject(%newTeam = %targetClient.FortWars_getTeam()) && %team != %newTeam)
					{
						%someonesTeam = true;
						%team = %newTeam;
					}
			}

			%time = getSimTime();

			if(!%client.isSpamming)
			{
				//did they repeat the same message recently?
				if(%msg $= %client.lastMsg && %time - %client.lastMsgTime < $SPAM_PROTECTION_PERIOD)
				{
					messageClient(%client,'',"\c5Do not repeat yourself.");
					if(!%client.isAdmin)
					{

						%client.isSpamming = true;
						%client.spamProtectStart = %time;
						%client.schedule($SPAM_PENALTY_PERIOD,spamReset);
					}
				}

				//are they sending messages too quickly?
				if(!%client.isAdmin)
				{
					if(%client.spamMessageCount >= $SPAM_MESSAGE_THRESHOLD)
					{
						%client.isSpamming = true;
						%client.spamProtectStart = %time;
						%client.schedule($SPAM_PENALTY_PERIOD, spamReset);
					}
					else
					{
						%client.spamMessageCount ++;
						%client.schedule($SPAM_PROTECTION_PERIOD, spamMessageTimeout);
					}
				}
			}

			//tell them they're spamming and block the message
			if(%client.isSpamming)
			{
				spamAlert(%client);
				return;
			}

			//eTard Filter, which I hate, but have to include
			if($Pref::Server::eTardFilter)
			{
				%list = strReplace($Pref::Server::eTardList,",","\t");

				for(%i = 0; %i < getFieldCount(%list); %i ++)
				{
					%wrd = trim(getField(%list,%i));
					if(%wrd $= "")
						continue;
					if(striPos(" " @ %msg @ " "," " @ %wrd @ " ") >= 0)
					{
						messageClient(%client,'',"\c5This is a civilized game. Please use full words.");
						return;
					}
				}
			}

			//URLs
			for(%i = 0; %i < getWordCount(%msg); %i ++)
			{
				%word = getWord(%msg, %i);
				%pos = strPos(%word, "://") + 3;
				%pro = getSubStr(%word, 0, %pos);
				%url = getSubStr(%word, %pos, strLen(%word));

				if((%pro $= "http://" || %pro $= "https://") && strPos(%url, ":") == -1)
				{
					%word = "<sPush><a:" @ %url @ ">" @ %url @ "</a><sPop>";
					%msg = setWord(%msg, %i, %word);
				}
			}

			if(%all $= "")
			{
				%all  = '\c7[\c3%6\c7] \c7%1\c3%2\c7%3%7: %4';

				if(%team.ownerBL_ID == %client.getBLID())
				{
					if(isPackage("Server_StaffShields"))
						%all = '\c7[\c3%6\c7] <bitmap:add-ons/Server_StaffShields/icon_goldBadge.png> \c7%1\c3%2\c7%3%7: %4';
					else
						%all = '\c7[\c3%6\c7] \c3HOST \c7%1\c3%2\c7%3%7: %4';
				}
				else
				{
					if(hasItemOnList(%team.autoSuperAdminList, %client.getBLID()))
						%all = '\c7[\c3%6\c7] <bitmap:add-ons/Server_StaffShields/icon_goldBadge.png> \c7%1\c3%2\c7%3%7: %4';
					else if(hasItemOnList(%team.autoAdminList, %client.getBLID()))
						%all = '\c7[\c3%6\c7] <bitmap:add-ons/Server_StaffShields/icon_silverBadge.png> \c7%1\c3%2\c7%3%7: %4';
				}
			}

			%pre  = %client.clanPrefix;
			%suf  = %client.clanSuffix;

			%team.FortWars_commandToAll('chatMessage', %client, '', '', %all, %pre, %client.getPlayerName(), %suf, %msg, "\c6", %team.uiName, "\c4");

			for(%i = 0; %i < ClientGroup.getCount(); %i++)
			{
				%cl = ClientGroup.getObject(%i);
				if((%cl.FortWarsData["NSA"] || %someonesTeam) && %team != %cl.FortWars_getTeam())
					commandToClient(%cl, 'chatMessage', %client, '', '', %all, %pre, %client.getPlayerName(), %suf, %msg, "\c6", %team.uiName, "\c4");
			}

			echo("[" @ %team.uiName @ "] " @ %client.getSimpleName() @ ": " @ %msg);

			%client.lastMsg = %msg;
			%client.lastMsgTime = %time;

			if(isObject(%client.player))
			{
				%client.player.playThread(3,"talk");
				%client.player.schedule(%length * 50,playThread,3,"root");
			}
			return;
		}
		Parent::serverCmdTeamMessageSent(%client, %msg);
	}
};
activatePackage("FortWars_Team");

function GameConnection::FortWars_CheckTeam(%this, %silent)
{
	if($FortWars::Team[%this.getBLID()])
		%t = %this.FortWars_JoinTeam($FortWars::Team[%this.getBLID()], 0, 1);
	
	if(!isObject(%t))
		%t = %this.FortWars_JoinTeam(FortWars_TeamGroup.joinTeamByAllow(%this.getBLID()), 0, 1);

	if(!isObject(%t))
		%t = %this.FortWars_JoinTeam(FortWars_TeamGroup.findExactTeamByBLID(%this.getBLID()), 0, 1);

	%this.schedule(0, "FortWars_AutoAdminCheck", "", %silent);
}

function GameConnection::FortWars_RenameTeam(%this, %name)
{
	//If you're trying to spam go screw yourself
	if(!%this.hasSpawnedOnce)
		return;

	%name = trim(stripMLControlChars(%name));

	if(%name $= "")
	{
		%this.chatMessage("\c6Please put a name.");
		return;
	}

	%id = %this.getBLID();
	%clName = %this.getPlayerName();
	%team = %this.FortWars_getTeam();
	if(!isObject(%team))
	{
		%this.chatMessage("\c6You don't have a team!");
		return;
	}

	if(%team.ownerBL_ID != %id)
	{
		%this.chatMessage("\c6You don't own this team!");
		return;
	}

	if(isObject(FortWars_TeamGroup.findExactTeam(%name)))
	{
		%this.chatMessage("\c6That team already exists!");
		return;
	}

	if($Sim::Time - %this.lastTeamInfo < 30)
	{
		%time = mFloatLength(30 - ($Sim::Time - %this.lastTeamInfo), 1);
		%this.chatMessage("\c6Sorry, you cannot modify your team for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : ""));
		return;
	}

	%team.uiName = %name;
	messageAll('', "\c3" @ %this.getPlayerName() @ " \c6has renamed their team to \c3" @ %name @ "\c6. (/FortWars ListTeams)");
}

function GameConnection::FortWars_CreateTeam(%this, %name)
{
	//If you're trying to spam go screw yourself
	if(!%this.hasSpawnedOnce)
		return;

	%name = trim(stripMLControlChars(%name));

	if(%name $= "")
	{
		%this.chatMessage("\c6Please put a team name.");
		return;
	}

	%id = %this.getBLID();
	%clName = %this.getPlayerName();
	%team = %this.FortWars_getTeam();
	if(isObject(%team))
	{
		%this.chatMessage("\c6Please leave your team before creating another one.");
		return;
	}

	if(isObject(FortWars_TeamGroup.findExactTeam(%name)))
	{
		%this.chatMessage("\c6That team already exists!");
		return;
	}

	if($Sim::Time - %this.lastTeamInfo < 30)
	{
		%time = mFloatLength(30 - ($Sim::Time - %this.lastTeamInfo), 1);
		%this.chatMessage("\c6Sorry, you cannot create a team for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : ""));
		return;
	}

	%this.lastTeamInfo = $Sim::Time;

	%newTeam = new SimSet("FortWars_" @ getSafeVariableName(%name))
	{
		ownerBL_ID = %id;
		ownerName = %this.getPlayerName();
		uiName = %name;
		isFortWars = true;
		allowBLID = %id;
		lastLogin = $Sim::Time;
		FortWarsPrefSave = true;
	};
	%newTeam.FortWars_AssignColor();
	FortWars_TeamGroup.add(%newTeam);

	messageAll('', "\c3" @ %this.getPlayerName() @ " \c6has created their own team. (\c3" @ %name @ "\c6) (/FortWars ListTeams)");
	%this.FortWars_JoinTeam(%newTeam);
	%newTeam.schedule(33, "FortWars_messageAll", '', "Team created!");
}

function GameConnection::FortWars_JoinTeam(%this, %name, %isNew, %bypass)
{
	if(!isObject(%name))
	{
		if(!isObject(%group = FortWars_TeamGroup.findExactTeamByBLID(%name)))
			if(!isObject(%group = FortWars_TeamGroup.findTeam(%name)))
				return 0;
	}
	else
	{
		%group = nameToID(%name);

		if(!%group.isFortWars)
			return 0;
	}

	%team = %this.FortWars_getTeam();
	if(%team == %group)
		return %group;
	
	if(isObject(%team) && !%bypass)
	{
		%this.chatMessage("\c6Please leave your current team before joining another team.");
		return 0;
	}

	if(%this.lastTeamJoinInfo !$= "" && %this.lastTeamJoinInfo > 0 && $Sim::Time - %this.lastTeamJoinInfo < 20 && !%bypass)
	{
		%time = mFloatLength(20 - ($Sim::Time - %this.lastTeamJoinInfo), 1);
		%this.chatMessage("\c6Sorry, you cannot join " @ (isObject(%team) ? "another" : "a") @ " team yet for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : ""));
		return 0;
	}

	%id = %this.getBLID();
	%clName = %this.getPlayerName();

	if(!%group.FortWars_isAllowed(%id) && !%bypass)
	{
		%this.chatMessage("\c6Sorry, you cannot join this team. please ask the guy who owns this team (\"ID: " @ %group.ownerBL_ID @ "\") \c6to do \c1/FortWars Allow " @ %id);
		return 0;
	}

	%this.lastTeamJoinInfo = $Sim::Time;
	$FortWars::Team[%id] = %group.ownerBL_ID;
	export("$FortWars::Team*", "config/server/FortWars/TeamConfig.cs");

	if(%group.ownerBL_ID == %id)
		%group.FortWars_messageAll('', "\c6The owner is now \c2online\c6. (\c3" @ %clName @ "\c6)");
	else
	{
		if(%isNew)
		{

			if(isObject(%player = %this.player))
			{
				%color = "1 1 1 1";
				if(%team.teamColorF !$= "")
					%color = %team.teamColorF;

				%player.setShapeNameColor(%color);
			}

			%group.FortWars_messageAll('', "\c3" @ %clName @ " \c6has joined the team.");
		}
		else
			%group.FortWars_messageAll('', "\c3" @ %clName @ " \c6is now \c2online\c6.");
	}

	%this.namecolor = %group.teamColorHex;
	%group.add(%this);
	messageClient(%this, '', "\c7[\c6Fort Wars | \c3" @ %group.uiName @ "\c7] \c5Welcome, " @ %clName @ "!");
	return %group;
}

if(isFile("config/server/FortWars/TeamConfig.cs"))
	exec("config/server/FortWars/TeamConfig.cs");

function GameConnection::FortWars_LeaveTeam(%this)
{
	%team = %this.FortWars_getTeam();
	if(!isObject(%team))
	{
		%this.chatMessage("You're not on a team!");
		return;
	}

	%id = %this.getBLID();
	%clName = %this.getPlayerName();

	if(%team.ownerBL_ID == %id)
	{
		%team.FortWars_messageAll('', "\c6The owner has abandoned the team. (\c3" @ %clName @ "\c6)");
		%team.FortWars_Delete();
	}
	else
	{
		%team.remove(%this);
		$FortWars::Team[%id] = "";
		export("$FortWars::Team*", "config/server/FortWars/TeamConfig.cs");
		%team.FortWars_removeID(%id);
		%team.FortWars_messageAll('', "\c3" @ %clName @ " \c6has left the team. - They are now disallowed to get back into the team (You have to toggle again)");
		%this.chatMessage("\c6You have left the team \c3" @ %team.uiName @ "\c6.");
		%this.instantRespawn();
		if(isObject(%player = %this.player))
			%player.setShapeNameColor("1 1 1 1");

		%this.namecolor = "";
	}
}

function GameConnection::FortWars_getTeam(%this)
{
	%id = %this.getBLID();
	if(isObject(%team = FortWars_TeamGroup.findExactTeamByBLID($FortWars::Team[%id])))
	{
		if(%team.isMember(%this))
			return %team;
	}

	if(isObject(%team = FortWars_TeamGroup.findExactTeam($FortWars::Team[%id])))
	{
		if(%team.isMember(%this))
			return %team;
	}

	return 0;
}

function SimSet::FortWars_addID(%this, %id)
{
	if(hasItemOnList(%this.allowBLID, %id))
		return false;

	%this.allowBLID = addItemToList(%this.allowBLID, %id);

	return true;
}

function SimSet::FortWars_removeID(%this, %id)
{
	if(!hasItemOnList(%this.allowBLID, %id))
		return false;

	%this.allowBLID = removeItemFromList(%this.allowBLID, %id);

	return true;
}

function SimSet::FortWars_isAllowed(%this, %id)
{
	if(%id < 0)
		return false;

	if(%this.ownerBL_ID == %id)
		return true;

	if(!hasItemOnList(%this.allowBLID, %id))
		return false;

	return true;
}

function GameConnection::FortWars_CanSetPrefs(%this)
{
	if(!isObject(%team = %this.FortWars_getTeam()))
		return false;

	if(%this.getBLID() == %team.ownerBL_ID)
		return true;

	if(!hasItemOnList(%team.autoSuperAdminList, %this.getBLID()))
		return false;

	return true;
}

function GameConnection::FortWars_CanInvite(%this)
{
	if(!isObject(%team = %this.FortWars_getTeam()))
		return false;

	if(%this.getBLID() == %team.ownerBL_ID)
		return true;

	if(!hasItemOnList(%team.autoAdminList, %this.getBLID()))
		if(!hasItemOnList(%team.autoSuperAdminList, %this.getBLID()))
			return false;

	return true;
}

function GameConnection::FortWars_CanKick(%this)
{
	if(!isObject(%team = %this.FortWars_getTeam()))
		return false;

	if(%this.getBLID() == %team.ownerBL_ID)
		return true;

	if(!hasItemOnList(%team.autoSuperAdminList, %this.getBLID()))
		return false;

	return true;
}

function SimSet::joinTeamByAllow(%this, %id)
{
	if(%this.getCount() > 0)
		for(%a = 0; %a < %this.getCount(); %a++)
		{
			%objA = %this.getObject(%a);
			if((hasItemOnList(%objA.autoAdminList, %id) || hasItemOnList(%objA.autoSuperAdminList, %id)) && %objA.FortWars_isAllowed(%id))
				return %objA;

			if(%objA.ownerBL_ID == %id)
				return %objA;
		}

	return 0;
}

function GameConnection::FortWars_AutoAdminCheck(%this, %isNew, %silent)
{
	if(!isObject(%team = %this.FortWars_getTeam()))
		return -1;

	%id = %this.getBLID();

	if(getWordCount(%team.autoSuperAdminList) > 0)
		for(%i = 0; %i < getWordCount(%team.autoSuperAdminList); %i++)
		{
			%word = getWord(%team.autoSuperAdminList, %i);
			if(%word == %id)
				%isSuperAdmin = 1;
		}

	if(getWordCount(%team.autoAdminList) > 0)
		for(%i = 0; %i < getWordCount(%team.autoAdminList); %i++)
		{
			%word = getWord(%team.autoAdminList, %i);
			if(%word == %id)
				%isAdmin = 1;
		}

	if(%id == %team.ownerBL_ID)
		%isHost = 1;

	if(%isAdmin && !%isSuperAdmin)
	{
		if(!%silent)
			%team.FortWars_messageAll('MsgUploadEnd', "\c2" @ %this.getPlayerName() @ " has become Team Admin " @ (%isNew ? "(Manual)" : "(Auto)"));
	}
	else if(!%isAdmin && %isSuperAdmin)
	{
		if(!%silent)
			%team.FortWars_messageAll('MsgUploadEnd', "\c2" @ %this.getPlayerName() @ " has become Team Super Admin " @ (%isNew ? "(Manual)" : "(Auto)"));
	}
	else if(%isAdmin && %isSuperAdmin)
	{
		if(hasItemOnList(%team.autoAdminList, %id))
			%team.autoAdminList = removeItemFromList(%team.autoAdminList, %id);

		if(!%silent)
			%team.FortWars_messageAll('MsgUploadEnd', "\c2" @ %this.getPlayerName() @ " has become Team Super Admin " @ (%isNew ? "(Manual)" : "(Auto)"));
	}
	else if(%isHost)
	{
		if(!%silent)
			%team.FortWars_messageAll('MsgUploadEnd', "\c2" @ %this.getPlayerName() @ " has become Team Super Admin (Team Owner)");
	}
	else
		return 0;

	return 1;
}

if(!isObject(FortWars_TeamGroup))
{
	new SimSet(FortWars_TeamGroup)
	{
		isFortWars = true;
	};
}

FortWars_TeamGroup.schedule(0, "FortWars_LoadAll");

function SimSet::FortWars_Delete(%this)
{
	if(!%this.isFortWars)
		return;

	if(%this.getCount() > 0)
	{
		for(%i = 0; %i < %this.getCount(); %i++)
		{
			%client = %this.getObject(%i);
			if(%client.getClassName() $= "GameConnection")
			{
				%client.namecolor = "";
				if(isObject(%player = %client.player))
					%player.setShapeNameColor("1 1 1 1");
				
				$FortWars::Team[%client] = "";
			}
		}
	}

	if(isFile(%file = $Pref::Server::FortWars_TeamFolder @ getSafeVariableName(%this.uiName) @ "-" @ %this.ownerBL_ID @ ".cs"))
		fileDelete(%file);

	%this.delete();
}

function SimSet::FortWars_AssignColor(%this, %col)
{
	if(!%this.isFortWars)
		return 0;

	%cc = FortWars_TeamGroup.getCount();
	for(%i = 0; %i < %cc; %i++)
	{
		%obj = FortWars_TeamGroup.getObject(%i);
		%teamcol[%i] = %obj.teamColor;
	}

	if(strLen(%col) == 6)
	{
		%newTeamColor = getColorI(hexToRGB(%col) @ " 1");
		%hasCol = 1;

		//Grab a new color
		for(%i = 0; %i < %cc; %i++)
		{
			//Grab a new color
			if(%teamcol[%i] !$= "" && vectorDist(%newTeamColor, %teamcol[%i]) < 14)
				return 2;
		}

	}
	else if(%col !$= "")
		return 0;

	if(!%hasCol)
	{
		%tries = 0;
		while(%tries < 50)
		{
			%tries++;
			%continue = 0;

			%newTeamColor = getRandom(0, 255) SPC getRandom(0, 255) SPC getRandom(0, 255) SPC 255;
			for(%i = 0; %i < %cc; %i++)
			{
				//Grab a new color
				if(%teamcol[%i] !$= "" && vectorDist(%newTeamColor, %teamcol[%i]) < 14)
				{
					%continue = 1;
					%i = %cc;
				}
			}

			if(!%continue)
				break;
		}
	}

	%this.teamColor = %newTeamColor;
	%this.teamColorF = getColorF(%newTeamColor);
	%this.teamColorHex = rgbtohex(getWords(%this.teamColorF, 0, 2));
	FortWars_TeamGroup.FortWars_SaveAll();

	if(%this.getCount() == 0) //???
		return 1;

	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%client = %this.getObject(%i);
		if(%client.getClassName() $= "GameConnection" && isObject(%player = %client.player))
		{
			%client.namecolor = %this.teamColorHex;
			%player.setShapeNameColor(%this.teamColorF);
		}
	}

	return 1;
}

function SimSet::FortWars_commandToAll(%this, %type, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n)
{
	%g = nameToID("FortWars_TeamGroup");
	if(!%this.isFortWars || !%g.isMember(%this))
		return;

	if(%this.getCount() == 0) //???
		return;

	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%client = %this.getObject(%i);
		if(%client.getClassName() $= "GameConnection")
			commandToClient(%client, %type, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n);
	}
}

function SimSet::FortWars_messageAll(%this, %type, %message, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n)
{
	if(!%this.isFortWars)
		return;

	if(%this.getCount() == 0) //???
		return;

	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%client = %this.getObject(%i);
		if(%client.getClassName() $= "GameConnection")
			messageClient(%client, %type, "\c7[\c3" @ %this.uiName @ "\c7] \cr" @ %message, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n);
	}

	for(%i = 0; %i < ClientGroup.getCount(); %i++)
	{
		%cl = ClientGroup.getObject(%i);
		if(%cl.FortWarsData["NSA"] && %this != %cl.FortWars_getTeam())
			messageClient(%cl, %type, "\c4Listening \c7> \c7[\c3" @ %this.uiName @ "\c7] \cr" @ %message, %a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n);
	}
}

function SimSet::FortWars_setPref(%this, %type, %value, %client)
{
	switch$(%type)
	{
		case "Save":
			%variable = "permanent team";
			%variableName = "Save";
			%value = mFloor(%value);
			%valueName = (%value == 1 ? "true" : "false");

		case "FF" or "friendlyfire" or "fire" or "dmg":
			%variable = "friendly fire";
			%variableName = "FriendlyFire";
			%value = mFloor(%value);
			%valueName = (%value == 1 ? "true" : "false");

		default:
			return;
	}

	if(%this.FortWarsPref[%variableName] $= %value)
		return;

	if(isObject(%client))
		%nameMsg = %client.getPlayerName();
	else
		%nameMsg = "CONSOLE";

	%this.FortWarsPref[%variableName] = %value;
	%this.FortWars_messageAll('MsgUploadEnd', "\c2Preference \c7-> \c3" @ %nameMsg @ " \c6has set \c3" @ %variable @ " \c6to \c3" @ %valueName);
	FortWars_TeamGroup.FortWars_SaveAll();
}

function SimSet::findExactTeamByBLID(%this, %name)
{
	if(%this.getCount() > 0)
	{
		%result["string"] = 0;
		%result["id"] = 0; //If this is found we are definitely giving it
		%result["string", "pos"] = 9999;
		for(%a = 0; %a < %this.getCount(); %a++)
		{
			%objA = %this.getObject(%a);
			if(%objA.ownerBL_ID $= %name)
			{
				%result["id"] = 1;
				%result["id", "item"] = %objA;
			}
		}

		if(%result["id"] && isObject(%result["id", "item"])) //This should most likely say yes
			return nameToID(%result["id", "item"]);
	}

	return 0;
}

function SimSet::findExactTeam(%this, %name)
{
	if(%this.getCount() > 0)
	{
		%result["string"] = 0;
		%result["id"] = 0; //If this is found we are definitely giving it
		%result["string", "pos"] = 9999;
		for(%a = 0; %a < %this.getCount(); %a++)
		{
			%objA = %this.getObject(%a);
			if(%objA.uiName $= %name || %objA.getName() $= %name)
			{
				%result["id"] = 1;
				%result["id", "item"] = %objA;
			}
		}

		if(%result["id"] && isObject(%result["id", "item"])) //This should most likely say yes
			return nameToID(%result["id", "item"]);
	}

	return 0;
}

function SimSet::findTeam(%this, %name)
{
	if(%this.getCount() > 0)
	{
		%result["string"] = 0;
		%result["id"] = 0; //If this is found we are definitely giving it
		%result["string", "pos"] = 9999;
		for(%a = 0; %a < %this.getCount(); %a++)
		{
			%objA = %this.getObject(%i);
			if(%objA.uiName $= %name || %objA.getName() $= %name)
			{
				%result["id"] = 1;
				%result["id", "item"] = %objA;
			}
			else
			{
				%pos = striPos(%objA.uiName, %name);
				if(striPos(%objA.uiName, %name) >= 0 && %pos < %result["string", "pos"])
				{
					%result["string"] = 1;
					%result["string", "item"] = %objA;
					%result["string", "pos"] = %pos;
				}						
			}
		}

		if(%result["id"] && isObject(%result["id", "item"])) //This should most likely say yes
			return nameToID(%result["id", "item"]);

		if(%result["string"] && isObject(%result["string", "item"]))
			return nameToID(%result["string", "item"]);
	}

	return 0;
}

function SimSet::FortWars_LoadAll(%this)
{
	%path = $Pref::Server::FortWars_TeamFolder @ "*.txt";
	if(getFileCount(%path) <= 0)
		return -1;

	echo("========== Initiating FortWars Teams ==========");
	for(%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path))
	{
		%io = new FileObject();
		%io.openForRead(%file);
		while(!%io.isEOF())
		{
			%line = %io.readLine();
			%var[getField(%line, 0)] = getField(%line, 1);
		}
		%io.close();
		%io.delete();

		if(%var["uiName"] !$= "" && %var["ownerBL_ID"] !$= "")
		{
			if(!isObject("FortWars_" @ getSafeVariableName(%var["uiName"])))
			{
				echo(" + Loaded team: " @ %var["uiName"]);
				echo("    Owner: " @ %var["ownerName"] @ " (" @ %var["ownerBL_ID"] @ ")");
				echo("    AdminList count: " @ getWordCount(%var["autoAdminList"]));
				echo("    SuperAdminList count: " @ getWordCount(%var["autoSuperAdminList"]));
				echo("    AllowID count: " @ getWordCount(%var["allowBLID"]));

				%newTeam = new SimSet("FortWars_" @ getSafeVariableName(%var["uiName"]))
				{
					uiName = %var["uiName"];
					ownerBL_ID = %var["ownerBL_ID"];
					ownerName = %var["ownerName"];
					autoAdminList = %var["autoAdminList"];
					autoSuperAdminList = %var["autoSuperAdminList"];
					allowBLID = %var["allowBLID"];
					teamColor = %var["teamColor"];
					teamColorF = %var["teamColorF"];
					teamColorHex = %var["teamColorHex"];
					FortWarsPrefSave = true;
					isFortWars = true;
				};
				if(%var["teamColor"] $= "")
					%newTeam.FortWars_AssignColor();
				
				%this.add(%newTeam);
			}
		}
	}

	if(ClientGroup.getCount() > 0)
		for(%i = 0; %i < ClientGroup.getCount(); %i++)
		{
			%cl = ClientGroup.getObject(%i);
			%cl.FortWars_CheckTeam(1);
		}

	echo("========== FortWars Team complete ==========");

	return 1;
}

function SimSet::FortWars_SaveAll(%this)
{
	%file = $Pref::Server::FortWars_TeamFolder;
	for(%i = 0; %i < %this.getCount(); %i++)
	{
		%team = %this.getObject(%i);
		%file = $Pref::Server::FortWars_TeamFolder @ getSafeVariableName(%team.uiName) @ "-" @ %team.ownerBL_ID @ ".txt";
		if(%team.FortWarsPref["Save"])
		{
			%teamCount++;
			%io = new FileObject();
			%io.openForWrite(%file);
			%io.writeLine("ownerBL_ID" TAB %team.ownerBL_ID);
			%io.writeLine("ownerName" TAB %team.ownerName);
			%io.writeLine("uiName" TAB %team.uiName);
			if(%team.autoAdminList !$= "")
				%io.writeLine("autoAdminList" TAB %team.autoAdminList);
			if(%team.autoSuperAdminList !$= "")
				%io.writeLine("autoSuperAdminList" TAB %team.autoSuperAdminList);
			if(%team.allowBLID !$= "")
				%io.writeLine("allowBLID" TAB %team.allowBLID);
			if(%team.teamColor !$= "")
				%io.writeLine("teamColor" TAB %team.teamColor);
			if(%team.teamColorF !$= "")
				%io.writeLine("teamColorF" TAB %team.teamColorF);
			if(%team.teamColorHex !$= "")
				%io.writeLine("teamColorHex" TAB %team.teamColorHex);
			%io.close();
			%io.delete();
		}
		else if(isFile(%file))
			fileDelete(%file);
	}

	//messageAll('', "\c6Saved \c3" @ %teamCount @ " team" @ (%teamCount != 1 ? "s" : ""));
}

function FortWars_CheckTeams()
{
	cancel($FortWars_CheckTeamSch);
	if(!isObject(FortWars_TeamGroup))
		return;

	%day = 60 * 60 * 24;
	%week = %day * 7;

	if(FortWars_TeamGroup.getCount() > 0)
	{
		for(%i = 0; %i < FortWars_TeamGroup.getCount(); %i++)
		{
			%team = FortWars_TeamGroup.getObject(%i);
			if(%team.FortWarsPref["Save"]) //Longer time, just 1 more day (3 days)
			{
				if(%team.lastLogin !$= "")
				{
					if(%team.getCount() > 0)
						%team.lastLogin = $Sim::Time;

					else if(mFloor($Sim::Time - %team.lastLogin) > %week)
					{
						messageAll('MsgUploadEnd', "\c6Team \c3" @ %team.uiName @ " \c7(\c6Owner BL_ID: \c1" @ %team.ownerBL_ID @ "\c7) \c6has been deleted due to team inactivity. (Last login, 1 week ago)");
						%team.schedule(0, "FortWars_Delete");
					}
				}
				else
				{
					messageAll('MsgUploadEnd', "\c6Team \c3" @ %team.uiName @ " \c7(\c6Owner BL_ID: \c1" @ %team.ownerBL_ID @ "\c7) \c6has an invalid last login time, resetting.");
					%team.lastLogin = $Sim::Time;
				}
			}
			else //Otherwise they have 2 days
			{
				if(%team.lastLogin !$= "")
				{
					if(%team.getCount() > 0)
						%team.lastLogin = $Sim::Time;
					else if(mFloor($Sim::Time - %team.lastLogin) > %day)
					{
						messageAll('MsgUploadEnd', "\c6Team \c3" @ %team.uiName @ " \c7(\c6Owner BL_ID: \c1" @ %team.ownerBL_ID @ "\c7) \c6has been deleted due to team inactivity. (Last login, 1 day ago)");
						%team.schedule(0, "FortWars_Delete");
					}
				}
				else
				{
					messageAll('MsgUploadEnd', "\c6Team \c3" @ %team.uiName @ " \c7(\c6Owner BL_ID: \c1" @ %team.ownerBL_ID @ "\c7) \c6has an invalid last login time, resetting.");
					%team.lastLogin = $Sim::Time;
				}
			}
		}

		FortWars_TeamGroup.FortWars_SaveAll();
	}

	$FortWars_CheckTeamSch = schedule(10000, 0, "FortWars_CheckTeams");
}

if(!isEventPending($FortWars_CheckTeamSch))
	$FortWars_CheckTeamSch = schedule(0, 0, "FortWars_CheckTeams");