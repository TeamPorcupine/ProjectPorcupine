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
        pressureDoor.Parameters["pressure_locked"].SetValue(true)
        pressureDoor.Parameters["airlock_controlled"].SetValue(true)
        pressureDoor.UpdateOnChanged(pressureDoor)
    end
    
    for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
        if (pump.Tile.South().Room == roomBehavior.room or pump.Tile.West().Room == roomBehavior.room ) then
            pump.Parameters["out_direction"].SetValue(1)
        else
            pump.Parameters["out_direction"].SetValue(0)
        end
        pump.Parameters["active"].SetValue(false)
        pump.Parameters["flow_direction_up"].SetValue(pump.Parameters["out_direction"].ToFloat())
        pump.UpdateOnChanged(pump)
    end
end

function PumpOut_Airlock( roomBehavior, targetPressure )
--ModUtils.ULogWarning(os.clock() - roomBehavior.Parameters["pump_off_time"].ToFloat())
    if (roomBehavior.Parameters["is_pumping_in"].ToBool() == false and (os.clock() - roomBehavior.Parameters["pump_off_time"].ToFloat()) > 2) then
        -- ModUtils.ULogWarning("Yes We Are")
        roomBehavior.Parameters["is_pumping_out"].SetValue(true)
        for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
            pump.Parameters["source_pressure_limit"].SetValue(targetPressure)
            pump.Parameters["target_pressure_limit"].SetValue(3)
            pump.Parameters["active"].SetValue(true)
            pump.Parameters["flow_direction_up"].SetValue(pump.Parameters["out_direction"].ToFloat())
            pump.UpdateOnChanged(pump)
        end
    end
end

function PumpIn_Airlock( roomBehavior, targetPressure )
--ModUtils.ULogWarning(os.clock() - roomBehavior.Parameters["pump_off_time"].ToFloat())
    if (roomBehavior.Parameters["is_pumping_out"].ToBool() == false and (os.clock() - roomBehavior.Parameters["pump_off_time"].ToFloat()) > 2) then
    -- ModUtils.ULogWarning("Yes We Are")
        roomBehavior.Parameters["is_pumping_in"].SetValue(true)
        for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
            pump.Parameters["source_pressure_limit"].SetValue(0)
            pump.Parameters["target_pressure_limit"].SetValue(targetPressure)
            pump.Parameters["active"].SetValue(true)
            pump.Parameters["flow_direction_up"].SetValue(1 - pump.Parameters["out_direction"].ToFloat())
            pump.UpdateOnChanged(pump)
        end
    end
end

function PumpOff_Airlock( roomBehavior, deltaTime )
    roomBehavior.Parameters["pump_off_time"].SetValue(os.clock())
    roomBehavior.Parameters["is_pumping_in"].SetValue(false)
    roomBehavior.Parameters["is_pumping_out"].SetValue(false)
    for discard, pump in pairs(roomBehavior.ControlledFurniture["pump_air"]) do
        pump.Parameters["active"].SetValue(false)
        pump.UpdateOnChanged(pump)
    end
end