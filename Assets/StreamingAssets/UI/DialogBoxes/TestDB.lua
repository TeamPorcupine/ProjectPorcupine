DIALOGRESULT_YES = 0
DIALOGRESULT_NO = 1
DIALOGRESULT_CANCEL = 2
DIALOGRESULT_OKAY = 3

function Testing_DialogClosed( DialogBoxResult, result, data )
	ModUtils.ULog("DialogClosed")
	ModUtils.ULog("Result = " .. result)
	if (result == DIALOGRESULT_YES) then
		ModUtils.ULog(data[1])
	end
end