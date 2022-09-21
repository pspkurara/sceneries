#if SCENERIES_TEXTMESHPRO_SUPPORT

using UnityEngine;
using TMPro;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// ダイアログのボタン。
	/// </summary>
	public class DialogButtonForTextMeshPro : DialogButtonBase
	{

		/// <summary>
		/// ボタンの表示テキスト。
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI m_Title = null;

		/// <summary>
		/// ボタンの表示テキストを適応する。
		/// </summary>
		/// <param name="title">表示テキスト</param>
		public override void SetTitle(string title)
		{
			m_Title.text = title;
		}

	}

}

#endif
