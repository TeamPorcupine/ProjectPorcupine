-- HOWTO Log:
--ModUtils.ULog("Testing ModUtils.ULogChannel")
--ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
--ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

---------------------------- Quests Actions --------------------------------

function Quest_Have_Furniture_Built(goal)
    goal.IsCompleted = false
    objectType = goal.Parameters["objectType"].Value
    amount = goal.Parameters["amount"].ToInt()
    amountFound = World.current.CountFurnitureType(objectType)
    if(amountFound >= amount) then
        goal.IsCompleted = true
    end
end

function Quest_Spawn_Inventory(reward)
 tile = World.current.GetCenterTile()
 objectType = reward.Parameters["objectType"].Value
 amount = reward.Parameters["amount"].ToInt()
 inv = Inventory.__new(objectType, amount, amount)
 World.current.inventoryManager.PlaceInventory( tile, inv)
end

return "LUA Script Parsed!"
