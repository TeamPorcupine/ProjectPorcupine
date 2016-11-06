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
	DevConsole.TextObject().text = "\n<color=#7CFC00> Cleared Console :D </color>"
	DevConsole.ClearHistory()
end

function ShowTimeStamp( on )
	on = tobool(on)
	if on ~= nil then
		DevConsole.ShowTimeStamp(on)
	else
		DevConsole.LogError("The parameter provided is not of boolean type")
	end
end

function HelpAll()
	DevConsole.Log("-- Help --", "green")
	for index, value in ipairs(DevConsole:CommandArray()) do
		local text = "<color=orange>"..value.title..DevConsole.GetParameters(value).."</color>"
		if (value.descriptiveText ~= nil) then
			text = text .. "<color=#7CFC00> //" .. value.descriptiveText .. "</color>"
		end
		
		DevConsole.Log(text)
	end
	
	DevConsole.Log("<color=orange>Note:</color> If the function has no parameters you <color=red>don't</color> need to use the parameter modifier.")
	DevConsole.Log("<color=orange>Note:</color> You <color=red>don't</color> need to use the trailing parameter modifier either")
end

function ChangeCameraPositionLUA ( x, y )
	DevConsole:ChangeCameraPositionCSharp(ModUtils.LUAVector3(tonumber(x), tonumber(y)))
end

-------------------------------- Help Actions --------------------------------

