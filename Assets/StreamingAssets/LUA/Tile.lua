--------------------------------      UTILITY      --------------------------------
function Clamp01( value )
	if (value > 1) then
		return 1
	elseif (value < 0) then
		return 0
	end

	return value
end

-------------------------------- Tile Actions --------------------------------
function MovementCost_Standard ( tile )
	if (tile.furniture == nil) then
		return tile.Type.BaseMovementCost
	end

	return tile.Type.BaseMovementCost * tile.furniture.movementCost
end

--TODO: This needs to be cleaned up
function CanBuildHere_Standard ( tile )
	return true
end

function CanBuildHere_Ladder ( tile )
	return tile.room.IsOutsideRoom()
end

return "LUA Tile Script Parsed!"
