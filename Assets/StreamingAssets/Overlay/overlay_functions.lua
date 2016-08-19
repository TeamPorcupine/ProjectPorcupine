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

-- Returns a magic value if furniture in tile is a power gen
function powerValueAt(tile)
	mid = 128
	if tile == nil or tile.furniture == nil then
		return mid
	else
		if tile.furniture.isPowerGenerator then
			val = mid + 10*tile.furniture.powerValue
		else
			val = mid - 10*tile.furniture.powerValue
		end
	end
	return math.max(math.min(val, 255), 0)
end
