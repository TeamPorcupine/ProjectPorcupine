-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

-- HOWTO Log:
-- ModUtils.ULog("Testing ModUtils.ULogChannel")
-- ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
-- ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

-------------------------------- RoomBehavior Actions --------------------------------
function OnControl_Airlock( roomBehavior, deltaTime )
    for discard, pressureDoor in pairs(roomBehavior["Pressure Door"]) do
        pressureDoor.Parameters["pressure_locked"].SetValue(1)
    end
    
    for discard, pump in pairs(roomBehavior["pump_air"]) do
        pump.Parameters["flow_direction_up"].SetValue(1 - pump.Parameters["flow_direction_up"].ToFloat())
    end
end
