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
    objectType = goal.Parameters["objectType"].Value
    amount = goal.Parameters["amount"].ToInt()
    amountFound = World.Current.CountFurnitureType(objectType)
    if(amountFound >= amount) then
        goal.IsCompleted = true
    end
end

function Quest_Spawn_Inventory(reward)
 --tile = World.Current.GetCenterTile()
 tile = World.Current.GetFirstCenterTileWithNoInventory(6)
 if(tile == nil) then
  return
 end
 objectType = reward.Parameters["objectType"].Value
 amount = reward.Parameters["amount"].ToInt()
 inv = Inventory.__new(objectType, amount, amount)
 World.Current.inventoryManager.PlaceInventory( tile, inv)
 reward.IsCollected = true;
end

ModUtils.ULog("Quest.lua loaded")
return "LUA Script Parsed!"
