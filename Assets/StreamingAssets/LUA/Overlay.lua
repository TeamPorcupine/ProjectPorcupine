-- Example of overlay function
-- Input: tile, the current tile for which the
-- overlay wants to display the data
-- Input: world, world class
-- Return: an integer (by default should be scaled between 0 and 255)
function oxygenValueAt(tile)
    if tile == nil then
        return 0
    end
    room = tile.room
    if room == nil then
        return 0
    end
    if (room.GetGasAmount("O2") > 0) then
        return 128
    end
    return 0
end

-- Returns room id or null if room or tile invalid
function roomNumberValueAt(tile)
    if tile == nil or tile.room == nil then
        return 0
    else
        return tile.room.ID
    end
end

-- Returns a magic value if furniture in tile is a power gen
function powerValueAt(tile)
    zero = 128 -- This is middle between 0 and 256
    multiplier = 12,8 -- For now 1 power is 40 in overlay
	
	local powerComponent = ModUtils.GetPlugablePowerConnectionForTile(tile)

	if (powerComponent == nil) then
        return zero
    end
    if (powerComponent.IsConsumer) then
        zero = zero - powerComponent.InputRate * multiplier
    end
    if (powerComponent.IsProducer) then
        zero = zero + powerComponent.OutputRate * multiplier
    end
    return ModUtils.Clamp(zero, 0, 256)
end

-- Return temperature (in K) in current tile
function temperatureValueAt(tile, world)
    --if world == nil or world.current == nil or world.current.temperature == nil then
    --	return -1
    --end

    --if tile == nil then
    --	return -2
    --end
    return math.max(math.min(world.temperature.GetTemperature(tile.X, tile.Y, tile.Z) / 3, 254), 0)
end


-- Returns coloring of thermal diffusivity of tile
function thermalDiffusivityValueAt(tile, world)
    if tile == nil then
        return 255
    else
        return 255
    end
end

-- Dummy function, will be implemented
function heatGenerationValueAt(tile, world)
    return 0
end
