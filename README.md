# External Selection State (ESS)

The system allows external access to Selection.SelectionState, which is a protected Enum of uGUI.</br>
You need to create your own UI (CustomButton, CustomToggle, etc.) classes that inherit from components that inherit from Selection classes such as Button and Toggle, and implement this library.

[![](https://img.shields.io/npm/v/com.pspkurara.external-selection-state?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.pspkurara.external-selection-state/)
[![](https://img.shields.io/github/v/release/pspkurara/external-selection-state)](https://github.com/pspkurara/external-selection-state/releases/)
[![](https://img.shields.io/github/watchers/pspkurara/external-selection-state?style=social)](https://github.com/pspkurara/external-selecion-state/subscription)

## Usage

### Creating a UI that inherits from "UnityEngine.UI.Selectable"

1. Create a script that extends the UI (e.g. UnityEngine.UI.Button etc.) class to which you want to add ESS functionality. (Selection is limited to components that have UnityEngine.UI.)
2. Add "using Pspkurara.UI" and "using Pspkurara.UI.ESS", 
3. Make it inherit the "ISelectableWithESS" interface and implementation functions.
4. Override the "Selection.DoStateTransition" function and call "this.DoStateTransitionFromSelectable" function.
```
using UnityEngine;
using UnityEngine.UI;
using Pspkurara.UI;
using Pspkurara.UI.ESS;

public class CustomButton : Button, ISelectableWithESS
{
	// Use UnityEvent to register in the inspector and execute in the editor.
	[SerializeField]
	private OnDoStateTransitionEvent m_OnDoStateTransitionEvent = new OnDoStateTransitionEvent();
	
	public ExternalSelectionState currentExternalSelectionState
	{ 	// Convert protected Selectable.SelectionState to public.
		get { return (ExternalSelectionState)(int)base.currentSelectionState; } 
	}
	
	public OnDoStateTransitionEvent onDoStateTransition
	{	// Callback executed when DoStateTransition is called.
		get { return m_OnDoStateTransitionEvent ; } 
	}
	
	protected override void DoStateTransition(SelectionState state, bool instant)
	{
		base.DoStateTransition(state, instant);
		// Trigger from UnityEngine.UI.Selectable.
		this.DoStateTransitionFromSelectable((int)state, instant);
	}
}
```

### Create a script to add functionality to an extended "UnityEngine.UI.Selectable"

1. Create a new script and add "using Pspkurara.UI".
2. Add an implementation to receive "ISelectableWithESS".
3. Implement a function with the same arguments as "Selectable.DoStateTransition". (OnDoStateTransition thereafter)
4. Added implementation to register with ESS callbacks upon generation or activation.
5. Added implementation to release ESS callback on destroy or disable.
6. Add "immediate reflection" call just before adding callback.
7. Implement the function or effect you want to have in the ”OnDoStateTransition”.
```
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pspkurara.UI;

public class SampleLink : UIBehaviour
{
	// Own Selectable.
	private ISelectableWithESS m_Selectable;

	/// <summary>
	/// Own Selectable.
	/// ISelectableWithESS must be inherited.
	/// </summary>
	public ISelectableWithESS selectable {
		get {
			// interface cannot be Serialized, so it should be acquired automatically.
			if (m_Selectable == null)
			{
				m_Selectable = GetComponent<ISelectableWithESS>();
			}
			return m_Selectable;
		}
	}

	protected override void OnEnable()
	{
		if (selectable == null) return;

		// Immediate reflection at initialization.
		OnDoStateTransition(selectable.currentExternalSelectionState, true);

		// Callback Registration.
		selectable.onDoStateTransition.AddListener(OnDoStateTransition);
	}

	protected override void OnDisable()
	{
		if (selectable == null) return;
		
		// Callback Release.
		selectable.onDoStateTransition.RemoveListener(OnDoStateTransition);
	}

	// Called when there is an actual change in state.
	private void OnDoStateTransition(ExternalSelectionState state, bool instant)
	{
		// any processing.
		Debug.Log(state);
	}

}
```

### Use the components you have created

1. Attach an effect adaptation component (SampleLink in the example) to an object with an extended Selectable (CustomButton in the example).
2. Execute or change parameters and check if they are reflected correctly.

## Installation

### Using OpenUPM
Go to Unity's project folder on the command line and call:

```
openupm add com.pspkurara.external-selection-state
```

### Using Unity Package Manager (For Unity 2018.3 or later)
Find the manifest.json file in the Packages folder of your project and edit it to look like this:

```
{
  "dependencies": {
    "com.pspkurara.external-selection-state": "https://github.com/pspkurara/external-selection-state.git#upm",
    ...
  },
}
```

#### Requirement
Unity 2018.1 or later<br>
May work in Unity5, but unofficial.

## License

* [MIT](https://github.com/pspkurara/external-selection-state/blob/master/Packages/ExternalSelectionState/LICENSE.md)

## Author

* [pspkurara](https://github.com/pspkurara) 
[![](https://img.shields.io/twitter/follow/pspkurara.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=pspkurara) 

## See Also

* GitHub page : https://github.com/pspkurara/external-selection-state
