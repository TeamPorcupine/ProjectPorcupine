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

-------------------------------- Command Actions --------------------------------

function Set_FontSize( size ) 
	size = tonumber(size)
	if (size < 10) then
		DevConsole.LogError("Font size would be too small")
	elseif (size > 20) then
		ModUtils.ULog("")
		DevConsole.LogError("Font size would be too big")
	else
		ModUtils.ULog("")
		DevConsole.SetTextSize(size)
		DevConsole.Log("Change successful :D", "green")
	end
end

-------------------------------- Help Actions --------------------------------

function Set_FontSizeHelp()
	DevConsole.Log("<color=yellow>Command Info:</color> Sets the font size, must be between 10 and 20 (inclusive)")
	DevConsole.Log("Call it like: SetFontSize: 10")
end