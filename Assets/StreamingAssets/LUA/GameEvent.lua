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
	World.current.CreateCharacter(tile)
	return "GameEvent!"
end

return "Event LUA Script Parsed!"
