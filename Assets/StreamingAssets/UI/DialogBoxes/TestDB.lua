DIALOGRESULT_YES = 0
DIALOGRESULT_NO = 1
DIALOGRESULT_CANCEL = 2
DIALOGRESULT_OK = 3

function Testing_DialogClosed( DialogBoxResult, data )
	ModUtils.ULog("DialogClosed")
	if (data == DIALOGRESULT_YES) then
		ModUtils.ULog("You've successfully added LUA to a dialog box.")
	end
end