-- TODO: Figure out the nicest way to have unified defines/enums
-- between C# and Lua so we don't have to duplicate anything.
ENTERABILITY_YES  = 0
ENTERABILITY_NO   = 1
ENTERABILITY_SOON = 2

--------------------------------      UTILITY      --------------------------------
function Clamp01( value )
	if (value > 1) then
		return 1
	elseif (value < 0) then
		return 0
	end

	return value
end

-------------------------------- Furniture Actions --------------------------------
function OnUpdate_GasGenerator( furniture, deltaTime )
	if ( furniture.tile.room == nil ) then
		return "Furniture's room was null."
	end

	if ( furniture.tile.room.GetGasAmount("O2") < 0.20) then
		furniture.tile.room.ChangeGas("O2", 0.01 * deltaTime)
	else
		-- Do we go into a standby mode to save power?
	end

	return
end

function OnUpdate_Door( furniture, deltaTime )
	if (furniture.GetParameter("is_opening") >= 1.0) then
		furniture.ChangeParameter("openness", deltaTime * 4) -- FIXME: Maybe a door open speed parameter?
		if (furniture.GetParameter("openness") >= 1)  then
			furniture.SetParameter("is_opening", 0)
		end
	else
		furniture.ChangeParameter("openness", deltaTime * -4)
	end


	furniture.SetParameter("openness", Clamp01(furniture.GetParameter("openness")) )

	if (furniture.cbOnChanged != nil) then
		furniture.cbOnChanged(furniture)
	end


end

function IsEnterable_Door( furniture )
	furniture.SetParameter("is_opening", 1)

	if (furniture.GetParameter("openness") >= 1) then
		return ENTERABILITY_YES --ENTERABILITY.Yes
	end

	return ENTERABILITY_SOON --ENTERABILITY.Soon
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


	if( furniture.tile.inventory != nil and furniture.tile.inventory.stackSize >= furniture.tile.inventory.maxStackSize ) then
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
	if( furniture.tile.inventory != nil and furniture.tile.inventory.stackSize == 0 ) then
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

	if( furniture.tile.inventory == nil ) then
		--Debug.Log("Creating job for new stack.");
		itemsDesired = Stockpile_GetItemsFromFilter()
	else
		--Debug.Log("Creating job for existing stack.");
		desInv = furniture.tile.inventory.Clone()
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
		false
	)

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
		if( spawnSpot.inventory != nil and spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize ) then
			-- We should stop this job, because it's impossible to make any more items.
			furniture.CancelJobs()
		end

		return
	end

	-- If we get here, then we have no current job. Check to see if our destination is full.
	if( spawnSpot.inventory != nil and spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize ) then
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
		true	-- This job repeats until the destination tile is full.
	)
	j.RegisterJobCompletedCallback("MiningDroneStation_JobComplete")

	furniture.AddJob( j )
end


function MiningDroneStation_JobComplete(j)
	World.current.inventoryManager.PlaceInventory( j.furniture.GetSpawnSpotTile(), Inventory.__new("Steel Plate", 50, 20) )
end

return "LUA Script Parsed!"
