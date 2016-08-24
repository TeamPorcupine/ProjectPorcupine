-- Example of overlay function
-- Input: tile, the current tile for which the
-- overlay wants to display the data
-- Input: world, world class
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
		val = mid + 10*tile.furniture.powerValue
	end
	return math.max(math.min(val, 254), 0)
end

-- Return temperature (in K) in current tile
function temperatureValueAt( tile, world )
	--if world == nil or world.current == nil or world.current.temperature == nil then
	--	return -1
	--end
	
	--if tile == nil then
	--	return -2
	--end
	return math.max(math.min(world.temperature.GetTemperature(tile.X, tile.Y) / 3, 254), 0)
end


-- Returns coloring of thermal diffusivity of tile
function thermalDiffusivityValueAt(tile, world)
	if tile == nil then
		return 255
	else
		return math.max(math.min(254*world.temperature.GetThermalDiffusivity(tile.X, tile.Y)))
	end
end

-- Dummy function, will be implemented
function heatGenerationValueAt( tile, world )
	return 0
end
