DIALOGRESULT_YES = 0
DIALOGRESULT_NO = 1
DIALOGRESULT_CANCEL = 2
DIALOGRESULT_OKAY = 3

function Testing_DialogClosed( DialogBoxResult, result, data )
	if (result == DIALOGRESULT_YES) then
		Prompt = WorldController.Instance.dialogBoxManager.ShowDialogBoxByName("Prompt or Info")
		Prompt.SetPrompt("Are you sure?")
		Buttons = {0,1,2}
		Prompt.SetButtons(Buttons)
		Prompt.setClosedAction("Prompt_Closed")
	end
end

function Prompt_Closed( DialogResult )
	ModUtils.ULog(DialogResult)
end