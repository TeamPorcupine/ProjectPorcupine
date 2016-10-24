DIALOGRESULT_YES = 0
DIALOGRESULT_NO = 1
DIALOGRESULT_CANCEL = 2
DIALOGRESULT_OKAY = 3

local localDialogBox
function Filter_YesClicked( DialogBox )
    localDialogBox = DialogBox
    Prompt = WorldController.Instance.dialogBoxManager.ShowDialogBoxByName("Prompt or Info")
    Prompt.SetPrompt("Are you sure?")
    Buttons = {0,1,2}
    Prompt.SetButtons(Buttons)
    Prompt.setClosedAction("Prompt_Closed")
end

function Filter_NoClicked ( DialogBox )
    DialogBox.NoButtonClicked()
end

function Filter_CancelClicked ( DialogBox )
    DialogBox.CancelButtonClicked()
end

function Prompt_Closed( DialogResult )
	if DialogResult == DIALOGRESULT_YES then
        ModUtils.ULog("Clicked yes")
        localDialogBox.YesButtonClicked()
    end
end