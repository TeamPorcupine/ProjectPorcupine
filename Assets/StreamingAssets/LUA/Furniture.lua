-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

-- TODO: Figure out the nicest way to have unified defines/enums
-- between C# and Lua so we don't have to duplicate anything.
ENTERABILITY_YES = 0
ENTERABILITY_NO = 1
ENTERABILITY_SOON = 2

-- HOWTO Log:
-- ModUtils.ULog("Testing ModUtils.ULogChannel")
-- ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
-- ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

-------------------------------- Furniture Actions --------------------------------
function OnUpdate_GasGenerator( furniture, deltaTime )
    if ( furniture.Tile.Room == nil ) then
		return "Furniture's room was null."
	end

    local keys = furniture.Parameters["gas_gen"].Keys()
    for discard, key in pairs(keys) do
        if ( furniture.Tile.Room.GetGasPressure(key) < furniture.Parameters["gas_gen"][key]["gas_limit"].ToFloat()) then
            furniture.Tile.Room.ChangeGas(key, furniture.Parameters["gas_per_second"].ToFloat() * deltaTime * furniture.Parameters["gas_gen"][key]["gas_limit"].ToFloat())
        else
            -- Do we go into a standby mode to save power?
        end
    end
	return
end

function OnUpdate_Door( furniture, deltaTime )
	if (furniture.Parameters["is_opening"].ToFloat() >= 1.0) then
		furniture.Parameters["openness"].ChangeFloatValue(deltaTime * 4) -- FIXME: Maybe a door open speed parameter?
		if (furniture.Parameters["openness"].ToFloat() >= 1)  then
			furniture.Parameters["is_opening"].SetValue(0)
		end
	else
        furniture.Parameters["openness"].ChangeFloatValue(deltaTime * -4)
	end

	furniture.Parameters["openness"].SetValue( ModUtils.Clamp01(furniture.Parameters["openness"].ToFloat()) )
	furniture.UpdateOnChanged(furniture);
end


function OnUpdate_AirlockDoor( furniture, deltaTime )
    if (furniture.Parameters["pressure_locked"].ToFloat() >= 1.0) then
        local neighbors = furniture.Tile.GetNeighbours(false)
        local adjacentRooms = {}
        local pressureEqual = true;
        local count = 0
        for k, tile in pairs(neighbors) do
            if (tile.Room != nil) then
                count = count + 1
                adjacentRooms[count] = tile.Room
            end
        end
        if(ModUtils.Round(adjacentRooms[1].GetTotalGasPressure(),3) == ModUtils.Round(adjacentRooms[2].GetTotalGasPressure(),3)) then
            OnUpdate_Door(furniture, deltaTime)
        end
    else
        OnUpdate_Door(furniture, deltaTime)
    end
end
        
    
function AirlockDoor_Toggle_Pressure_Lock(furniture, character)

    ModUtils.ULog("Toggling Pressure Lock")
    
	if (furniture.Parameters["pressure_locked"].ToFloat() == 1) then
        furniture.Parameters["pressure_locked"].SetValue(0)
    else
        furniture.Parameters["pressure_locked"].SetValue(1)
    end
    
    ModUtils.ULog(furniture.Parameters["pressure_locked"].ToFloat())
end


function OnUpdate_Leak_Door( furniture, deltaTime )
	furniture.Tile.EqualiseGas(deltaTime * 10.0 * (furniture.Parameters["openness"].ToFloat() + 0.1))
end

function OnUpdate_Leak_Airlock( furniture, deltaTime )
	furniture.Tile.EqualiseGas(deltaTime * 10.0 * (furniture.Parameters["openness"].ToFloat()))
end

function IsEnterable_Door( furniture )
	furniture.Parameters["is_opening"].SetValue(1)

	if (furniture.Parameters["openness"].ToFloat() >= 1) then
		return ENTERABILITY_YES --ENTERABILITY.Yes
	end

    return ENTERABILITY_SOON --ENTERABILITY.Soon
end

function GetSpriteName_Door( furniture )

	local openness = furniture.Parameters["openness"].ToFloat()

	if (furniture.verticalDoor == true) then
			-- Door is closed
		if (openness < 0.1) then
			return "DoorVertical_0"
		end

		if (openness < 0.25) then
			return "DoorVertical_1"
		end

		if (openness < 0.5) then
			return "DoorVertical_2"
		end

		if (openness < 0.75) then
			return "DoorVertical_3"
		end

		if (openness < 0.9) then
			return "DoorVertical_4"
		end
		-- Door is a fully open
		return "DoorVertical_5"
	end


	-- Door is closed
	if (openness < 0.1) then
		return "DoorHorizontal_0"
	end

	if (openness < 0.25) then
		return "DoorHorizontal_1"
	end

	if (openness < 0.5) then
		return "DoorHorizontal_2"
	end

	if (openness < 0.75) then
		return "DoorHorizontal_3"
	end

	if (openness < 0.9) then
		return "DoorHorizontal_4"
	end
	-- Door is a fully open
	return "DoorHorizontal_5"
end

function GetSpriteName_Airlock( furniture )

	local openness = furniture.Parameters["openness"].ToFloat()

	-- Door is closed
	if (openness < 0.1) then
		return "Airlock"
	end
	-- Door is a bit open
	if (openness < 0.5) then
		return "Airlock_openness_1"
	end
	-- Door is a lot open
	if (openness < 0.9) then
		return "Airlock_openness_2"
	end
	-- Door is a fully open
	return "Airlock_openness_3"
end

function Stockpile_GetItemsFromFilter( furniture )
	-- TODO: This should be reading from some kind of UI for this
	-- particular stockpile

    -- Probably, this doesn't belong in Lua at all and instead we should
    -- just be calling a C# function to give us the list.

    -- Since jobs copy arrays automatically, we could already have
    -- an Inventory[] prepared and just return that (as a sort of example filter)

	--return { Inventory.__new("Steel Plate", 50, 0) }
	return furniture.AcceptsForStorage()
end

function Stockpile_UpdateAction( furniture, deltaTime )
    -- We need to ensure that we have a job on the queue
    -- asking for either:
    -- (if we are empty): That ANY loose inventory be brought to us.
    -- (if we have something): Then IF we are still below the max stack size,
    -- that more of the same should be brought to us.

    -- TODO: This function doesn't need to run each update. Once we get a lot
    -- of furniture in a running game, this will run a LOT more than required.
    -- Instead, it only really needs to run whenever:
    -- -- It gets created
    -- -- A good gets delivered (at which point we reset the job)
    -- -- A good gets picked up (at which point we reset the job)
    -- -- The UI's filter of allowed items gets changed

    if( furniture.Tile.Inventory != nil and furniture.Tile.Inventory.StackSize >= furniture.Tile.Inventory.MaxStackSize ) then
        -- We are full!
        furniture.CancelJobs()
        return
    end

    -- Maybe we already have a job queued up?
    if( furniture.JobCount() > 0 ) then
        -- Cool, all done.
        return
    end

    -- We Currently are NOT full, but we don't have a job either.
    -- Two possibilities: Either we have SOME inventory, or we have NO inventory.

    -- Third possibility: Something is WHACK
    if( furniture.Tile.Inventory != nil and furniture.Tile.Inventory.StackSize == 0 ) then
        furniture.CancelJobs()
        return "Stockpile has a zero-size stack. This is clearly WRONG!"
    end


	-- TODO: In the future, stockpiles -- rather than being a bunch of individual
	-- 1x1 tiles -- should manifest themselves as single, large objects.  This
	-- would respresent our first and probably only VARIABLE sized "furniture" --
	-- at what happenes if there's a "hole" in our stockpile because we have an
	-- actual piece of furniture (like a cooking stating) installed in the middle
	-- of our stockpile?
	-- In any case, once we implement "mega stockpiles", then the job-creation system
	-- could be a lot smarter, in that even if the stockpile has some stuff in it, it
	-- can also still be requestion different object types in its job creation.

    local itemsDesired = {}

	if( furniture.Tile.Inventory == nil ) then
		--ModUtils.ULog("Creating job for new stack.")
		itemsDesired = Stockpile_GetItemsFromFilter( furniture )
	else
		--ModUtils.ULog("Creating job for existing stack.")
		desInv = furniture.Tile.Inventory.Clone()
		desInv.MaxStackSize = desInv.MaxStackSize - desInv.StackSize
		desInv.StackSize = 0

        itemsDesired = { desInv }
    end

	local j = Job.__new(
		furniture.Tile,
		nil,
		nil,
		0,
		itemsDesired,
		Job.JobPriority.Low,
		false
	)
	j.JobDescription = "job_stockpile_moving_desc"
	j.acceptsAny = true

	-- TODO: Later on, add stockpile priorities, so that we can take from a lower
	-- priority stockpile for a higher priority one.
	j.canTakeFromStockpile = false

	j.RegisterJobWorkedCallback("Stockpile_JobWorked")
	furniture.AddJob( j )
end

function Stockpile_JobWorked(j)
    j.CancelJob()

    -- TODO: Change this when we figure out what we're doing for the all/any pickup job.
    --values = j.GetInventoryRequirementValues();
    for k, inv in pairs(j.inventoryRequirements) do
        if(inv.StackSize > 0) then
            World.Current.inventoryManager.PlaceInventory(j.tile, inv)
            return -- There should be no way that we ever end up with more than on inventory requirement with StackSize > 0
        end
    end
end

function MiningDroneStation_UpdateAction( furniture, deltaTime )
    local spawnSpot = furniture.GetSpawnSpotTile()

	if( furniture.JobCount() > 0 ) then
		-- Check to see if the Metal Plate destination tile is full.
		if( spawnSpot.Inventory != nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
			-- We should stop this job, because it's impossible to make any more items.
			furniture.CancelJobs()
		end
		return
	end

	-- If we get here, then we have no Current job. Check to see if our destination is full.
	if( spawnSpot.Inventory != nil and spawnSpot.Inventory.StackSize >= spawnSpot.Inventory.MaxStackSize ) then
		-- We are full! Don't make a job!
		return
	end

	if(furniture.GetSpawnSpotTile().Inventory != nil and furniture.GetSpawnSpotTile().Inventory.Type != furniture.Parameters["mine_type"].ToString()) then
		return
	end

	-- If we get here, we need to CREATE a new job.
	local jobSpot = furniture.GetJobSpotTile()
	local j = Job.__new(
		jobSpot,
		nil,
		nil,
		1,
		nil,
		Job.JobPriority.Medium,
		true	-- This job repeats until the destination tile is full.
	)

	j.RegisterJobCompletedCallback("MiningDroneStation_JobComplete")
	j.JobDescription = "job_mining_drone_station_mining_desc"
	furniture.AddJob( j )
end

function MiningDroneStation_JobComplete(j)
	if (j.furniture.GetSpawnSpotTile().Inventory == nil or j.furniture.GetSpawnSpotTile().Inventory.Type == j.furniture.Parameters["mine_type"].ToString()) then
		World.Current.inventoryManager.PlaceInventory( j.furniture.GetSpawnSpotTile(), Inventory.__new(j.furniture.Parameters["mine_type"].ToString() , 2) )
	else
		j.CancelJob()
	end
end

function MiningDroneStation_Change_to_Raw_Iron(furniture, character)
	furniture.Parameters["mine_type"].SetValue("Raw Iron")
end

function MiningDroneStation_Change_to_Raw_Copper(furniture, character)
	furniture.Parameters["mine_type"].SetValue("raw_copper")
end

function MetalSmelter_UpdateAction(furniture, deltaTime)
    local spawnSpot = furniture.GetSpawnSpotTile()

    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize >= 5) then
        furniture.Parameters["smelttime"].ChangeFloatValue(deltaTime)
        if(furniture.Parameters["smelttime"].ToFloat() >= furniture.Parameters["smelttime_required"].ToFloat()) then
            furniture.Parameters["smelttime"].SetValue(0)
            local outputSpot = World.Current.GetTileAt(spawnSpot.X+2, spawnSpot.Y, spawnSpot.Z)

            if(outputSpot.Inventory == nil) then
                World.Current.inventoryManager.PlaceInventory(outputSpot, Inventory.__new("Steel Plate", 5))
                spawnSpot.Inventory.StackSize = spawnSpot.Inventory.StackSize - 5
            else
                if(outputSpot.Inventory.StackSize <= 45) then
                    outputSpot.Inventory.StackSize = outputSpot.Inventory.StackSize + 5
                    spawnSpot.Inventory.StackSize = spawnSpot.Inventory.StackSize - 5
                end
            end

            if(spawnSpot.Inventory.StackSize <= 0) then
                spawnSpot.Inventory = nil
            end
        end
    end

    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize == spawnSpot.Inventory.MaxStackSize) then
        -- We have the max amount of resources, cancel the job.
        -- This check exists mainly, because the job completed callback doesn't
        -- seem to be reliable.
        furniture.CancelJobs()
        return
    end

    if(furniture.JobCount() > 0) then
        return
    end

    -- Create job depending on the already available stack size.
    local desiredStackSize = 50
    local itemsDesired = { Inventory.__new("Raw Iron", 0, desiredStackSize) }
    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.StackSize < spawnSpot.Inventory.MaxStackSize) then
        desiredStackSize = spawnSpot.Inventory.MaxStackSize - spawnSpot.Inventory.StackSize
        itemsDesired[1].MaxStackSize = desiredStackSize
    end
    ModUtils.ULog("MetalSmelter: Creating job for " .. desiredStackSize .. " raw iron.")

    local jobSpot = furniture.GetJobSpotTile()
    local j = Job.__new(
        jobSpot,
        nil,
        nil,
        0.4,
        itemsDesired,
        Job.JobPriority.Medium,
        false
    )

    j.RegisterJobWorkedCallback("MetalSmelter_JobWorked")
    furniture.AddJob(j)
    return
end

function MetalSmelter_JobWorked(j)
    j.CancelJob()
    local spawnSpot = j.tile.Furniture.GetSpawnSpotTile()
    for k, inv in pairs(j.inventoryRequirements) do
        if(inv ~= nil and inv.StackSize > 0) then
            World.Current.inventoryManager.PlaceInventory(spawnSpot, inv)
            spawnSpot.Inventory.Locked = true
            return
        end
    end
end

function CloningPod_UpdateAction(furniture, deltaTime)
	
    if( furniture.JobCount() > 0 ) then
        return
    end

    local j = Job.__new(
        furniture.GetJobSpotTile(),
        nil,
        nil,
        10,
        nil,
        Job.JobPriority.Medium,
        false
    )

    furniture.SetAnimationState("idle")
    j.RegisterJobWorkedCallback("CloningPod_JobRunning")
    j.RegisterJobCompletedCallback("CloningPod_JobComplete")
	j.JobDescription = "job_cloning_pod_cloning_desc"
    furniture.AddJob(j)
end

function CloningPod_JobRunning(j)
    j.furniture.SetAnimationState("running")
end

function CloningPod_JobComplete(j)
    World.Current.CreateCharacter(j.furniture.GetSpawnSpotTile())
    j.furniture.Deconstruct()
end

function PowerGenerator_UpdateAction(furniture, deltatime)
    if (furniture.JobCount() < 1 and furniture.Parameters["burnTime"].ToFloat() == 0) then
        furniture.PowerConnection.OutputRate = 0
        local itemsDesired = {Inventory.__new("Uranium", 0, 5)}

        local j = Job.__new(
            furniture.GetJobSpotTile(),
            nil,
            nil,
            0.5,
            itemsDesired,
            Job.JobPriority.High,
            false
        )

        j.RegisterJobCompletedCallback("PowerGenerator_JobComplete")
        j.JobDescription = "job_power_generator_fulling_desc"
        furniture.AddJob( j )
    else
        furniture.Parameters["burnTime"].ChangeFloatValue(-deltatime)
        if ( furniture.Parameters["burnTime"].ToFloat() < 0 ) then
            furniture.Parameters["burnTime"].SetValue(0)
        end        
    end
    if (furniture.Parameters["burnTime"].ToFloat() == 0) then
        furniture.SetAnimationState("idle")
    else
        furniture.SetAnimationState("running")
    end
end

function PowerGenerator_JobComplete( j )
    j.furniture.Parameters["burnTime"].SetValue(j.furniture.Parameters["burnTimeRequired"].ToFloat())
    j.furniture.PowerConnection.OutputRate = 5
end

function LandingPad_Test_CallTradeShip(furniture, character)
   WorldController.Instance.TradeController.CallTradeShipTest(furniture)
end

-- This function gets called once, when the furniture is installed
function Heater_InstallAction( furniture, deltaTime)
    -- TODO: find elegant way to register heat source and sinks to Temperature
	furniture.EventActions.Register("OnUpdateTemperature", "Heater_UpdateTemperature")
	World.Current.temperature.RegisterSinkOrSource(furniture)
end

-- This function gets called once, when the furniture is uninstalled
function Heater_UninstallAction( furniture, deltaTime)
    furniture.EventActions.Deregister("OnUpdateTemperature", "Heater_UpdateTemperature")
	World.Current.temperature.DeregisterSinkOrSource(furniture)
	-- TODO: find elegant way to unregister previous register
end

function Heater_UpdateTemperature( furniture, deltaTime)
    if (furniture.tile.Room.IsOutsideRoom() == true) then
        return
    end
    
    tile = furniture.tile
    pressure = tile.Room.GetGasPressure() / tile.Room.GetSize()
    efficiency = ModUtils.Clamp01(pressure / furniture.Parameters["pressure_threshold"].ToFloat())
    temperatureChangePerSecond = furniture.Parameters["base_heating"].ToFloat() * efficiency
    temperatureChange = temperatureChangePerSecond * deltaTime
    
    World.Current.temperature.ChangeTemperature(tile.X, tile.Y, temperatureChange)
    --ModUtils.ULogChannel("Temperature", "Heat change: " .. temperatureChangePerSecond .. " => " .. World.current.temperature.GetTemperature(tile.X, tile.Y))
end

-- Should maybe later be integrated with GasGenerator function by
-- someone who knows how that would work in this case
function OxygenCompressor_OnUpdate(furniture, deltaTime)
    local room = furniture.Tile.Room
    local pressure = room.GetGasPressure("O2")
    local gasAmount = furniture.Parameters["flow_rate"].ToFloat() * deltaTime
    
    if (pressure < furniture.Parameters["give_threshold"].ToFloat()) then
        -- Expel gas if available
        if (furniture.Parameters["gas_content"].ToFloat() > 0) then
            furniture.Parameters["gas_content"].ChangeFloatValue(-gasAmount)
            room.ChangeGas("O2", gasAmount / room.GetSize())
            furniture.UpdateOnChanged(furniture)
        end
    elseif (pressure > furniture.Parameters["take_threshold"].ToFloat()) then
        -- Suck in gas if not full
        if (furniture.Parameters["gas_content"].ToFloat() < furniture.Parameters["max_gas_content"].ToFloat()) then
            furniture.Parameters["gas_content"].ChangeFloatValue(gasAmount)
            room.ChangeGas("O2", -gasAmount / room.GetSize())
            furniture.UpdateOnChanged(furniture)
        end
    end
end

function OxygenCompressor_GetSpriteName(furniture)
    local baseName = furniture.Type
    local suffix = 0
    if (furniture.Parameters["gas_content"].ToFloat() > 0) then
        idxAsFloat = 8 * (furniture.Parameters["gas_content"].ToFloat() / furniture.Parameters["max_gas_content"].ToFloat())
        suffix = ModUtils.FloorToInt(idxAsFloat)
    end
    return baseName .. "_" .. suffix
end

function SolarPanel_OnUpdate(furniture, deltaTime)
    local baseOutput = furniture.Parameters["base_output"].ToFloat()
    local efficiency = furniture.Parameters["efficiency"].ToFloat()
    local powerPerSecond = baseOutput * efficiency
	furniture.PowerConnection.OutputRate = powerPerSecond
end

function AirPump_OnUpdate(furniture, deltaTime)
    if (furniture.HasPower() == false) then
        return
    end

    local t = furniture.Tile
    local north = World.Current.GetTileAt(t.X, t.Y + 1, t.Z)
    local south = World.Current.GetTileAt(t.X, t.Y - 1, t.Z)
    local west = World.Current.GetTileAt(t.X - 1, t.Y, t.Z)
    local east = World.Current.GetTileAt(t.X + 1, t.Y, t.Z)
    
    -- Find the correct rooms for source and target
    -- Maybe in future this could be cached. it only changes when the direction changes
    local sourceRoom = nil
    local targetRoom = nil
    if (north.Room != nil and south.Room != nil) then
        if (furniture.Parameters["flow_direction_up"].ToFloat() > 0) then
            sourceRoom = south.Room
            targetRoom = north.Room
        else
            sourceRoom = north.Room
            targetRoom = south.Room
        end
    elseif (west.Room != nil and east.Room != nil) then
        if (furniture.Parameters["flow_direction_up"].ToFloat() > 0) then
            sourceRoom = west.Room
            targetRoom = east.Room
        else
            sourceRoom = east.Room
            targetRoom = west.Room
        end
    else
        ModUtils.UChannelLogWarning("Furniture", "Air Pump blocked. Direction unclear")
        return
    end
    
    local sourcePressureLimit = furniture.Parameters["source_pressure_limit"].ToFloat()
    local targetPressureLimit = furniture.Parameters["target_pressure_limit"].ToFloat()
    local flow = furniture.Parameters["gas_throughput"].ToFloat() * deltaTime
    
    -- Only transfer gas if the pressures are within the defined bounds
    if (sourceRoom.GetTotalGasPressure() > sourcePressureLimit and targetRoom.GetTotalGasPressure() < targetPressureLimit) then
        sourceRoom.MoveGasTo(targetRoom, flow)
    end
end

function AirPump_GetSpriteName(furniture)
    local t = furniture.Tile
    if (furniture.Tile == nil) then
        return furniture.Type
    end
    local north = World.Current.GetTileAt(t.X, t.Y + 1, t.Z)
    local south = World.Current.GetTileAt(t.X, t.Y - 1, t.Z)
    local west = World.Current.GetTileAt(t.X - 1, t.Y, t.Z)
    local east = World.Current.GetTileAt(t.X + 1, t.Y, t.Z)
    
    suffix = ""
    if (north.Room != nil and south.Room != nil) then
        if (furniture.Parameters["flow_direction_up"].ToFloat() > 0) then
           suffix = "_SN"
        else
           suffix = "_NS"
        end
    elseif (west.Room != nil and east.Room != nil) then
        if (furniture.Parameters["flow_direction_up"].ToFloat() > 0) then
            suffix = "_WE"
        else
            suffix = "_EW"
        end
    end
    
    return furniture.Type .. suffix
end

function AirPump_FlipDirection(furniture, character)
    if (furniture.Parameters["flow_direction_up"].ToFloat() > 0) then
        furniture.Parameters["flow_direction_up"].SetValue(0)
    else
        furniture.Parameters["flow_direction_up"].SetValue(1)
    end
    furniture.UpdateOnChanged(furniture)
end
    
function Accumulator_GetSpriteName(furniture)
	local baseName = furniture.Type
	local suffix = furniture.PowerConnection.CurrentThreshold 
	return baseName .. "_" .. suffix
end

function OreMine_CreateMiningJob(furniture, character)
    -- Creates job for a character to go and "mine" the Ore
    local j = Job.__new(
		furniture.Tile,
		nil,
		nil,
		0,
		nil,
		Job.JobPriority.High,
		false
	)

    j.RegisterJobWorkedCallback("OreMine_OreMined")
    furniture.AddJob(j)
    ModUtils.ULog("Ore Mine - Mining Job Created")
end

function OreMine_OreMined(job)
    -- Defines the ore to be spawned by the mine
    local inventory = Inventory.__new(job.furniture.Parameters["ore_type"], 10)

    -- Place the "mined" ore on the tile
    World.Current.inventoryManager.PlaceInventory(job.tile, inventory)

    -- Deconstruct the ore mine
    job.furniture.Deconstruct()
    job.CancelJob()
end

function OreMine_GetSpriteName(furniture)
    return "mine_" .. furniture.Parameters["ore_type"].ToString()
end

-- This function gets called once, when the furniture is installed
function Rtg_InstallAction( furniture, deltaTime)
    -- TODO: find elegant way to register heat source and sinks to Temperature
	furniture.EventActions.Register("OnUpdateTemperature", "Rtg_UpdateTemperature")
	World.Current.temperature.RegisterSinkOrSource(furniture)
end

-- This function gets called once, when the furniture is uninstalled
function Rtg_UninstallAction( furniture, deltaTime)
    furniture.EventActions.Deregister("OnUpdateTemperature", "Rtg_UpdateTemperature")
	World.Current.temperature.DeregisterSinkOrSource(furniture)
	-- TODO: find elegant way to unregister previous register
end

function Rtg_UpdateTemperature( furniture, deltaTime)
    if (furniture.tile.Room.IsOutsideRoom() == true) then
        return
    end
    
    tile = furniture.tile
    pressure = tile.Room.GetGasPressure() / tile.Room.GetSize()
    efficiency = ModUtils.Clamp01(pressure / furniture.Parameters["pressure_threshold"].ToFloat())
    temperatureChangePerSecond = furniture.Parameters["base_heating"].ToFloat() * efficiency
    temperatureChange = temperatureChangePerSecond * deltaTime
    
    World.Current.temperature.ChangeTemperature(tile.X, tile.Y, temperatureChange)
    --ModUtils.ULogChannel("Temperature", "Heat change: " .. temperatureChangePerSecond .. " => " .. World.current.temperature.GetTemperature(tile.X, tile.Y))
end

ModUtils.ULog("Furniture.lua loaded")
return "LUA Script Parsed!"
