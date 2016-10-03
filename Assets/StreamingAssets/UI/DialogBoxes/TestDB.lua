DIALOGRESULT_YES = 0
DIALOGRESULT_NO = 1
DIALOGRESULT_CANCEL = 2
DIALOGRESULT_OKAY = 3

function Testing_DialogClosed( DialogBoxResult, data )
	ModUtils.ULog("DialogClosed")
	if (data == DIALOGRESULT_YES) then
		WorldController.instance.dialogBoxManager.ShowDialogBoxByName("Load File")
	end
end