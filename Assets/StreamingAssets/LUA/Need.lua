-- HOWTO Log:
--ModUtils.ULog("Testing ModUtils.ULogChannel")
--ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
--ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

---------------------------- Need Actions --------------------------------

function OnUpdate_Oxygen( need, deltaTime )
    if (need.Character != nil and need.Character.CurrTile.GetGasPressure("O2") < 0.15) then
        need.Amount = need.Amount + ((0.3 - (0.3 * (need.Character.CurrTile.GetGasPressure("O2") * 5))) * deltaTime)
    else
        need.Amount = need.Amount - (need.GrowthRate * deltaTime)
    end
end

ModUtils.ULog("Need.lua loaded")
return "LUA Script Parsed!"
