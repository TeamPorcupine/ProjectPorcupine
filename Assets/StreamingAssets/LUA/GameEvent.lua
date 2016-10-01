-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

function Precondition_Event_NewCrewMember(gameEvent, deltaTime)
    gameEvent.AddTimer(deltaTime)
    local timer = gameEvent.GetTimer()
    if (timer >= 30.0) then
        gameEvent.ResetTimer()
        return true
    end
end

function Execute_Event_NewCrewMember( gameEvent )
    local tile = World.Current.GetTileAt(World.Current.Width / 2, World.Current.Height / 2, 0)
    local character = World.Current.CharacterManager.Create(tile)
    ModUtils.ULog("GameEvent: New Crew Member spawned named '" .. character.GetName() .. "'.")
end


function Precondition_Event_Fire( gameEvent, deltaTime )
	gameEvent.AddTimer(deltaTime)
	local timer = gameEvent.GetTimer()
	if ( math.random() <=  math.min(- 1 / timer + 1, 0.01 ) ) then
		gameEvent.ResetTimer()
		return true
	end
end

function Execute_Event_Fire( gameEvent )
	-- NOTE: this choosees a random tile in the world, not nessicarily near the base
	local tile = World.Current.GetTileAt(
		math.floor(math.random() * World.Current.Width),
		math.floor(math.random() * World.Current.Height))
	if tile != nil then
		World.Current.PlaceFurniture("fire", tile, false)
		ModUtils.ULog("GameEvent: fire spread '" .. tile.X .. "," .. tile.Y .. "'.")
	end
end

ModUtils.ULog("GameEvent.lua loaded")
return "Event LUA Script Parsed!"
