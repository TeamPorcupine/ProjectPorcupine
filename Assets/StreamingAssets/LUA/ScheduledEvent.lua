-------------------------------------------------------
-- Project Porcupine Copyright(C) 2016 Team Porcupine
-- This program comes with ABSOLUTELY NO WARRANTY; This is free software,
-- and you are welcome to redistribute it under certain conditions; See
-- file LICENSE, which is part of this source code package, for details.
-------------------------------------------------------

function ping_log_lua (event)
    ModUtils.ULogChannel("Scheduler", "Scheduled Lua event '" .. event.Name .. "'")
    return
end

ModUtils.ULog("Events.lua loaded")
return "LUA Script Parsed!"
