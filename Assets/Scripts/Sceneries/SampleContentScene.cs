using UnityEngine;
using Pspkurara.Sceneries;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

// This is a scene sample that can change the contents dynamically.
public class SampleContentScene : HasInitScene<SampleContentSceneInitData>
{

	[SerializeField] private Text m_ContentText;
	[SerializeField] private Button m_BackButton;

	// initialize from init data.
	protected override void OnInitializeScene(SampleContentSceneInitData initData)
	{
		m_ContentText.text = initData.contentText;
	}

	// initialize scene.
	protected override UniTask OnInstancedScene()
	{
		m_BackButton.onClick.AddListener(() =>
		{
			SceneriesManager.UndoScene();
		});
		return base.OnInstancedScene();
	}

}

// SampleContentScene initialize data.
public class SampleContentSceneInitData : SceneInitData
{

	public string contentText;

}
