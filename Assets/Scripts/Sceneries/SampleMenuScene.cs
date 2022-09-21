using UnityEngine;
using Pspkurara.Sceneries;
using Pspkurara.Sceneries.Dialogs;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

// It is a scene sample of screen transition and dialog display.
public class SampleMenuScene : Scene
{

	[SerializeField] private Button m_Content1;
	[SerializeField] private Button m_Content2;
	[SerializeField] private Button m_Content3;
	[SerializeField] private Button m_ShowDialog;
	[SerializeField] private Button m_BackButton;

	// initialize scene.
	protected override UniTask OnInstancedScene()
	{
		// add the move screen functions.
		m_Content1.onClick.AddListener(()=>
		{
			SceneriesManager.LoadScene("SampleContentScene", new SampleContentSceneInitData()
			{
				contentText = "From Content1"
			});
		});
		m_Content2.onClick.AddListener(() =>
		{
			SceneriesManager.LoadScene("SampleContentScene", new SampleContentSceneInitData()
			{
				contentText = "From Content2"
			});
		});
		m_Content3.onClick.AddListener(() =>
		{
			SceneriesManager.LoadScene("SampleContentScene", new SampleContentSceneInitData()
			{
				contentText = "From Content3"
			});
		});

		// add the show dialog function.
		m_ShowDialog.onClick.AddListener(() =>
		{
			DialogManager.ShowDialog<SampleMessageDialog>("SampleMessageDialog", (dialog) =>
			{
				dialog.SetTitle("Sample Dialog");
				dialog.SetMessage("Sample Dialog Details");
				dialog.SetActiveButtons(1);
				dialog.SetButtonTitle(0, "Close");
				dialog.SetButtonCallback(0, dialog.Close);
			}).Forget();
		});

		// add the back scene function.
		m_BackButton.onClick.AddListener(() =>
		{
			SceneriesManager.UndoScene();
		});
		return base.OnInstancedScene();
	}

}
