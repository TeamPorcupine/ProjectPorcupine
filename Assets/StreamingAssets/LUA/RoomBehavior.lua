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
    for discard, pressureDoor in pairs(roomBehavior.ControlledFurniture["Pressure Door"]) do
        pressureDoor.Parameters["pressure_locked"].SetValue(1)
        ModUtils.ULogWarning("Locked an Airlock Door")
        pressureDoor.UpdateOnChanged(pressureDoor)
    end
    
    for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
        if (pump.Tile.South().Room == roomBehavior.room or pump.Tile.West().Room == roomBehavior.room ) then
            pump.Parameters["out_direction"].SetValue(1)
        else
            pump.Parameters["out_direction"].SetValue(0)
        end
        pump.Parameters["flow_direction_up"].SetValue(pump.Parameters["out_direction"].ToFloat())
        pump.UpdateOnChanged(pump)
    end
end

function PumpOut_Airlock( roomBehavior, deltaTime )
    for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
        pump.Parameters["flow_direction_up"].SetValue(pump.Parameters["out_direction"].ToFloat())
        pump.UpdateOnChanged(pump)
    end
end

function PumpIn_Airlock( roomBehavior, deltaTime )
    for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
        pump.Parameters["flow_direction_up"].SetValue(1 - pump.Parameters["out_direction"].ToFloat())
        pump.UpdateOnChanged(pump)
    end
end
