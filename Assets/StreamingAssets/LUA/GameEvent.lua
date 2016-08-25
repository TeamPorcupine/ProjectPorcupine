-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

function Precondition_Event_NewCrewMember( gameEvent, deltaTime )
	gameEvent.AddTimer(deltaTime)
	local timer = gameEvent.GetTimer()
	if (timer >= 30.0) then
		gameEvent.ResetTimer()
		return true
	end
end

function Execute_Event_NewCrewMember( gameEvent )
	local tile = World.current.GetTileAt(World.current.Width / 2, World.current.Height / 2)
	c = World.current.CreateCharacter(tile)
	ModUtils.ULog("GameEvent: New Crew Member spawned named '" .. c.GetName() .. "'.")
end

ModUtils.ULog("GameEvent.lua loaded")
return "Event LUA Script Parsed!"
