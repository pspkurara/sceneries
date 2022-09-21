using UnityEngine;
using Pspkurara.Sceneries;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

// This is a scene sample of screen transition..
public class SampleTitleScene : Scene
{

	[SerializeField] private Button m_GoMenuButton;

	// initialize scene.
	protected override UniTask OnInstancedScene()
	{
		// add the move scene function.
		m_GoMenuButton.onClick.AddListener(() =>
		{
			SceneriesManager.LoadScene("SampleMenuScene");
		});
		return base.OnInstancedScene();
	}

}
