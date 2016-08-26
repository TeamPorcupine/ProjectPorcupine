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
    if (furniture.HasPower() == false) then
        return
    end

	if ( furniture.tile.room == nil ) then
		return "Furniture's room was null."
	end

    local keys = furniture.Parameters["gas_gen"].Keys()
    for discard, key in pairs(keys) do
        if ( furniture.tile.room.GetGasPressure(key) < furniture.Parameters["gas_gen"][key]["gas_limit"].ToFloat()) then
            furniture.tile.room.ChangeGas(key, furniture.Parameters["gas_per_second"].ToFloat() * deltaTime * furniture.Parameters["gas_gen"][key]["gas_limit"].ToFloat())
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

function OnUpdate_Leak_Door( furniture, deltaTime )
	furniture.tile.EqualiseGas(deltaTime * 10.0 * (furniture.Parameters["openness"].ToFloat() + 0.1))
end

function OnUpdate_Leak_Airlock( furniture, deltaTime )
	furniture.tile.EqualiseGas(deltaTime * 10.0 * (furniture.Parameters["openness"].ToFloat()))
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

    if( furniture.tile.Inventory != nil and furniture.tile.Inventory.stackSize >= furniture.tile.Inventory.maxStackSize ) then
        -- We are full!
        furniture.CancelJobs()
        return
    end

    -- Maybe we already have a job queued up?
    if( furniture.JobCount() > 0 ) then
        -- Cool, all done.
        return
    end

    -- We currently are NOT full, but we don't have a job either.
    -- Two possibilities: Either we have SOME inventory, or we have NO inventory.

    -- Third possibility: Something is WHACK
    if( furniture.tile.Inventory != nil and furniture.tile.Inventory.stackSize == 0 ) then
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

	if( furniture.tile.Inventory == nil ) then
		--ModUtils.ULog("Creating job for new stack.")
		itemsDesired = Stockpile_GetItemsFromFilter( furniture )
	else
		--ModUtils.ULog("Creating job for existing stack.")
		desInv = furniture.tile.Inventory.Clone()
		desInv.maxStackSize = desInv.maxStackSize - desInv.stackSize
		desInv.stackSize = 0

        itemsDesired = { desInv }
    end

	local j = Job.__new(
		furniture.tile,
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
        if(inv.stackSize > 0) then
            World.current.inventoryManager.PlaceInventory(j.tile, inv)
            return -- There should be no way that we ever end up with more than on inventory requirement with stackSize > 0
        end
    end
end

function MiningDroneStation_UpdateAction( furniture, deltaTime )
    local spawnSpot = furniture.GetSpawnSpotTile()

	if( furniture.JobCount() > 0 ) then
		-- Check to see if the Metal Plate destination tile is full.
		if( spawnSpot.Inventory != nil and spawnSpot.Inventory.stackSize >= spawnSpot.Inventory.maxStackSize ) then
			-- We should stop this job, because it's impossible to make any more items.
			furniture.CancelJobs()
		end
		return
	end

	-- If we get here, then we have no current job. Check to see if our destination is full.
	if( spawnSpot.Inventory != nil and spawnSpot.Inventory.stackSize >= spawnSpot.Inventory.maxStackSize ) then
		-- We are full! Don't make a job!
		return
	end

	if(furniture.GetSpawnSpotTile().Inventory != nil and furniture.GetSpawnSpotTile().Inventory.objectType != furniture.Parameters["mine_type"].ToString()) then
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
	if (j.furniture.GetSpawnSpotTile().Inventory == nil or j.furniture.GetSpawnSpotTile().Inventory.objectType == j.furniture.Parameters["mine_type"].ToString()) then
		World.current.inventoryManager.PlaceInventory( j.furniture.GetSpawnSpotTile(), Inventory.__new(j.furniture.Parameters["mine_type"].ToString() , 50, 20) )
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

    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.stackSize >= 5) then
        furniture.Parameters["smelttime"].ChangeFloatValue(deltaTime)
        if(furniture.Parameters["smelttime"].ToFloat() >= furniture.Parameters["smelttime_required"].ToFloat()) then
            furniture.Parameters["smelttime"].SetValue(0)
            local outputSpot = World.current.GetTileAt(spawnSpot.X+2, spawnSpot.y)

            if(outputSpot.Inventory == nil) then
                World.current.inventoryManager.PlaceInventory(outputSpot, Inventory.__new("Steel Plate", 50, 5))
                spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize - 5
            else
                if(outputSpot.Inventory.stackSize <= 45) then
                    outputSpot.Inventory.stackSize = outputSpot.Inventory.stackSize + 5
                    spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize - 5
                end
            end

            if(spawnSpot.Inventory.stackSize <= 0) then
                spawnSpot.Inventory = nil
            end
        end
    end

    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.stackSize == spawnSpot.Inventory.maxStackSize) then
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
    local itemsDesired = { Inventory.__new("Raw Iron", desiredStackSize, 0) }
    if(spawnSpot.Inventory ~= nil and spawnSpot.Inventory.stackSize < spawnSpot.Inventory.maxStackSize) then
        desiredStackSize = spawnSpot.Inventory.maxStackSize - spawnSpot.Inventory.stackSize
        itemsDesired.maxStackSize = desiredStackSize
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
        if(inv ~= nil and inv.stackSize > 0) then
            World.current.inventoryManager.PlaceInventory(spawnSpot, inv)
            spawnSpot.Inventory.isLocked = true
            return
        end
    end
end

function PowerCellPress_UpdateAction(furniture, deltaTime)
    local spawnSpot = furniture.GetSpawnSpotTile()

    if(spawnSpot.Inventory == nil) then
        if(furniture.JobCount() == 0) then
            local itemsDesired = {Inventory.__new("Steel Plate", 10, 0)}
            local jobSpot = furniture.GetJobSpotTile()

            local j = Job.__new(
                jobSpot,
                nil,
                nil,
                1,
                itemsDesired,
                Job.JobPriority.Medium,
                false
            )

            j.RegisterJobCompletedCallback("PowerCellPress_JobComplete")
            j.JobDescription = "job_power_cell_fulling_desc"
            furniture.AddJob(j)
        end
    else
        furniture.Parameters["presstime"].ChangeFloatValue(deltaTime)

        if(furniture.Parameters["presstime"].ToFloat() >= furniture.Parameters["presstime_required"].ToFloat()) then
            furniture.Parameters["presstime"].SetValue(0)
            local outputSpot = World.current.GetTileAt(spawnSpot.X+2, spawnSpot.y)

            if(outputSpot.Inventory == nil) then
                World.current.inventoryManager.PlaceInventory( outputSpot, Inventory.__new("Power Cell", 5, 1) )
                spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize-10
            else
                if(outputSpot.Inventory.stackSize <= 4) then
                    outputSpot.Inventory.stackSize = outputSpot.Inventory.stackSize+1
                    spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize-10
                end
            end

            if(spawnSpot.Inventory.stackSize <= 0) then
                spawnSpot.Inventory = nil
            end
        end
    end
end

function PowerCellPress_JobComplete(j)
    local spawnSpot = j.tile.Furniture.GetSpawnSpotTile()

    for k, inv in pairs(j.inventoryRequirements) do
        if(inv.stackSize > 0) then
            World.current.inventoryManager.PlaceInventory(spawnSpot, inv)
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

    j.RegisterJobCompletedCallback("CloningPod_JobComplete")
	j.JobDescription = "job_cloning_pod_cloning_desc"
    furniture.AddJob(j)
end

function CloningPod_JobComplete(j)
    World.current.CreateCharacter(j.furniture.GetSpawnSpotTile())
    j.furniture.Deconstruct()
end

function PowerGenerator_UpdateAction(furniture, deltatime)
    if (furniture.JobCount() < 1 and furniture.Parameters["burnTime"].ToFloat() == 0) then
        furniture.PowerValue = 0
        local itemsDesired = {Inventory.__new("Uranium", 5, 0)}

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
end

function PowerGenerator_JobComplete( j )
    j.furniture.Parameters["burnTime"].SetValue(j.furniture.Parameters["burnTimeRequired"].ToFloat())
    j.furniture.PowerValue = 5
end

function LandingPad_Temp_UpdateAction(furniture, deltaTime)
    if(not furniture.tile.room.IsOutsideRoom()) then
        return
    end

    local spawnSpot = furniture.GetSpawnSpotTile()
    local jobSpot = furniture.GetJobSpotTile()
    local inputSpot = World.current.GetTileAt(jobSpot.X, jobSpot.y-1)

    if(inputSpot.Inventory == nil) then
        if(furniture.JobCount() == 0) then
            local itemsDesired = {Inventory.__new("Steel Plate", furniture.Parameters["tradeinamount"].ToFloat())}

            local j = Job.__new(
                inputSpot,
                nil,
                nil,
                0.4,
                itemsDesired,
                Job.JobPriority.Medium,
                false
            )

            j.furniture = furniture
            j.RegisterJobCompletedCallback("LandingPad_Temp_JobComplete")
			j.JobDescription = "job_landing_pad_fulling_desc"
            furniture.AddJob(j)
        end
    else
        furniture.Parameters["tradetime"].ChangeFloatValue(deltaTime)

		if(furniture.Parameters["tradetime"].ToFloat() >= furniture.Parameters["tradetime_required"].ToFloat()) then
			furniture.Parameters["tradetime"].SetValue(0)
            local outputSpot = World.current.GetTileAt(spawnSpot.X+1, spawnSpot.y)

            if(outputSpot.Inventory == nil) then
                World.current.inventoryManager.PlaceInventory( outputSpot, Inventory.__new("Steel Plate", 50, furniture.Parameters["tradeoutamount"].ToFloat()))
                inputSpot.Inventory.stackSize = inputSpot.Inventory.stackSize-furniture.Parameters["tradeinamount"].ToFloat()
            else
                if(outputSpot.Inventory.stackSize <= 50 - outputSpot.Inventory.stackSize + furniture.Parameters["tradeoutamount"].ToFloat()) then
                    outputSpot.Inventory.stackSize = outputSpot.Inventory.stackSize + furniture.Parameters["tradeoutamount"].ToFloat()
                    inputSpot.Inventory.stackSize = inputSpot.Inventory.stackSize - furniture.Parameters["tradeoutamount"].ToFloat()
                end
            end

            if(inputSpot.Inventory.stackSize <= 0) then
                inputSpot.Inventory = nil
            end
        end
    end
end

function LandingPad_Temp_JobComplete(j)
    local jobSpot = j.furniture.GetJobSpotTile()
    local inputSpot = World.current.GetTileAt(jobSpot.X, jobSpot.y-1)

    for k, inv in pairs(j.inventoryRequirements) do
        if(inv.stackSize > 0) then
            World.current.inventoryManager.PlaceInventory(inputSpot, inv)
            inputSpot.Inventory.isLocked = true
            return
        end
    end
end

function LandingPad_Test_ContextMenuAction(furniture, character)
   furniture.Deconstruct()
end

-- This function gets called once, when the funriture is isntalled
function Heater_InstallAction( furniture, deltaTime)
    -- TODO: find elegant way to register heat source and sinks to Temperature
	furniture.eventActions.Register("OnUpdateTemperature", "Heater_UpdateTemperature")
	World.current.temperature.RegisterSinkOrSource(furniture)
end

-- This function gets called once, when the funriture is unisntalled
function Heater_UninstallAction( furniture, deltaTime)
    -- TODO: find elegant way to unregister previous register
	furniture.eventActions.Deregister("OnUpdateTemperature", "Heater_UpdateTemperature")
	World.current.temperature.DeregisterSinkOrSource(furniture)
end

-- Dummy heater uninstall function
-- THis function gets called once, when the funriture is unisntalled
function Heater_UpdateTemperature( furniture, deltaTime)
	World.current.temperature.SetTemperature(furniture.tile.X, furniture.tile.Y, 300)
end

-- Should maybe later be integrated with GasGenerator function by
-- someone who knows how that would work in this case
function OxygenCompressor_OnUpdate(furniture, deltaTime)
    local room = furniture.tile.Room
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
    local baseName = furniture.objectType
    local suffix = 0
    if (furniture.Parameters["gas_content"].ToFloat() > 0) then
        idxAsFloat = 8 * (furniture.Parameters["gas_content"].ToFloat() / furniture.Parameters["max_gas_content"].ToFloat())
        suffix = ModUtils.FloorToInt(idxAsFloat)
    end
    return baseName .. "_" .. suffix
end

ModUtils.ULog("Furniture.lua loaded")
return "LUA Script Parsed!"
