-- Example of overlay function
-- Input: tile, the current tile for which the
-- overlay wants to display the data
-- Return: an integer (by default should be scaled between 0 and 255)
function oxygenValueAt ( tile )
	if tile == nil then
		return 0
	end
    room = tile.room
    if room == nil then
		return 0
	end
    return room.GetGasAmount("O2") * 1e3
end

-- Returns room id or null if room or tile invalid
function roomNumberValueAt ( tile )
	if tile == nil or tile.room == nil then
		return 0
	else 
		return tile.room.ID
	end
end
