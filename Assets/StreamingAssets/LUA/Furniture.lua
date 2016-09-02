-- TODO: Figure out the nicest way to have unified defines/enums
-- between C# and Lua so we don't have to duplicate anything.
ENTERABILITY_YES  = 0
ENTERABILITY_NO   = 1
ENTERABILITY_SOON = 2

-------------------------------- Furniture Actions --------------------------------
function OnUpdate_GasGenerator( furniture, deltaTime )
	if ( furniture.HasPower() == false) then
		return
	end

	if ( furniture.tile.room == nil ) then
		return "Furniture's room was null."
	end
    
    keys = furniture.Parameters["gas_gen"].Keys()
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
	-- Door is closed
	if (furniture.Parameters["openness"].ToFloat() < 0.1) then
		return "Door"
	end
	-- Door is a bit open
	if (furniture.Parameters["openness"].ToFloat() < 0.5) then
		return "Door_openness_1"
	end
	-- Door is a lot open
	if (furniture.Parameters["openness"].ToFloat() < 0.9) then
		return "Door_openness_2"
	end
	-- Door is a fully open
	return "Door_openness_3"
end

function GetSpriteName_Airlock( furniture )
	-- Door is closed
	if (furniture.Parameters["openness"].ToFloat() < 0.1) then
		return "Airlock"
	end
	-- Door is a bit open
	if (furniture.Parameters["openness"].ToFloat() < 0.5) then
		return "Airlock_openness_1"
	end
	-- Door is a lot open
	if (furniture.Parameters["openness"].ToFloat() < 0.9) then
		return "Airlock_openness_2"
	end
	-- Door is a fully open
	return "Airlock_openness_3"
end

function Stockpile_GetItemsFromFilter()
	-- TODO: This should be reading from some kind of UI for this
	-- particular stockpile

	-- Probably, this doesn't belong in Lua at all and instead we should
	-- just be calling a C# function to give us the list.

	-- Since jobs copy arrays automatically, we could already have
	-- an Inventory[] prepared and just return that (as a sort of example filter)

	return { Inventory.__new("Steel Plate", 50, 0) }
end


function Stockpile_UpdateAction( furniture, deltaTime )
	-- We need to ensure that we have a job on the queue
	-- asking for either:
	--  (if we are empty): That ANY loose inventory be brought to us.
	--  (if we have something): Then IF we are still below the max stack size,
	--						    that more of the same should be brought to us.

	-- TODO: This function doesn't need to run each update.  Once we get a lot
	-- of furniture in a running game, this will run a LOT more than required.
	-- Instead, it only really needs to run whenever:
	--		-- It gets created
	--		-- A good gets delivered (at which point we reset the job)
	--		-- A good gets picked up (at which point we reset the job)
	--		-- The UI's filter of allowed items gets changed


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

	itemsDesired = {}

	if( furniture.tile.Inventory == nil ) then
		--Debug.Log("Creating job for new stack.");
		itemsDesired = Stockpile_GetItemsFromFilter()
	else
		--Debug.Log("Creating job for existing stack.");
		desInv = furniture.tile.Inventory.Clone()
		desInv.maxStackSize = desInv.maxStackSize - desInv.stackSize
		desInv.stackSize = 0

		itemsDesired = { desInv }
	end

	j = Job.__new(
		furniture.tile,
		nil,
		nil,
		0,
		itemsDesired,
		Job.JobPriority.Low,
		false
	)
	j.JobDescription = "job_stockpile_moving_desc"

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

			return  -- There should be no way that we ever end up with more than on inventory requirement with stackSize > 0
		end
	end
end


function MiningDroneStation_UpdateAction( furniture, deltaTime )

	spawnSpot = furniture.GetSpawnSpotTile()

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

	-- If we get here, we need to CREATE a new job.

	jobSpot = furniture.GetJobSpotTile()

	j = Job.__new(
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
	World.current.inventoryManager.PlaceInventory( j.furniture.GetSpawnSpotTile(), Inventory.__new("Raw Iron", 50, 20) )
end

function MetalSmelter_UpdateAction(furniture, deltaTime)
	spawnSpot = furniture.GetSpawnSpotTile()

	if(furniture.Parameters["smelting"].ToFloat() == 0) then
		if(furniture.JobCount() == 0) then
			itemsDesired = {Inventory.__new("Raw Iron", 50, 0)}

			jobSpot = furniture.GetJobSpotTile()

			j = Job.__new(
			jobSpot,
			nil,
			nil,
			0.4,
			itemsDesired,
			Job.JobPriority.Medium,
			false
			)

			j.RegisterJobCompletedCallback("MetalSmelter_JobComplete")
			j.JobDescription = "job_metal_smelter_fulling_desc"

			furniture.AddJob(j)
		end
	else
		-- ugly hack because spawnSpot inventory is disappearing, so just reset it to what it should be if it's gone
		if(spawnSpot.Inventory == nil) then
			spawnSpot.Inventory = Inventory.__new("Raw Iron", 50, 0)
		end
		furniture.ChangeParameter("smelttime", deltaTime)

		if(furniture.Parameters["smelttime"].ToFloat >= furniture.Parameters["smelttime_required"]) then
			furniture.Parameters["smelttime"].SetValue(0)

			outputSpot = World.current.GetTileAt(spawnSpot.X+2, spawnSpot.y)

			if(outputSpot.Inventory == nil) then
				World.current.inventoryManager.PlaceInventory( outputSpot, Inventory.__new("Steel Plate", 50, 5) )

				spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize-5
			else
				if(outputSpot.Inventory.stackSize <= 45) then
					outputSpot.Inventory.stackSize = outputSpot.Inventory.stackSize+5

					spawnSpot.Inventory.stackSize = spawnSpot.Inventory.stackSize-5
				end
			end

			if(spawnSpot.Inventory.stackSize <= 0) then
				spawnSpot.Inventory = nil
				furniture.Parameters["smelting"].SetValue(0)
			end
		end
	end
end

function MetalSmelter_JobComplete(j)
	j.furniture.SetParameter("smelting", 1)
    j.UnregisterJobCompletedCallback("MetalSmelter_JobComplete")
    j.UnregisterJobWorkedCallback("MetalSmelter_JobWorked")
end

function MetalSmelter_JobWorked(j)
    for k, inv in pairs(j.inventoryRequirements) do
        if(inv ~= nil and inv.stackSize > 0) then
            spawnSpot = j.tile.Furniture.GetSpawnSpotTile()
            World.current.inventoryManager.PlaceInventory(spawnSpot, inv)
            spawnSpot.Inventory.isLocked = true
            return
        end
    end
end

function PowerCellPress_UpdateAction(furniture, deltaTime)
	spawnSpot = furniture.GetSpawnSpotTile()
	
	if(spawnSpot.Inventory == nil) then
		if(furniture.JobCount() == 0) then
			itemsDesired = {Inventory.__new("Steel Plate", 10, 0)}
			
			jobSpot = furniture.GetJobSpotTile()

			j = Job.__new(
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
			
			outputSpot = World.current.GetTileAt(spawnSpot.X+2, spawnSpot.y)
			
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
	spawnSpot = j.tile.Furniture.GetSpawnSpotTile()
	
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

	j = Job.__new(
	furniture.GetJobSpotTile(),
	nil,
	nil,
	10,
	nil,
	false
	)
	j.RegisterJobCompletedCallback("CloningPod_JobComplete")
	j.JobDescription = "job_cloning_pod_cloning_desc"
	furniture.AddJob( j )
end

function CloningPod_JobComplete(j)
	j.furniture.Deconstruct()
	char = World.current.CreateCharacter(j.furniture.GetSpawnSpotTile())

end

function PowerGenerator_UpdateAction(furniture, deltatime)
    
    if ( furniture.JobCount() < 1 and furniture.Parameters["burnTime"].ToFloat() == 0 ) then
        
        furniture.PowerValue = 0
        itemsDesired = {Inventory.__new("Uranium", 5, 0)}
        
        j = Job.__new(
            furniture.GetJobSpotTile(),
            nil,
            nil,
            0.5,
            itemsDesired,
            Job.JobPriority.High,
            false
        )
		j.JobDescription = "job_power_generator_fulling_desc"

        j.RegisterJobCompletedCallback("PowerGenerator_JobComplete")
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
    
	spawnSpot = furniture.GetSpawnSpotTile()
	jobSpot = furniture.GetJobSpotTile()
	inputSpot = World.current.GetTileAt(jobSpot.X, jobSpot.y-1)

	if(inputSpot.Inventory == nil) then
		if(furniture.JobCount() == 0) then
			itemsDesired = {Inventory.__new("Steel Plate", furniture.Parameters["tradeinamount"].ToFloat())}

			j = Job.__new(
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

		 	outputSpot = World.current.GetTileAt(spawnSpot.X+1, spawnSpot.y)

			if(outputSpot.Inventory == nil) then
				World.current.inventoryManager.PlaceInventory( outputSpot, Inventory.__new("Steel Plate", 50, furniture.Parameters["tradeoutamount"].ToFloat()) )

				inputSpot.inventory.stackSize = inputSpot.inventory.stackSize-furniture.Parameters["tradeinamount"].ToFloat()
			else
				if(outputSpot.inventory.stackSize <= 50 - outputSpot.inventory.stackSize+furniture.Parameters["tradeoutamount"].ToFloat()) then
					outputSpot.inventory.stackSize = outputSpot.inventory.stackSize+furniture.Parameters["tradeoutamount"].ToFloat()
					inputSpot.inventory.stackSize = inputSpot.inventory.stackSize-furniture.Parameters["tradeinamount"].ToFloat()
				end
			end

			if(inputSpot.Inventory.stackSize <= 0) then
				inputSpot.Inventory = nil
			end

		end
	end
end

function LandingPad_Temp_JobComplete(j)
	jobSpot = j.furniture.GetJobSpotTile()
	inputSpot = World.current.GetTileAt(jobSpot.X, jobSpot.y-1)

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

-- Dummy heater install function
-- THis function gets called once, when the funriture is isntalled
function Heater_InstallAction( furniture, deltaTime)
	-- TODO: find elegant way to register heat source and sinks to Temperature
end

-- Dummy heater uninstall function
-- THis function gets called once, when the funriture is unisntalled
function Heater_UninstallAction( furniture, deltaTime)
	-- TODO: find elegant way to unregister previous register
end

return "LUA Script Parsed!"
