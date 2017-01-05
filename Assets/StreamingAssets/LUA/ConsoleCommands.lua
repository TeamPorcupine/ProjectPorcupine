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

-------------------------------- Helper Functions --------------------------------

-- Just some simple helper functions

-- Returns true if x can be numeric
function isNumeric( x )
	if tonumber(x) ~= nil then
		return true
	else
		return false
	end
end

-- Cases:
-- Value is string
--  Value:lower() is "false" - > boolean false
--  Value:lower() is "true"  - > boolean true

-- Value is convertable to a number
--	Value is 0 			     - > boolean false
--  Value is 1 			     - > boolean true

-- Value is a boolean
--  Value is true 			 - > boolean true
--  Value is false 			 - > boolean false
-- Else:
--  return nil
function tobool(value)
	if isNumeric( value ) then
		number = tonumber(value)
		if number == 0 then
			return false
		elseif number == 1 then
			return true
		end
	end
	
	if type(value) == "boolean" then
        return value
	elseif type(value) == "string" and (value:lower() == "false") then
        return false
	elseif type(value) == "string" and (value:lower() == "true") then
		return true
	end

	return nil
end

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
		DevConsole.TextObject().fontSize = size;
		DevConsole.Log("Change successful :D", "green")
	end
end

function Clear()
	DevConsole.TextObject().text = "\n<color=#7CFC00> Cleared Console :D</color>"
	DevConsole.ClearHistory()
end

-------------------------------- Help Actions --------------------------------

