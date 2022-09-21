using UnityEngine;
using UnityEngine.UI;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// ダイアログのボタン。
	/// </summary>
	public class DialogButton : DialogButtonBase
	{

		/// <summary>
		/// ボタンの表示テキスト。
		/// </summary>
		[SerializeField]
		private Text m_Title = null;

		public override void SetTitle(string title)
		{
			m_Title.text = title;
		}

	}

}

