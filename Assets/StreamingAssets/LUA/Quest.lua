-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

-- HOWTO Log:
--ModUtils.ULog("Testing ModUtils.ULogChannel")
--ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
--ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

---------------------------- Quests Actions --------------------------------

function Quest_Have_Furniture_Built(goal)
    goal.IsCompleted = false
    local type = goal.Parameters["type"].Value
    local amount = goal.Parameters["amount"].ToInt()
    local amountFound = World.Current.FurnitureManager.CountWithType(type)
    if (amountFound >= amount) then
        goal.IsCompleted = true
    end
end

function Quest_Spawn_Inventory(reward)
    local type = reward.Parameters["type"].Value
    local amount = reward.Parameters["amount"].ToInt()
    local inv = Inventory.__new(type, amount, amount)
    local tile = World.Current.InventoryManager.GetFirstTileWithValidInventoryPlacement(6, World.Current.GetCenterTile(), inv)
    if (tile == nil) then
        return
    end
    World.Current.inventoryManager.PlaceInventory(tile, inv)
    reward.IsCollected = true;
end

ModUtils.ULog("Quest.lua loaded")
return "LUA Script Parsed!"
