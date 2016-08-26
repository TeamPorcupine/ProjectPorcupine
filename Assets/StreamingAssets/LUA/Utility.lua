-- TODO: Figure out the nicest way to have unified defines/enums
-- between C# and Lua so we don't have to duplicate anything.
ENTERABILITY_YES  = 0
ENTERABILITY_NO   = 1
ENTERABILITY_SOON = 2

-- HOWTO Log:
--ModUtils.ULog("Testing ModUtils.ULogChannel")
--ModUtils.ULogWarning("Testing ModUtils.ULogWarningChannel")
--ModUtils.ULogError("Testing ModUtils.ULogErrorChannel") -- Note: pauses the game

-------------------------------- Utility Actions --------------------------------

ModUtils.ULog("Utility.lua loaded")
return "LUA Script Parsed!"
