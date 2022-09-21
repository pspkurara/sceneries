# Sceneries

UI screen transition system using uGUI.
Supports full screen screens and dialogs.

[![](https://img.shields.io/npm/v/com.pspkurara.sceneries?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pspkurara.sceneries/)
[![](https://img.shields.io/github/v/release/pspkurara/sceneries)](https://github.com/pspkurara/sceneries/releases/)
[![](https://img.shields.io/github/watchers/pspkurara/sceneries?style=social)](https://github.com/pspkurara/external-selecion-state/subscription)

## Usage

### Create Scene

1. Create a new script and add "using Pspkurara.Sceneries".
2. Inherits the "Scene" class.
3. Override virtual functions to describe scene processing.
4. Build a scene prefab based on the template, attach the script to the prefab, and save it in a folder. (Default is Resources/Sceneries/{prefab name})
5. Load a scene by calling the "SceneriesManager.LoadScene(scene name)" function from another scene.
6. Set the initial scene name to "Initialize Load Sceneries Name" in the "SceneriesManager" prefab.
```
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
```

### Create Dialog

1. Create a new script and add "using Pspkurara.Sceneries.Dialogs".
2. Inherits the "Dialog" class. but most of the time you want a button, so extend "ArrayButtonDialog" instead.
3. Override virtual functions to describe scene processing.
4. Build a scene prefab based on the template, attach the script to the prefab, and save it in a folder. (Default is Resources/Dialogs/{prefab name})
5. Load a dialog by calling the "DialogManager.ShowDialog<DialogClass>(dialog name, dialog initializer).Forget()" function from another scene.
```
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
```

```
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
```

### Transition animation

1. Attaches a "StandardSceneTransition" to a Scene or Dialog attached object. (Another option is "PlayableSceneTransition" using the timeline)
2. Set the animation by referring to DefaultSceneTransition and DefaultSceneLoader in Templates. (If you edit at will, please copy or create from "Create/Pspkurara/Sceneries/Standard Scene Transition Setting")
3. Animation will work if you attach it to the "Setting" of the screen.
4. Setting the "Allow Duplicate Playback" flag to true will animate the screen before and after the transition at the same time.

### Full API references
* https://pspkurara.github.io/sceneries/

## Installation

### Using OpenUPM
Go to Unity's project folder on the command line and call:

```
openupm add com.pspkurara.sceneries
```

### Using Unity Package Manager (For Unity 2018.3 or later)
Find the manifest.json file in the Packages folder of your project and edit it to look like this:

```
{
  "dependencies": {
    "com.pspkurara.sceneries": "https://github.com/pspkurara/sceneries.git#upm",
    ...
  },
}
```

#### Requirement
Unity 2018.1 or later<br>
May work in Unity5, but unofficial.

## License

* [MIT](https://github.com/pspkurara/sceneries/blob/master/Packages/Sceneries/LICENSE.md)

## Author

* [pspkurara](https://github.com/pspkurara) 
[![](https://img.shields.io/twitter/follow/pspkurara.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=pspkurara) 

## See Also

* GitHub page : https://github.com/pspkurara/sceneries
