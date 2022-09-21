using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Pspkurara.Sceneries.Dialogs
{


	/// <summary>
	/// ダイアログのボタン。
	/// </summary>
	public abstract class DialogButtonBase : UIBehaviour
	{

		/// <summary>
		/// ボタン本体。
		/// </summary>
		[SerializeField]
		private Button m_Button = null;

		/// <summary>
		/// 押下時のコールバック。
		/// </summary>
		private UnityAction onClickButtonAction = null;

		/// <summary>
		/// ボタンの表示テキストを適応する。
		/// </summary>
		/// <param name="title">表示テキスト</param>
		public abstract void SetTitle(string title);

		/// <summary>
		/// ボタン押下時のコールバックを設定する。
		/// </summary>
		/// <param name="onClickButtonAction">押下時のコールバック</param>
		public void SetClickCallback(UnityAction onClickButtonAction)
		{
			this.onClickButtonAction = onClickButtonAction;
		}

		/// <summary>
		/// 初期化処理
		/// </summary>
		protected override void Start()
		{
			base.Start();
			m_Button.onClick.AddListener(OnClickButton);
		}

		/// <summary>
		/// ボタンを押下した際の処理。
		/// </summary>
		private void OnClickButton()
		{
			if (onClickButtonAction != null) onClickButtonAction.Invoke();
		}

	}

}
