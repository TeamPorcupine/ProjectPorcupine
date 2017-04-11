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

function OnUpdate_Door( furniture, deltaTime )
	if (furniture.Parameters["is_opening"].ToFloat() >= 1.0) then
		furniture.Parameters["openness"].ChangeFloatValue(deltaTime * 4) -- FIXME: Maybe a door open speed parameter?
		if (furniture.Parameters["openness"].ToFloat() >= 1)  then
			furniture.Parameters["is_opening"].SetValue(0)
		end
	elseif (furniture.Parameters["openness"].ToFloat() > 0.0) then
        furniture.Parameters["openness"].ChangeFloatValue(deltaTime * -4)
	end

	furniture.Parameters["openness"].SetValue( ModUtils.Clamp01(furniture.Parameters["openness"].ToFloat()) )

	if (furniture.verticalDoor == true) then
		furniture.SetAnimationState("vertical")
	else
		furniture.SetAnimationState("horizontal")
	end
    furniture.SetAnimationProgressValue(furniture.Parameters["openness"].ToFloat(), 1)
end

function OnUpdate_AirlockDoor( furniture, deltaTime )
    OnUpdate_Door(furniture, deltaTime)
    return
end

function IsEnterable_AirlockDoor( furniture )
    -- If we're not pressure locked we ignore everything else, and act like a normal door
    if (furniture.Parameters["pressure_locked"].ToBool() == false) then
        
        furniture.Parameters["is_opening"].SetValue(1)

        if (furniture.Parameters["openness"].ToFloat() >= 1) then
            return ENTERABILITY_YES --ENTERABILITY.Yes
        end
        
        return ENTERABILITY_SOON --ENTERABILITY.Soon
    else
        local tolerance = 0.005
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
        -- Pressure locked but not controlled by an airlock we only open 
        if(furniture.Parameters["airlock_controlled"].ToBool() == false) then
            if (ModUtils.Round(adjacentRooms[1].GetGasPressure(),3) == ModUtils.Round(adjacentRooms[2].GetGasPressure(),3)) then
                furniture.Parameters["is_opening"].SetValue(1)
                return ENTERABILITY_SOON
            else
            -- I don't think responding with no here actually makes a difference, but let's make the door close immediately just in case
                furniture.Parameters["is_opening"].SetValue(0)
                return ENTERABILITY_NO
            end
        else
            if (adjacentRooms[1].HasRoomBehavior("roombehavior_airlock") or adjacentRooms[2].HasRoomBehavior("roombehavior_airlock")) then
                -- Figure out what's inside and what's outside.
                local insideRoom
                local outsideRoom
                if(adjacentRooms[1].HasRoomBehavior("roombehavior_airlock")) then
                    insideRoom = adjacentRooms[1]
                    outsideRoom = adjacentRooms[2]
                else
                    insideRoom = adjacentRooms[2]
                    outsideRoom = adjacentRooms[1]
                end
                -- Pressure's different, pump to equalize
                if(math.abs(ModUtils.Round(insideRoom.GetGasPressure(),3) - ModUtils.Round(outsideRoom.GetGasPressure(),3)) > tolerance) then
                    if (insideRoom.GetGasPressure() < outsideRoom.GetGasPressure()) then
                        insideRoom.RoomBehaviors["roombehavior_airlock"].CallEventAction("PumpIn",  outsideRoom.GetGasPressure())
                    else
                        insideRoom.RoomBehaviors["roombehavior_airlock"].CallEventAction("PumpOut", outsideRoom.GetGasPressure())
                    end
                    return ENTERABILITY_SOON
                else
                    if (furniture.Parameters["openness"].ToFloat() >= 1) then
                        -- We're fully open deactivate pumps and let the room know we're done pumping
                        insideRoom.RoomBehaviors["roombehavior_airlock"].CallEventAction("PumpOff")
                        return ENTERABILITY_YES --ENTERABILITY.Yes
                    end
                    furniture.Parameters["is_opening"].SetValue(1)
                    return ENTERABILITY_SOON --ENTERABILITY.Soon
                end
            end
        end
    end
end


function AirlockDoor_Toggle_Pressure_Lock(furniture, character)

    ModUtils.ULog("Toggling Pressure Lock")
    
	furniture.Parameters["pressure_locked"].SetValue(not furniture.Parameters["pressure_locked"].ToBool())
    
    ModUtils.ULog(furniture.Parameters["pressure_locked"].ToBool())
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

function Stockpile_SetNewFilter( furniture, character )
    WorldController.Instance.dialogBoxManager.ShowDialogBoxByName("Filter")
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
        furniture.Jobs.CancelAll()
        return
    end

    -- Maybe we already have a job queued up?
    if( furniture.Jobs.Count > 0 ) then
        -- Cool, all done.
        return
    end

    -- We Currently are NOT full, but we don't have a job either.
    -- Two possibilities: Either we have SOME inventory, or we have NO inventory.

    -- Third possibility: Something is WHACK
    if( furniture.Tile.Inventory != nil and furniture.Tile.Inventory.StackSize == 0 ) then
        furniture.Jobs.CancelAll()
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
		local inventory = furniture.Tile.Inventory
		local item = RequestedItem.__new(inventory.Type, 1, inventory.MaxStackSize - inventory.StackSize)
        itemsDesired = { item }
    end

  	local job = Job.__new(
    		furniture.Tile,
    		"Stockpile_UpdateAction",
    		nil,
    		0,
    		itemsDesired,
    		Job.JobPriority.Low,
    		false
  	)
  	job.Description = "job_stockpile_moving_desc"
  	job.acceptsAny = true

  	-- TODO: Later on, add stockpile priorities, so that we can take from a lower
  	-- priority stockpile for a higher priority one.
  	job.canTakeFromStockpile = false

  	job.RegisterJobWorkedCallback("Stockpile_JobWorked")
  	furniture.Jobs.Add(job)
end

function Stockpile_JobWorked(job)
    job.CancelJob()

    -- TODO: Change this when we figure out what we're doing for the all/any pickup job.
    --values = job.GetInventoryRequirementValues();
    for k, inv in pairs(job.DeliveredItems) do
        if(inv.StackSize > 0) then
            World.Current.inventoryManager.PlaceInventory(job.tile, inv)
            return -- There should be no way that we ever end up with more than on inventory requirement with StackSize > 0
        end
    end
end

function MiningDroneStation_UpdateAction( furniture, deltaTime )
    local outputSpot = furniture.Jobs.OutputSpotTile

	if (furniture.Jobs.Count > 0) then
		-- Check to see if the Metal Plate destination tile is full.
		if (outputSpot.Inventory != nil and outputSpot.Inventory.StackSize >= outputSpot.Inventory.MaxStackSize) then
			-- We should stop this job, because it's impossible to make any more items.
			furniture.Jobs.CancelAll()
		end
		return
	end

	-- If we get here, then we have no Current job. Check to see if our destination is full.
	if (outputSpot.Inventory != nil and outputSpot.Inventory.StackSize >= outputSpot.Inventory.MaxStackSize) then
		-- We are full! Don't make a job!
		return
	end

	if (outputSpot.Inventory != nil and outputSpot.Inventory.Type != furniture.Parameters["mine_type"].ToString()) then
		return
	end

	-- If we get here, we need to CREATE a new job.
	local job = Job.__new(
		furniture.Jobs.WorkSpotTile,
		"MiningDroneStation_UpdateAction",
		nil,
		1,
		nil,
		Job.JobPriority.Medium,
		true	-- This job repeats until the destination tile is full.
	)

	job.RegisterJobCompletedCallback("MiningDroneStation_JobComplete")
	job.Description = "job_mining_drone_station_mining_desc"
	furniture.Jobs.Add(job)
end

function MiningDroneStation_JobComplete(job)
	if (job.buildable.Jobs.OutputSpotTile.Inventory == nil or job.buildable.Jobs.OutputSpotTile.Inventory.Type == job.buildable.Parameters["mine_type"].ToString()) then
		World.Current.inventoryManager.PlaceInventory(job.buildable.Jobs.OutputSpotTile, Inventory.__new(job.buildable.Parameters["mine_type"].ToString(), 2))
	else
		job.CancelJob()
	end
end

function MiningDroneStation_Change_to_Raw_Iron(furniture, character)
	furniture.Parameters["mine_type"].SetValue("Raw Iron")
end

function MiningDroneStation_Change_to_Raw_Copper(furniture, character)
	furniture.Parameters["mine_type"].SetValue("Raw Copper")
end

function MetalSmelter_UpdateAction(furniture, deltaTime)
    local inputSpot = furniture.Jobs.InputSpotTile
    local outputSpot = furniture.Jobs.OutputSpotTile

    if (inputSpot.Inventory ~= nil and inputSpot.Inventory.StackSize >= 5) then
        furniture.Parameters["smelttime"].ChangeFloatValue(deltaTime)
        if (furniture.Parameters["smelttime"].ToFloat() >= furniture.Parameters["smelttime_required"].ToFloat()) then
            furniture.Parameters["smelttime"].SetValue(0)

            if (outputSpot.Inventory == nil) then
                World.Current.inventoryManager.PlaceInventory(outputSpot, Inventory.__new("Steel Plate", 5))
                inputSpot.Inventory.StackSize = inputSpot.Inventory.StackSize - 5

            elseif (outputSpot.Inventory.StackSize <= outputSpot.Inventory.MaxStackSize - 5) then
                outputSpot.Inventory.StackSize = outputSpot.Inventory.StackSize + 5
                inputSpot.Inventory.StackSize = inputSpot.Inventory.StackSize - 5
            end

            if (inputSpot.Inventory.StackSize <= 0) then
                inputSpot.Inventory = nil
            end
        end
    end

    if (inputSpot.Inventory ~= nil and inputSpot.Inventory.StackSize == inputSpot.Inventory.MaxStackSize) then
        -- We have the max amount of resources, cancel the job.
        -- This check exists mainly, because the job completed callback doesn't
        -- seem to be reliable.
        furniture.Jobs.CancelAll()
        return
    end

    if (furniture.Jobs.Count > 0) then
        return
    end

    -- Create job depending on the already available stack size.
    local desiredStackSize = 50
    if(inputSpot.Inventory ~= nil and inputSpot.Inventory.StackSize < inputSpot.Inventory.MaxStackSize) then
        desiredStackSize = inputSpot.Inventory.MaxStackSize - inputSpot.Inventory.StackSize
    end
    local itemsDesired = { RequestedItem.__new("Raw Iron", desiredStackSize) }
    ModUtils.ULog("MetalSmelter: Creating job for " .. desiredStackSize .. " raw iron.")

    local job = Job.__new(
        furniture.Jobs.WorkSpotTile,
        "MetalSmelter_UpdateAction",
        nil,
        0.4,
        itemsDesired,
        Job.JobPriority.Medium,
        false
    )

    job.RegisterJobWorkedCallback("MetalSmelter_JobWorked")
    furniture.Jobs.Add(job)
    return
end

function MetalSmelter_JobWorked(job)
    job.CancelJob()
    local inputSpot = job.tile.Furniture.Jobs.InputSpotTile
    for k, inv in pairs(job.DeliveredItems) do
        if(inv ~= nil and inv.StackSize > 0) then
            World.Current.inventoryManager.PlaceInventory(inputSpot, inv)
            inputSpot.Inventory.Locked = true
            return
        end
    end
end

function CloningPod_UpdateAction(furniture, deltaTime)

    if (furniture.Jobs.Count > 0) then
        return
    end

    local job = Job.__new(
        furniture.Jobs.WorkSpotTile,
        "CloningPod_UpdateAction",
        nil,
        10,
        nil,
        Job.JobPriority.Medium,
        false
    )

    furniture.SetAnimationState("idle")
    job.RegisterJobWorkedCallback("CloningPod_JobRunning")
    job.RegisterJobCompletedCallback("CloningPod_JobComplete")
	job.Description = "job_cloning_pod_cloning_desc"
    furniture.Jobs.Add(job)
end

function CloningPod_JobRunning(job)
    job.buildable.SetAnimationState("running")
end

function CloningPod_JobComplete(job)
    World.Current.CharacterManager.Create(job.buildable.Jobs.OutputSpotTile)
    job.buildable.Deconstruct()
end

function PowerGenerator_FuelInfo(furniture)
    local curBurn = furniture.Parameters["cur_processing_time"].ToFloat()
	local maxBurn = furniture.Parameters["max_processing_time"].ToFloat()
	local invProc = furniture.Parameters["cur_processed_inv"].ToInt()

	local perc = 0
	if (maxBurn != 0 and invProc > 0) then
		perc = 100 - (curBurn * 100 / maxBurn)
		if (perc <= 0) then
			perc = 0
		end
	end

    return "Fuel: " .. string.format("%.1f", perc) .. "%"
end

function LandingPad_Test_CallTradeShip(furniture, character)
   WorldController.Instance.TradeController.CallTradeShipTest(furniture)
end

-- This function gets called once, when the furniture is installed
function Heater_InstallAction( furniture, deltaTime)
    -- TODO: find elegant way to register heat source and sinks to Temperature
	World.Current.temperature.RegisterSinkOrSource(furniture)
end

-- This function gets called once, when the furniture is uninstalled
function Heater_UninstallAction( furniture, deltaTime)
	World.Current.temperature.DeregisterSinkOrSource(furniture)
	-- TODO: find elegant way to unregister previous register
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
            room.Atmosphere.CreateGas("O2", gasAmount / room.TileCount, 0.0)
            furniture.UpdateOnChanged(furniture)
        end
    elseif (pressure > furniture.Parameters["take_threshold"].ToFloat()) then
        -- Suck in gas if not full
        if (furniture.Parameters["gas_content"].ToFloat() < furniture.Parameters["max_gas_content"].ToFloat()) then
            furniture.Parameters["gas_content"].ChangeFloatValue(gasAmount)
            room.Atmosphere.DestroyGas("O2", gasAmount / room.TileCount)
            furniture.UpdateOnChanged(furniture)
        end
    end
    furniture.SetAnimationState("running")
    furniture.SetAnimationProgressValue(furniture.Parameters["gas_content"].ToFloat(), furniture.Parameters["max_gas_content"].ToFloat());
end

function OxygenCompressor_OnPowerOff(furniture, deltaTime)
    -- lose half of gas, in case of blackout
	local gasContent = furniture.Parameters["gas_content"].ToFloat()
	if (gasContent > 0) then
            furniture.Parameters["gas_content"].ChangeFloatValue(-gasContent/2)
            furniture.UpdateOnChanged(furniture)
    end
    furniture.SetAnimationProgressValue(furniture.Parameters["gas_content"].ToFloat(), furniture.Parameters["max_gas_content"].ToFloat());
end

function SolarPanel_OnUpdate(furniture, deltaTime)
    local baseOutput = furniture.Parameters["base_output"].ToFloat()
    local efficiency = furniture.Parameters["efficiency"].ToFloat()
    local powerPerSecond = baseOutput * efficiency
	furniture.PowerConnection.OutputRate = powerPerSecond
end

function AirPump_OnUpdate(furniture, deltaTime)
    if (furniture.IsOperating == false) then
        return
    end

    if (furniture.Parameters["active"].ToBool()) then
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
            ModUtils.ULogWarningChannel("Furniture", "Air Pump blocked. Direction unclear")
            return
        end
        
        local sourcePressureLimit = furniture.Parameters["source_pressure_limit"].ToFloat()
        local targetPressureLimit = furniture.Parameters["target_pressure_limit"].ToFloat()
        local flow = furniture.Parameters["gas_throughput"].ToFloat() * deltaTime
        
        -- Only transfer gas if the pressures are within the defined bounds
        if (sourceRoom.GetGasPressure() > sourcePressureLimit and targetRoom.GetGasPressure() < targetPressureLimit) then
            sourceRoom.Atmosphere.MoveGasTo(targetRoom.Atmosphere, flow)
        end
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

function Vent_OnUpdate(furniture, deltaTime)
    furniture.SetAnimationProgressValue(furniture.Parameters["openness"].ToFloat(), 1)
    furniture.Tile.EqualiseGas(deltaTime * furniture.Parameters["gas_throughput"].ToFloat() * furniture.Parameters["openness"].ToInt())
end

function Vent_SetOrientationState(furniture)
    if (furniture.Tile == nil) then
        return
    end
    
    local tile = furniture.Tile
    if (tile.North().Room != nil and tile.South().Room != nil) then
        furniture.SetAnimationState("vertical")
    elseif (tile.West().Room != nil and tile.East().Room != nil) then
        furniture.SetAnimationState("horizontal")
    end
end

function Vent_Open(furniture)
    furniture.Parameters["openness"].SetValue("1")
end

function Vent_Close(furniture)
    furniture.Parameters["openness"].SetValue("0")
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

function Berth_TestSummoning(furniture, deltaTime)
    if (furniture.Parameters["occupied"].ToFloat() <= 0) then
        Berth_SummonShip(furniture, nil)
        furniture.Parameters["occupied"].SetValue(1)
    elseif (World.Current.shipManager.IsOccupied(furniture)) then
        Berth_DismissShip(furniture, nil)
        furniture.Parameters["occupied"].SetValue(0)
    end
end

function Berth_SummonShip(furniture, character)
    --ModUtils.ULogChannel("Ships", "Summoning ship")
    local ship = World.Current.shipManager.AddShip("essentia", 0, 0)
    ship.SetDestination(furniture)
end

function Berth_DismissShip(furniture, character)
    local shipManager = World.Current.shipManager
    if (shipManager.IsOccupied(furniture)) then
        local ship = shipManager.GetBerthedShip(furniture)
        shipManager.DeberthShip(furniture)
        ship.SetDestination(0, 0)
    end
end

function Door_GetSpriteName(furniture)
	if (furniture.verticalDoor) then
	    return furniture.Type .. "_vertical_0"
	else
	    return furniture.Type .. "_horizontal_0"
	end
end

function OreMine_CreateMiningJob(furniture, character)
    local job = Job.__new(
		furniture.Tile,
		"OreMine_CreateMiningJob",
        nil,
        0,
        nil,
        Job.JobPriority.High,
        false,
        false,
        false,
        true
	)

    job.Description = "job_ore_mine_mining_desc"
    job.RegisterJobWorkedCallback("OreMine_OreMined")
    ModUtils.ULog("Create Mining Job - Mining Job Created")
    return job
end

function OreMine_OreMined(job)
    -- Defines the ore to be spawned by the mine
	if (job.buildable == nil) then
		return
	end

    local inventory = Inventory.__new(job.buildable.Parameters["ore_type"], 10)

    if (inventory.Type ~= "None") then
        -- Place the "mined" ore on the tile
        World.Current.inventoryManager.PlaceInventory(job.tile, inventory)
    end
    
    -- Deconstruct the mined object
    job.buildable.Deconstruct()
    job.CancelJob()
end

function OreMine_GetSpriteName(furniture)
    if ( furniture.Parameters["ore_type"].ToString() == "raw_iron-") then
        return "astro_wall_" .. furniture.Parameters["ore_type"].ToString()
    end

    return "astro_wall"
end

-- This function gets called once, when the furniture is installed
function Rtg_InstallAction( furniture, deltaTime)
    -- TODO: find elegant way to register heat source and sinks to Temperature
	World.Current.temperature.RegisterSinkOrSource(furniture)
end

-- This function gets called once, when the furniture is uninstalled
function Rtg_UninstallAction( furniture, deltaTime)
	World.Current.temperature.DeregisterSinkOrSource(furniture)
	-- TODO: find elegant way to unregister previous register
end

function Berth_TestSummoning(furniture, deltaTime)
    if (furniture.Parameters["occupied"].ToFloat() <= 0) then
        Berth_SummonShip(furniture, nil)
        furniture.Parameters["occupied"].SetValue(1)
    elseif (World.Current.shipManager.IsOccupied(furniture)) then
        Berth_DismissShip(furniture, nil)
        furniture.Parameters["occupied"].SetValue(0)
    end
end

function Berth_SummonShip(furniture, character)
    --ModUtils.ULogChannel("Ships", "Summoning ship")
    local ship = World.Current.ShipManager.AddShip("essentia", 0, 0)
    if (ship.WouldFitInBerth(furniture)) then
    	ship.SetDestination(furniture)
    else
		World.Current.ShipManager.RemoveShip(ship)
		furniture.Parameters["occupied"].SetValue(0)
    end
end

function Berth_DismissShip(furniture, character)
    local shipManager = World.Current.ShipManager
    if (shipManager.IsOccupied(furniture)) then
        local ship = shipManager.GetBerthedShip(furniture)
        shipManager.DeberthShip(furniture)
        ship.SetDestination(0, 0)
    end
end

ModUtils.ULog("Furniture.lua loaded")
return "LUA Script Parsed!"
