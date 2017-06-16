//Basically the same thing as /FortWars Build, just without the warn
function serverCmdAcceptFortWarsCombat(%this)
{
	if($Sim::Time - %this.lastBuilderSwitch < 15)
	{
		%time = mFloatLength(15 - ($Sim::Time - %this.lastBuilderSwitch), 1);
		%this.chatMessage("\c6Sorry, you can't toggle being a builder yet. Please wait \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : "") @ ".");
		return;
	}

	%this.isBuilder = !%this.isBuilder;
	%this.chatMessage("\c6Builder is now " @ (%this.isBuilder ? "\c2ON. \c0Damaging anyone \c6will turn this back off." : "\c0OFF\c6. You can now be damaged after 5 seconds, unless you damage someone before that."));
	%this.FortWars_Apply(true);
	%this.lastBuilderSwitch = $Sim::Time;
	%this.FortWarsData["SpawnInit"] = 1;
}

function getStringTime(%time)
{
	return "WIP";
}

function serverCmdRules(%this)
{
	%this.displayRules();
}

function GameConnection::displayRules(%this, %num, %ignore)
{
	cancel(%this.ruleSch);
	if($server::rulesid $= "")
		$server::rulesid = getRandom(1, 9999);

	if((%num == 0 || %num $= "") && !%ignore)
	{
		%this.chatMessage("\c6Fort Wars - Rules");
		%this.chatMessage("\c31. \c6Have some common sense");
		%this.chatMessage("   \c3- \c6Be respectful, and do not find loopholes in these rules");
		%this.chatMessage("\c32. \c6Do not abuse events");
		%this.chatMessage("\c33. \c6No griefing | No build/event trolling/spam");
		%this.chatMessage("   \c3- \c6Do not create events that will disturb a nearby base | Do not plant bots near an enemy base | NO DUPLICATOR SPAM");
	}

	if($server::hasseenrules[%this.getBLID(), $server::rulesid])
		return;

	if(%num < 5)
	{
		%str = "\n<font:impact:25>" @ (5 - %num);
	}
	else
	{
		$server::hasseenrules[%this.getBLID(), $server::rulesid] = 1;
		%str = "\nSee /Rules for more info.";
	}

	%rule0 = "Have some common sense";
	%rule1 = "Do not abuse events";
	%rule2 = "No griefing | No build/event trolling/spam";

	for(%i = 0; %i <= 2; %i++)
	{
		if(%ruleStr $= "")
			%ruleStr = (%i+1) @ ". " @ %rule[%i];
		else
			%ruleStr = %ruleStr NL (%i+1) @ ". " @ %rule[%i];
	}

	commandToClient(%this, 'MessageBoxOK', "Fort Wars - Rules", %ruleStr @ %str);
	if(%num < 5)
		%this.ruleSch = %this.schedule(1000, "displayRules", %num++, %ignore);
}

function serverCmdFT(%this, %command, %arg0, %arg1, %arg2, %arg3, %arg4){serverCmdFortWars(%this, %command, %arg0, %arg1, %arg2, %arg3, %arg4);}
function serverCmdFW(%this, %command, %arg0, %arg1, %arg2, %arg3, %arg4){serverCmdFortWars(%this, %command, %arg0, %arg1, %arg2, %arg3, %arg4);}

function serverCmdFortWars(%this, %command, %arg0, %arg1, %arg2, %arg3, %arg4)
{
	if(!%this.hasSpawnedOnce)
	{
		%this.chatMessage("Nice try, please spawn first.");
		return;
	}

	for(%i = 0; %i < 4; %i++)
		%arg = %arg SPC %arg[%i];

	%arg = trim(%arg);

	switch$(%command)
	{
		case "Build" or "Attack":
			%this.FW_Build();

		case "Kick":
			if(!isObject(%team = %this.FortWars_getTeam())) //No team?
				return;

			if(!%this.FortWars_CanKick())
			{
				%this.chatMessage("\c6You are not allowed to kick anyone.");
				return;
			}

			%kickClient = findClientByName(%arg);
			if(!isObject(%kickClient))
			{
				%this.chatMessage("\c6Invalid client to kick!");
				return;
			}

			if(%kickClient == %this)
			{
				%this.chatMessage("\c6Don't be dramatic, leave the team yourself.");
				return;
			}

			if(%kickClient.getBLID() == %team.ownerBL_ID)
			{
				%this.chatMessage("\c6You are not allowed to kick the owner.");
				return;
			}

			%team.FortWars_MessageAll('', "\c3" @ %kickClient.getPlayerName() @ " \c6has been kicked out of the party. (By: " @ %this.getPlayerName() @ ")");
			%kickClient.FortWars_LeaveTeam();
			commandToClient(%kickClient, 'MessageBoxOK', "Uh oh!", "You have been kicked out of your team.<br>Sorry about that!");

		case "ListTeams" or "ListTeam" or "TeamList":
			if($Sim::Time - %this.lastTeamListRequest < 3)
			{
				%time = mFloatLength(3 - ($Sim::Time - %this.lastTeamListRequest), 1);
				return;
			}
			%id = %this.getBLID();

			%this.lastTeamListRequest = $Sim::Time;

			%curTeam = %this.FortWars_getTeam();

			switch$(%arg0)
			{
				case "online":
					%this.chatMessage("\c6------ TEAMS ONLINE ------");
					%onlineTeams = 0;
					for(%i = 0; %i < FortWars_TeamGroup.getCount(); %i++)
					{
						%team = FortWars_TeamGroup.getObject(%i);

						%owner = (isObject(findClientByBL_ID(%team.ownerBL_ID)) ? "\c2online" : "\c7offline");
						%allowed = (%team.FortWars_isAllowed(%id) ? "\c4Allowed to join" : "\c0Not allowed to join (Request permission)");
						if(%team.ownerBL_ID == %this.getBLID())
							%allowed = "\c3You own this team";
						else if(%team == %curTeam)
							%allowed = "\c3You are on this team";

						%onlineCount = %team.getCount();

						if(%onlineCount > 0)
						{
							%online = "(Online: \c3" @ %onlineCount @ "\c6)";
							%onlineTeams++;
							%this.schedule(10 * %i, chatMessage, "  \c6(Owner is " @ %owner @ "\c6) " @ %online @" \c3" @ %team.uiName @ " \c7(\c4" @ %team.ownerName @ " \c6: \c4" @ %team.ownerBL_ID @ "\c7) " @ %allowed);
						}
					}
					%this.schedule(10 * %i, chatMessage, "\c6------ Page up (PGUP/PGDN) \c7| \c3/FortWars Join \c2name/ID \c7| \c6Total teams online: \c3" @ %onlineTeams @ " \c6------");

				case "offline":
					%this.chatMessage("\c6------ TEAMS OFFLINE ------");
					%onlineTeams = 0;
					for(%i = 0; %i < FortWars_TeamGroup.getCount(); %i++)
					{
						%team = FortWars_TeamGroup.getObject(%i);

						%owner = (isObject(findClientByBL_ID(%team.ownerBL_ID)) ? "\c2online" : "\c7offline");
						%allowed = (%team.FortWars_isAllowed(%id) ? "\c4Allowed to join" : "\c0Not allowed to join (Request permission)");
						if(%team.ownerBL_ID == %this.getBLID())
							%allowed = "\c3You own this team";
						else if(%team == %curTeam)
							%allowed = "\c3You are on this team";

						%onlineCount = %team.getCount();
						if(%onlineCount == 0)
						{
							%online = "(Last online: \c3" @ getStringTime(getTimeString(($Sim::Time - %team.lastLogin) / 60)) @ " ago\c6)";
							%onlineTeams++;
							%this.schedule(10 * %i, chatMessage, "  \c6(Owner is " @ %owner @ "\c6) " @ %online @" \c3" @ %team.uiName @ " \c7(\c4" @ %team.ownerName @ " \c6: \c4" @ %team.ownerBL_ID @ "\c7) " @ %allowed);
						}
					}
					%this.schedule(10 * %i, chatMessage, "\c6------ Page up (PGUP/PGDN) \c7| \c3/FortWars Join \c2name/ID \c7| \c6Total teams offline: \c3" @ %onlineTeams @ " \c6------");

				case "all":
					%this.chatMessage("\c6------ TEAMS ------");
					for(%i = 0; %i < FortWars_TeamGroup.getCount(); %i++)
					{
						%team = FortWars_TeamGroup.getObject(%i);

						%owner = (isObject(findClientByBL_ID(%team.ownerBL_ID)) ? "\c2online" : "\c7offline");
						%allowed = (%team.FortWars_isAllowed(%id) ? "\c4Allowed to join" : "\c0Not allowed to join (Request permission)");
						if(%team.ownerBL_ID == %this.getBLID())
							%allowed = "\c3You own this team";
						else if(%team == %curTeam)
							%allowed = "\c3You are on this team";

						%onlineCount = %team.getCount();
						%online = "(Online: \c3" @ %onlineCount @ "\c6)";
						if(%onlineCount == 0)
							%online = "(Last online: \c3" @ getStringTime(getTimeString(($Sim::Time - %team.lastLogin) / 60)) @ " ago\c6)";

						%this.schedule(10 * %i, chatMessage, "  \c6(Owner is " @ %owner @ "\c6) " @ %online @" \c3" @ %team.uiName @ " \c7(\c4" @ %team.ownerName @ " \c6: \c4" @ %team.ownerBL_ID @ "\c7) " @ %allowed);
					}
					%this.schedule(10 * %i, chatMessage, "\c6------ Page up (PGUP/PGDN) \c7| \c3/FortWars Join \c2name/ID \c7| \c6Total teams: \c3" @ FortWars_TeamGroup.getCount() @ " \c6------");

				default:
					%this.chatMessage("\c6---- \c3/FortWars ListTeams/TeamList \c6----");
					%this.chatMessage("  \c3/FortWars ListTeams \c2Online \c7- \c6Show ONLY online teams");
					%this.chatMessage("  \c3/FortWars ListTeams \c2Offline \c7- \c6Show ONLY offline teams");
					%this.chatMessage("  \c3/FortWars ListTeams \c2All \c7- \c6Show ALL teams");
			}

		case "setTeamColor":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You're not on a team!");
				return;
			}

			if(!%this.FortWars_CanSetPrefs())
			{
				%this.chatMessage("\c6You are not allowed to modify this team.");
				return;
			}

			if($Sim::Time - %this.lastTeamColorRequest < 5)
			{
				%time = mFloatLength(5 - ($Sim::Time - %this.lastTeamColorRequest), 1);
				return;
			}

			%ck = %team.FortWars_AssignColor(%arg0);
			if(%ck == 1)
				%team.FortWars_messageAll('MsgUploadEnd', "\c2Preference \c7-> \c3" @ %this.getPlayerName() @ " \c6has requested a new team color. Color is set to -> <color:" @ %team.teamColorHex @ ">" @ %team.uiName);
			else if(%ck == 2)
				%this.chatMessage("Invalid color: \c3" @ %arg0 @ " (Team color already exists)");
			else
			{
				if(%arg0 $= "")
					%team.FortWars_messageAll('MsgUploadEnd', "\c2Preference \c7-> \c3" @ %this.getPlayerName() @ " \c6has requested a new team color (random). Color is set to -> <color:" @ %team.teamColorHex @ ">" @ %team.uiName);
				else
					%this.chatMessage("Invalid color: \c3" @ %arg0 @ " (Must be a HEX)");
			}
			%this.lastTeamColorRequest = $Sim::Time;

		case "setPref":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You're not on a team!");
				return;
			}

			if($Sim::Time - %this.lastTeamPrefSet < 1)
			{
				%time = mFloatLength(1 - ($Sim::Time - %this.lastTeamPrefSet), 1);
				return;
			}

			if(!%this.FortWars_CanSetPrefs())
			{
				%this.chatMessage("\c6You are not allowed to modify this team.");
				return;
			}

			switch$(%arg0)
			{
				case "Save":
					%team.FortWars_setPref(%arg0, %arg1, %this);

				case "FF" or "friendlyfire" or "fire" or "dmg":
					%team.FortWars_setPref(%arg0, %arg1, %this);

				default:
					%this.chatMessage("\c6---- \c3/FortWars setPref \c6----");
					%this.chatMessage("  \c3/FortWars setPref \c2Save \c4boolean (0 or 1) \c7- \c6Makes your team permanent, even if the server goes down.");
					%this.chatMessage("  \c3/FortWars setPref \c2FF \c4boolean (0 or 1) \c7- \c6Toggles friendlyfire.");
					%this.chatMessage("  \c3/FortWars setTeamColor \c7- \c6Randomly assigned a new team color.");
			}

			%this.lastTeamPrefSet = $Sim::Time;

		case "listClients":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You're not on a team!");
				return;
			}

			if($Sim::Time - %this.lastTeamCLRequest < 3)
			{
				%time = mFloatLength(3 - ($Sim::Time - %this.lastTeamCLRequest), 1);
				return;
			}
			%this.lastTeamCLRequest = $Sim::Time;
			%id = %this.getBLID();

			%this.chatMessage("\c6------ ONLINE PLAYERS of \c3" @ %team.uiName @ " \c6------");
			for(%i = 0; %i < %team.getCount(); %i++)
			{
				%cl = %team.getObject(%i);
				if(%cl == %this)
					%this.schedule(10 * %i, "  \c3" @ %cl.getPlayerName() @ " \c6(ID: \c3" @ %cl.getBLID() @ "\c6) \c4(You)");
				else
				{
					if(%cl.getBLID() == %team.ownerBL_ID)
					{
						if(isPackage("Script_AdminShields"))
							%this.schedule(10 * %i, chatMessage, "  <bitmap:add-ons/script_adminshields/icon_silverBadge.png> \c3" @ %cl.getPlayerName() @ " \c6(ID: \c3" @ %cl.getBLID() @ "\c6)");
						else
							%this.schedule(10 * %i, chatMessage, "  \c2Owner \c3" @ %cl.getPlayerName() @ " \c6(ID: \c3" @ %cl.getBLID() @ "\c6)");
					}
					else
						%this.schedule(10 * %i, chatMessage, "  \c3" @ %cl.getPlayerName() @ " \c6(ID: \c3" @ %cl.getBLID() @ "\c6)");
				}
			}
			%this.schedule(10 * %i, chatMessage, "\c6------ END ------");

		case "Join":
			%joinClient = findClientByName(%arg);
			if(!isObject(%joinClient))
			{
				%this.chatMessage("\c6Invalid client to join allies with!");
				return;
			}

			if(%joinClient == %this)
			{
				%this.chatMessage("\c6Why would you want to join yourself?");
				return;
			}

			%team = %joinClient.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6They don't have a team!");
				return;
			}

			%this.FortWars_JoinTeam(%team, true);

		case "Admin":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You don't have a team!");
				return;
			}

			if(%this.getBLID() != %team.ownerBL_ID)
				return;

			%allowClient = findClientByName(%arg);
			if(!isObject(%allowClient))
				%allowClient = findClientByBL_ID(%arg);

			if(isObject(%allowClient))
			{
				%aka = " \c6aka \c1" @ %allowClient.getPlayerName();
				%allowID = %allowClient.getBLID();
			}
			else
				%allowID = mClampF(%arg, -1, 999999);

			if(%allowID == %team.ownerBL_ID)
			{
				%this.chatMessage("\c6You own this group.");
				return;
			}

			if(%allowID == -1)
			{
				%this.chatMessage("\c6Invalid client/ID to toggle.");
				return;
			}

			if($Sim::Time - $FortWars::TempData["Admin", %allowID] < 3)
			{
				%time = mFloatLength(3 - ($Sim::Time - $FortWars::TempData["Admin", %allowID]), 1);
				%this.chatMessage("\c6Sorry, you cannot toggle this ID again for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : "") @ ".");
				return;
			}
			$FortWars::TempData["Admin", %allowID] = $Sim::Time;

			if(hasItemOnList(%team.autoAdminList, %allowID))
			{
				%team.autoAdminList = removeItemFromList(%team.autoAdminList SPC %allowID);
				%team.FortWars_MessageAll('', "\c3" @ %this.getPlayerName() @ " \c6has demoted BL_ID: \c1" @ %allowID @ %aka @ "\c6.");
			}
			else
			{
				%team.FortWars_MessageAll('', "\c3" @ %this.getPlayerName() @ " \c6has promoted BL_ID: \c1" @ %allowID @ %aka @ " \c6to \c3Team Admin\c6.");
				%team.autoAdminList = addItemToList(%team.autoAdminList, %allowID);
				if(isObject(%allowClient))
					%allowClient.FortWars_AutoAdminCheck(true);
			}

		case "SuperAdmin":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You don't have a team!");
				return;
			}

			if(%this.getBLID() != %team.ownerBL_ID)
				return;

			%allowClient = findClientByName(%arg);
			if(!isObject(%allowClient))
				%allowClient = findClientByBL_ID(%arg);

			if(isObject(%allowClient))
			{
				%aka = " \c6aka \c1" @ %allowClient.getPlayerName();
				%allowID = %allowClient.getBLID();
			}
			else
				%allowID = mClampF(%arg, -1, 999999);

			if(%allowID == %team.ownerBL_ID)
			{
				%this.chatMessage("\c6You own this group.");
				return;
			}

			if(%allowID == -1)
			{
				%this.chatMessage("\c6Invalid client/ID to toggle.");
				return;
			}

			if($Sim::Time - $FortWars::TempData["SuperAdmin", %allowID] < 3)
			{
				%time = mFloatLength(3 - ($Sim::Time - $FortWars::TempData["SuperAdmin", %allowID]), 1);
				%this.chatMessage("\c6Sorry, you cannot toggle this ID again for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : "") @ ".");
				return;
			}
			$FortWars::TempData["SuperAdmin", %allowID] = $Sim::Time;

			if(hasItemOnList(%team.autoSuperAdminList, %allowID))
			{
				%team.autoSuperAdminList = removeItemFromList(%team.autoSuperAdminList SPC %allowID);
				%team.FortWars_MessageAll('', "\c3" @ %this.getPlayerName() @ " \c6has demoted BL_ID: \c1" @ %allowID @ %aka @ "\c6.");
			}
			else
			{
				if(hasItemOnList(%team.autoAdminList, %allowID))
					%team.autoAdminList = removeItemFromList(%team.autoAdminList, %allowID);

				%team.FortWars_MessageAll('', "\c3" @ %this.getPlayerName() @ " \c6has promoted BL_ID: \c1" @ %allowID @ %aka @ " \c6to \c3Team Super Admin\c6.");
				%team.autoSuperAdminList = addItemToList(%team.autoSuperAdminList, %allowID);
				if(isObject(%allowClient))
					%allowClient.FortWars_AutoAdminCheck(true);
			}

		case "Allow" or "Invite":
			%team = %this.FortWars_getTeam();
			if(!isObject(%team))
			{
				%this.chatMessage("\c6You don't have a team!");
				return;
			}

			if(!%this.FortWars_CanInvite())
			{
				%this.chatMessage("\c6You are not allowed to invite anyone.");
				return;
			}

			%allowClient = findClientByName(%arg);
			if(!isObject(%allowClient))
				%allowClient = findClientByBL_ID(%arg);

			if(isObject(%allowClient))
			{
				if(%allowClient == %this)
				{
					%this.chatMessage("\c6Why would you want to join yourself?");
					return;
				}

				%allowID = %allowClient.getBLID();
				%aka = " \c6aka \c1" @ %allowClient.getPlayerName();
			}
			else
				%allowID = mClampF(%arg, -1, 999999);

			if(%allowID == %team.ownerBL_ID)
			{
				%this.chatMessage("\c6The owner owns the group, you cannot toggle them.");
				return;
			}

			if(%allowID == -1)
			{
				%this.chatMessage("\c6Invalid client/ID to toggle.");
				return;
			}

			if($Sim::Time - $FortWars::TempData["Timeout", %allowID] < 10)
			{
				%time = mFloatLength(10 - ($Sim::Time - $FortWars::TempData["Timeout", %allowID]), 1);
				%this.chatMessage("\c6Sorry, you cannot toggle this ID again for \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : "") @ ".");
				return;
			}

			$FortWars::TempData["Timeout", %allowID] = $Sim::Time;
			%allowed = hasItemOnList(%team.allowBLID, %allowID);

			if(%allowed)
				%team.FortWars_removeID(%allowID);
			else
				%team.FortWars_addID(%allowID);

			%allowed = !%allowed; //Reverse the ID

			%team.FortWars_MessageAll('', "\c3" @ %this.getPlayerName() @ " \c6has " @ (%allowed ? "" : "dis") @ "allowed BL_ID: \c1" @ %allowID @ %aka @ " \c6to join the team. (Total allows: \c3" @ getWordCount(%team.allowBLID) @ "\c6)");

			if(isObject(%allowClient))
				%allowClient.chatMessage("\c3" @ %this.getPlayerName() @ " \c6has " @ (%allowed ? "allowed you to join their team. If you want to join, say \c3/FortWars join " @ %this.getPlayerName() @ "\c6!" : "disallowed you from joining their team, sorry."));

		case "Create":
			%this.FortWars_CreateTeam(%arg);

		case "Rename":
			%this.FortWars_RenameTeam(%arg);

		case "Leave":
			%this.FortWars_LeaveTeam();

		case "Rules":
			if(!isFunction(serverCmdRules))
			{
				%this.chatMessage("\c6There are rules for FortWars, please use common sense and do not abuse any events.");
				return;
			}

			serverCmdRules(%this);

		case "NSA":
			if(!%this.isAdmin)
				return;

			%this.FortWarsData["NSA"] = !%this.FortWarsData["NSA"];
			%this.chatMessage("\c6Team listening is now " @ (%this.FortWarsData["NSA"] ? "\c0ON" : "\c2OFF") @ "\c6.");

		case "help":
			switch$(%arg0)
			{
				case "rules":
					serverCmdFW(%this, "Rules");

				case "team" or "teams":
					%this.chatMessage("  /FW \c3ListTeams \c7- \c6Lists all active teams on the servers.");
					%this.chatMessage("  /FW \c3ListClients \c7- \c6Lists all active clients on your team if you have one.");
					%this.chatMessage("  /FW \c3Create \c2name \c7- \c6Creates a fort wars team, anyone on this team cannot kill each other. People can only get permission to join.");
					%this.chatMessage("  /FW \c3Join \c2client name \c7- \c6Join someone's team. They need your agreement first (/FW Allow " @ %this.getBLID() @ ")! They must be online!");
					%this.chatMessage("  /FW \c3Leave \c7- \c6Leave your team. If you're the host of the team you will abandon it, which means everyone gets kicked out.");
	
				case "leader":
					%this.chatMessage("  /FW \c3Admin \c2client name/bl_id \c7- \c6Gives admin to the ID/Name to the team, they can invite players. Must be the host of the team.");
					%this.chatMessage("  /FW \c3SuperAdmin \c2client name/bl_id \c7- \c6Gives admin to the ID/Name to the team, they can invite and kick players. Must be the host of the team.");
					%this.chatMessage("  /FW \c3Rename \c2name \c7- \c6Renames your team. Must be the host of the team.");
					%this.chatMessage("  /FW \c3setPref \c2name \c4value \c7- \c6Leave it blank to see what prefs you can do.");
					%this.chatMessage("  /FW \c3setTeamColor \c7- \c6Randomly assigned a new team color.");

				case "admin" or "a":
					if(%this.isAdmin)
					{
						%this.chatMessage(" \c6== \c2Admin Commands \c6==");
						%this.chatMessage("  /FW \c3NSA \c7- \c6You can listen to any team chat, don't be rude here if you're just doing it to watch their plans.");
					}

				default:
					%this.chatMessage("  /FW \c3Rules \c7- \c6Know the rules, otherwise breaking any of them will get you banned.");
					%this.chatMessage("  /FW \c3Create \c2name \c7- \c6Creates a fort wars team, anyone on this team cannot kill each other. People can only get permission to join.");
					%this.chatMessage("  /FW \c3Join \c2client name \c7- \c6Join someone's team. They need your agreement first (/FW Allow " @ %this.getBLID() @ ")! They must be online!");
					%this.chatMessage("  /FW \c3Build \c6or \c3Attack \c7- \c6Toggles being a builder, these have long timeouts so I wouldn't recommend quick changing just to kill people. Build or attack.");
			}

		default:
			%this.chatMessage("\c6---- \c3FortWars Commands (Mod made by Visolator [ID: 48980]) \c6----");
			//%this.chatMessage("\c3/command \c3category \c7- \c6Description");
			//%this.chatMessage("\c3/FortWars \c3category \c7- \c6Help");
			%this.chatMessage("  \c6/FW Help \c3Rules \c4 \c7- \c6See the rules (or /FW Rules)");
			%this.chatMessage("  \c6/FW Help \c3Teams \c4 \c7- \c6See team commands");
			%this.chatMessage("  \c6/FW Help \c3Leader \c4 \c7- \c6See team leader commands");
			if(%this.isAdmin)
				%this.chatMessage("  \c6/FW Help \c3Admin \c4 \c7- \c6See admin commands");

			//%this.chatMessage("   \c3category \c2thing \c7- \c6Description");
	}
}

function serverCmdToggleBuild(%this){serverCmdFortWars(%this, "Build");}
function serverCmdToggleBuilder(%this){serverCmdFortWars(%this, "Build");}
function serverCmdToggleBuilding(%this){serverCmdFortWars(%this, "Build");}

function GameConnection::FW_Build(%this, %bypass)
{
	if($Sim::Time - %this.lastBuilderSwitch < 20)
	{
		%time = mFloatLength(20 - ($Sim::Time - %this.lastBuilderSwitch), 1);
		%this.chatMessage("\c6Sorry, you can't toggle being a builder yet. Please wait \c3" @ %time @ " \c6second" @ (%time != 1 ? "s" : "") @ ".");
		return;
	}

	if(%this.isBuilder && isObject(%player = %this.player))
		if(isObject(%player.getObjectMount()))
		{
			%this.chatMessage("\c6Please get out of the vehicle first.");
			return;
		}

	//They want to turn it off
	if(!%this.FortWarsData["SpawnInit"] && %this.isBuilder)
	{
		%team = %this.FortWars_getTeam();
		if(isObject(%team) && !isObject(FortWars_FindSpawnBrick(%team.ownerBL_ID)))
		{
			commandToClient(%this, 'MessageBoxYesNo', "FortWars - Combat mode", "<br>Your team does not have a spawn set!<br><br>Continue?", 'AcceptFortWarsCombat');
			return;
		}
		else if(!isObject(FortWars_FindSpawnBrick(%this.getBLID())) && !isObject(%team))
		{
			commandToClient(%this, 'MessageBoxYesNo', "FortWars - Combat mode", "<br>You do not have a spawn set!<br><br>Continue?", 'AcceptFortWarsCombat');
			return;
		}

		%this.FortWarsData["SpawnInit"] = 1;
	}

	if(isObject(%player) && !%bypass)
	{
		initContainerRadiusSearch(%player.getPosition(), 5, $TypeMasks::FxBrickAlwaysObjectType);
		while(isObject(%brick = containerSearchNext()))
		{
			if(isObject(%brickClient = getBrickgroupFromObject(%brick).client) && %brickClient.getClassName() $= "GameConnection" && isObject(%brickClient.minigame))
				if((%this.FortWars_getTeam() == 0 && %brickClient.FortWars_getTeam() == 0 || %brickClient.FortWars_getTeam() != %this.FortWars_getTeam()) && %this != %brickClient)
				{
					%this.chatMessage("\c6You can't toggle builder near an enemy's base.");
					return;
				}
		}
	}

	%this.isBuilder = !%this.isBuilder;
	%this.chatMessage("\c6Builder is now " @ (%this.isBuilder ? "\c2ON\c6. \c0Damaging anyone \c6will turn this back off." : "\c0OFF\c6. You can now be damaged after 5 seconds, unless you damage someone before that."));
	%this.FortWars_Apply(true);
	%this.lastBuilderSwitch = $Sim::Time;
}

function GameConnection::FW_Cmd(%this, %command, %str1, %str2)
{
	serverCmdFortWars(%this, %command, %str1, %str2);
}
registerOutputEvent("GameConnection", "FW_Cmd", "string 155 50" TAB "string 155 65" TAB "string 155 65");
restrictOutputEvent("GameConnection", "FW_Cmd");

registerOutputEvent("GameConnection", "FW_Build", "bool");
restrictOutputEvent("GameConnection", "FW_Build");