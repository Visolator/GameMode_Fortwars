///////////////////////////////////
//
//   Add-On: Baseplate Rules
//
//   ---------------------------
//
//   Author: General
//   BL ID: 4878
//
//   ---------------------------
//
//   File: server.cs
//
//   File version: 5.2
//   Last updated on: 24/05/2011
//
///////////////////////////////////


//Removed client stuff - Visolator
//Also added edits

if($Server::LAN)
{
	error("ERROR: Baseplate Rules - Internet server required! Does not function on a non-internet server");
	return;
}

if(!$Pref::Server::BREnable)
{
	$Pref::Server::BREnable = 2;
	$Pref::Server::BRAppliesToAdmin = 1;
}
$BR_EnablePress = $Pref::Server::BREnable;

//Creates exception array
$BR_NumExceptions = 0;
for(%i=1;%i<getWordCount($Pref::Server::BRExceptions);%i++)
{
	if(getWord($Pref::Server::BRExceptions,%i) !$= ",")
	{
		$BR_Exception[$BR_NumExceptions] = getWord($Pref::Server::BRExceptions,%i);
		$BR_NumExceptions ++;
	}
}

if(isPackage(baseplateRulesServer))
	deactivatePackage(baseplateRulesServer);

package baseplateRulesServer
{
	function gameConnection::autoAdminCheck(%this)
	{
		//Sets player exception variable to 1 if on the list
		for(%i=0;%i<$BR_NumExceptions;%i++)
		{
			if(%this.getBLID() $= $BR_Exception[%i])
				%this.BRException = 1;
		}
		return parent::autoAdminCheck(%this);
	}

	function serverCmdPlantBrick(%client)
	{
		if(isObject(%player = %client.player) && isObject(%temp = %player.tempBrick))
		{
			if($Pref::Server::BR_EnableRange && !%client.isSuperAdmin && !%client.BRException)
			{
				if(isObject(%brBrick = $Server::BR_Brick))
				{
					if(vectorDist(%temp.getPosition(), %brBrick.getPosition()) > $Pref::Server::BR_Range && $Pref::Server::BR_Range > 0)
					{
						commandToClient(%client,'MessageBoxOK',"Too far!","Please build closer!");
						return;
					}

					if(vectorDist(%temp.getPosition(), %brBrick.getPosition()) <= $Pref::Server::BR_TooCloseRange && $Pref::Server::BR_TooCloseRange > 0)
					{
						commandToClient(%client,'MessageBoxOK',"Too close!","Please build father!");
						return;
					}

					%pos = "0";
					if(isObject(groundPlane))
						%pos = getWord(groundPlane.getPosition(), 2);

					%result = getWord(%temp.getPosition(), 2) - %pos;
					if(%result > $Pref::Server::BR_MaxHeight && $Pref::Server::BR_MaxHeight > 0)
					{
						commandToClient(%client,'MessageBoxOK',"Too high!","Please build lower!");
						return;
					}
				}
			}
		}
		return Parent::serverCmdPlantBrick(%client);
	}

	function fxDtsBrick::onPlant(%this)
	{
		Parent::onPlant(%this);
		%this.schedule(0, "BR_Check");
	}

	function NDM_PlantCopy::conditionalPlant(%this, %client, %force)
	{
		%selection = %client.ndSelection;
		%pos = %selection.getGhostWorldBox();

		if($Pref::Server::BR_EnableRange && !%client.isSuperAdmin && !%client.BRException)
		{
			if(isObject(%brBrick = $Server::BR_Brick))
			{
				if(vectorDist(%pos, %brBrick.getPosition()) > $Pref::Server::BR_Range && $Pref::Server::BR_Range > 0)
				{
					commandToClient(%client,'MessageBoxOK',"Too far!","Please build closer!");
					return;
				}

				if(vectorDist(%pos, %brBrick.getPosition()) <= $Pref::Server::BR_TooCloseRange && $Pref::Server::BR_TooCloseRange > 0)
				{
					commandToClient(%client,'MessageBoxOK',"Too close!","Please build father!");
					return;
				}

				%pos = "0";
				if(isObject(groundPlane))
					%pos = getWord(groundPlane.getPosition(), 2);

				%result = getWord(%pos, 2) - %pos;
				if(%result > $Pref::Server::BR_MaxHeight && $Pref::Server::BR_MaxHeight > 0)
				{
					commandToClient(%client,'MessageBoxOK',"Too high!","Please build lower!");
					return;
				}
			}
		}

		return Parent::conditionalPlant(%this, %client, %force);
	}

	function serverCmdBRexceptionRemove(%client,%ID,%gui)
	{
		if(%client.getBLID() == getNumKeyID())
		{
			if($Pref::Server::BREnable == 1)
			{
				messageClient(%client,'',"\c3Baseplate Rules \c0are disabled, you must enable them before changing any preferences.");
				return;
			}
			//Checks the string for errors
			%error = BRinsertError(%client,%ID,%gui,1);
			if(%error $= "ERROR")
				return;
			//Removes ID from list
			for(%i=0;%i<$BR_NumExceptions;%i++)
			{
				if(%ID == $BR_Exception[%i])
				{
					if(!%gui)
						messageClient(%client,'',"ID has been removed from the exception list.");
					for(%j=%i;%j<$BR_NumExceptions;%j++)
					{
						$BR_Exception[%j] = $BR_Exception[%j+1];
					}
					$BR_NumExceptions --;
					%found = 1;
				}
			}
			//Error if ID not found on the list
			if(!%found)
			{
				if(!%gui)
					messageClient(%client,'',"Unable to find ID on the exception list.");
				return;
			}
			//Finds player with the same ID as removed from the list and sets their exception variable to 0
			for(%i=0;%i<clientGroup.getCount();%i++)
			{
				if(clientGroup.getObject(%i).getBLID() $= %ID)
					clientGroup.getObject(%i).BRException = 0;
			}
			commandToClient(%client,'BRexceptionRemove',%ID);
			BRexceptionEvaluate();
		}
	}

	function serverCmdBRexceptionAdd(%client,%ID,%gui)
	{
		if(%client.getBLID() == getNumKeyID())
		{
			if($Pref::Server::BREnable == 1)
			{
				messageClient(%client,'',"\c3Baseplate Rules \c0are disabled, you must enable them before changing any preferences.");
				return;
			}
			//Checks the string for errors
			%error = BRinsertError(%client,%ID,%gui);
			if(%error $= "ERROR")
				return;
			//Error if ID is already on the list
			for(%i=0;%i<$BR_NumExceptions+1;%i++)
			{
				if($BR_Exception[%i] $= %ID)
				{
					if(!%gui)
						messageClient(%client,'',"ID is already on the list.");
					commandToClient(%client,'BRexceptionError');
					return;
				}
			}
			//Finds player with the same ID as added to the list and sets their exception variable to 1
			for(%i=0;%i<clientGroup.getCount();%i++)
			{
				if(clientGroup.getObject(%i).getBLID() $= %ID)
					clientGroup.getObject(%i).BRException = 1;
			}
			//Adds ID to exception array
			$BR_Exception[$BR_NumExceptions] = %ID;
			$BR_NumExceptions ++;
			commandToClient(%client,'BRexceptionAdd',%ID);
			BRexceptionEvaluate();
			if(!%gui)
				messageClient(%client,'',"ID has been added to the exception list.");
		}
	}
	
	function serverCmdBRstate(%client)
	{
		if(%client.getBLID() == getNumKeyID())
		{
			//Prepares variables
			if($Pref::Server::BREnable == 2)
				%enabled = "Yes";
			else
				%enabled = "No";
			if($Pref::Server::BRAppliesToAdmin == 0)
				%admin = "Super (Admins)";
			if($Pref::Server::BRAppliesToAdmin == 1)
				%admin = "Admins";
			if($Pref::Server::BRAppliesToAdmin == 2)
				%admin = "No Admin";
			if($BR_NumExceptions == 0)
				%exceptions = "None";
			//Messages the variables
			messageClient(%client,'',"---\c3Baseplate Rules - State\c0------------");
			messageClient(%client,'',"Enabled: \c3" @ %enabled);
			messageClient(%client,'',"Applies to: \c3" @ %admin);
			messageClient(%client,'',"Exceptions: \c3" @ %exceptions);
			for(%i=0;%i<$BR_NumExceptions;%i++)
			{
				messageClient(%client,'',"\c3" @ $BR_Exception[%i]);
			}
			messageClient(%client,'',"--------------------------------------------");
		}
	}
	
	function serverCmdBRcommandHelp(%client)
	{
		if(%client.getBLID() == getNumKeyID())
		{
			messageClient(%client,'',"---\c3Baseplate Rules - Command Help\c0------------");
			messageClient(%client,'',"\c3/BRstate \c0- Displays the current state of the mod.");
			messageClient(%client,''," ");
			messageClient(%client,'',"\c3/BRexceptionAdd ID \c0- Adds a specified ID to the rule exception list.");
			messageClient(%client,'',"\c3/BRexceptionRemove ID \c0- Removes a specified ID from the rule exception list.");
			messageClient(%client,'',"-----------------------------------------------------------");
		}
	}	

	//Creates the preference string
	function BRexceptionEvaluate()
	{
		$Pref::Server::BRExceptions = "";
		for(%i=0;%i<$BR_NumExceptions;%i++)
		{
			$Pref::Server::BRExceptions = $Pref::Server::BRExceptions @ " " @ $BR_Exception[%i] @ " ,";
		}
		if($Pref::Server::BRExceptions $= "")
			$Pref::Server::BRExceptions = " ";
	}

	//Checks for exception adding/removing string errors
	function BRinsertError(%client,%ID,%gui,%ir)
	{
		%length = strLen(%ID);
		//Error if string equals to nothing
		if(%ID $= "")
		{
			if(!%gui)
				messageClient(%client,'',"You must insert something!");
			if(!%ir)
				commandToClient(%client,'BRexceptionError');
			%ID = "ERROR";
			return %ID;
		}
		//Error if string contains a 0 at its beginning (impossible ID)
		if(getSubStr(%ID, 0, 1) $= "0" && %length > 1)
		{
			if(!%gui)
				messageClient(%client,'',"Your ID cannot start with a 0.");
			commandToClient(%client,'BRexceptionError');
			%ID = "ERROR";
			return %ID;
		}
		//Error if string contains any character other than numbers
		for(%i=0;%i<10;%i++)
		{
			if(%i==0)
				%r0 = strReplace(%ID,0,"");
			else
				%r[%i] = strReplace(%r[%i-1],%i,"");
		}
		if(%r9 !$= "")
		{
			if(!%gui)
				messageClient(%client,'',"Your ID can only contain numbers.");
			commandToClient(%client,'BRexceptionError');
			%ID = "ERROR";
			return %ID;
		}
		return %ID;
	}
};
if(isPackage(baseplateRulesServer))
	deactivatePackage(baseplateRulesServer);
activatePackage("baseplateRulesServer");

function fxDtsBrick::BR_Check(%this)
{
	%client = %this.client;
	if(isObject(%client) && %client.getClassName() $= "GameConnection")
	{
		if($Pref::Server::BREnable == 2)
		{
			if(%client.getBLID() == getNumKeyID())
				%ignore = 1;
			if($Pref::Server::BRAppliesToAdmin >= 1 && %client.isSuperAdmin)
				%ignore = 1;
			if($Pref::Server::BRAppliesToAdmin == 1 && %client.isAdmin)
				%ignore = 1;
			if(%client.BRException)
				%ignore = 1;

			if(!%ignore)
			{
				%dist = %this.getDistanceFromGround();
				if(%dist <= 0 && %this.getDatablock().category !$= "Baseplates")
				{
					commandToClient(%client,'MessageBoxOK',"Invalid Brick","You must start with a Baseplate.<bitmap:base/client/ui/brickIcons/16x16 Base>");
					%this.schedule(0, "delete");
				}
				else if(%dist <= 0 && $Pref::Server::FortWars_Terrain)
				{
					commandToClient(%client,'MessageBoxOK',"Invalid Arena","Please build on the terrain.");
					%this.schedule(0, "delete");
				}
			}
		}
	}
}

function fxDtsBrick::BR_SetDist(%this, %type, %range, %client)
{
	if(isObject(%client) && %client.getClassName() $= "GameConnection")
	{
		if(!isObject($Server::BR_Brick))
		{
			announce("\c6Brick distance has been limited by \c2" @ %client.getPlayerName() @ " \c6at \c2" @ %this.getPosition());
			echo("Brick distance has been limited by " @ %client.getPlayerName() @ " at " @ %this.getPosition());
		}

		$Server::BR_Brick = %this;

		switch$(%type)
		{
			case "0" or "Height":
				if(%range != $Pref::Server::BR_MaxHeight)
				{
					announce("\c6Max building height has been set by \c2" @ %client.getPlayerName() @ " \c6at \c2" @ %range);
					echo("Max building height has been set by " @ %client.getPlayerName() @ " at " @ %range);
				}

				$Pref::Server::BR_MaxHeight = %range;

			case "1" or "Distance":
				if(%range != $Pref::Server::BR_Range)
				{
					announce("\c6Building distance has been set by \c2" @ %client.getPlayerName() @ " \c6at \c2" @ %range);
					echo("Building distance has been set by " @ %client.getPlayerName() @ " at " @ %range);
				}

				$Pref::Server::BR_Range = %range;

			case "2" or "Too_Close" or "Too close":
				if(%range != $Pref::Server::BR_TooCloseRange)
				{
					announce("\c6Building too close distance has been set by \c2" @ %client.getPlayerName() @ " \c6at \c2" @ %range);
					echo("Building too close distance has been set by " @ %client.getPlayerName() @ " at " @ %range);
				}

				$Pref::Server::BR_TooCloseRange = %range;
		}
	}
}
registerOutputEvent("fxDtsBrick", "BR_SetDist", "list Height 0 Distance 1 Too_Close 2" TAB "int 0 999999 1000", 1);