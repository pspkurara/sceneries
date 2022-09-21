using UnityEngine;
using UnityEngine.UI;
using Pspkurara.Sceneries.Dialogs;

// simple the title and message dialog sample.
public class SampleMessageDialog : ArrayButtonsDialog
{

	[SerializeField] private Text m_Title;
	[SerializeField] private Text m_Message;

	// set title.
	public void SetTitle(string title)
	{
		m_Title.text = title;
	}

	// set message.
	public void SetMessage(string message)
	{
		m_Message.text = message;
	}

}
