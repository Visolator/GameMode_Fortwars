function FortWars_SheildGroup()
{
	if(!isObject(FortWars_SheildGroup))
		new SimGroup(FortWars_SheildGroup);

	return nameToID("FortWars_SheildGroup");
}

function FortWars_SheildLoop()
{
	cancel($FortWars_SheildLoopSch);

	if((%count = FortWars_SheildGroup().getCount()) > 0)
	{
		for(%i = 0; %i < %count; %i++)
		{
			%shape = FortWars_SheildGroup().getObject(%i);
			%obj = %shape.attachObj;

			if(!isObject(%obj) || %obj.getState() $= "dead")
				%shape.schedule(0, delete);
			else if(isObject(%shape))
			{
				%scale = getWord(%obj.getDatablock().boundingBox, 2) * 0.75;
				%shape.setScale(vectorScale("1 1 1", %scale));
				%shape.setTransform(vectorAdd(%obj.getPosition(), "0 0 1"));
			}
		}

		$uESS_SphereLoopSch = schedule(10, 0, "FortWars_SheildLoop");
	}
}
schedule(100, 0, "FortWars_SheildLoop");

function Player::spawnShield(%this, %secs)
{
	%time = %secs * 1000;

	%staticShape = %this.StaticShield;
	if(!isObject(%staticShape))
	{
		%scale = getWord(%this.getDatablock().boundingBox, 2) * 0.75;
		%staticShape = %this.StaticShield = new StaticShape()
		{
			datablock = StaticShield;
			position = vectorAdd(%this.getPosition(), "0 0 1");
			scale = vectorScale("1 1 1", %scale);
			attachObj = %this;
		};
		FortWars_SheildGroup().add(%staticShape);

		if(FortWars_SheildGroup().getCount() == 1)
			FortWars_SheildLoop();

		%staticShape.setNodeColor("ALL", "0.5 0.5 0 1");
	}
			
	cancel(%staticShape.staticShapeDel);
	%staticShape.staticShapeDel = %staticShape.schedule(%time, delete);
}