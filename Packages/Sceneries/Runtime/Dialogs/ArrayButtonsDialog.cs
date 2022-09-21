using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// 幾つかのボタンを設定できるダイアログ。
	/// 基本的に継承して使う。
	/// </summary>
	public class ArrayButtonsDialog : Dialog
	{

		/// <summary>
		/// ボタンの実体の配列。
		/// </summary>
		[SerializeField]
		private DialogButtonBase[] m_Buttons = null;

		/// <summary>
		/// ボタン押下時のコールバック。
		/// 任意設定用。
		/// </summary>
		private UnityAction[] buttonClickCallbacks = null;

		/// <summary>
		/// ボタン押下時のインデックス付きコールバック。
		/// 任意設定用。
		/// </summary>
		private UnityAction<int> buttonClickCallbackWithIndex = null;

		/// <summary>
		/// ボタンの実体の配列。
		/// </summary>
		protected DialogButtonBase[] buttons { get { return m_Buttons; } }

		/// <summary>
		/// ボタンの最大数。
		/// </summary>
		protected int maxButtonCount { get { return m_Buttons != null ? m_Buttons.Length : 0; } }

		/// <summary>
		/// 現在有効なボタンの数。
		/// </summary>
		protected int activeButtonCount { get { return m_Buttons != null ? m_Buttons.Count(b => b != null && b.gameObject.activeSelf) : 0; } }

		/// <summary>
		/// デフォルトのインデックス付きコールバック。
		/// 任意設定用。
		/// </summary>
		protected virtual UnityAction<int> defaultButtonClickCallbackWithIndex { get; }

		/// <summary>
		/// デフォルトのコールバック。
		/// </summary>
		protected virtual UnityAction defaultOnClickCallback { get; }

		/// <summary>
		/// ボタンの表示を有効にする。
		/// </summary>
		/// <param name="buttonCount">有効にしたいボタンの数</param>
		public void SetActiveButtons(int buttonCount)
		{
			for (int i = 0; i < m_Buttons.Length; i++)
			{
				if (!m_Buttons[i]) continue;
				m_Buttons[i].gameObject.SetActive(i < buttonCount);
			}
		}

		/// <summary>
		/// 全てのボタン押下コールバックをデフォルトで設定する。
		/// </summary>
		public void SetAllButtonsDefaultCallback()
		{
			SetAllButtonsCallback(null);
		}

		/// <summary>
		/// 全てのボタン押下コールバックを設定する。
		/// </summary>
		/// <param name="onClickCallback">押下時のコールバック</param>
		public void SetAllButtonsCallback(UnityAction onClickCallback)
		{
			for (int i = 0; i < buttonClickCallbacks.Length; i++)
			{
				buttonClickCallbacks[i] = onClickCallback != null ? onClickCallback : defaultOnClickCallback;
			}
		}

		/// <summary>
		/// 指定したボタンのコールバックを設定。
		/// </summary>
		/// <param name="index">ボタンの番号</param>
		/// <param name="onClickCallback">押下時のコールバック</param>
		public void SetButtonCallback(int index, UnityAction onClickCallback)
		{
			if (IsExceptionButtonIndex(index)) return;
			buttonClickCallbacks[index] = onClickCallback;
		}

		/// <summary>
		/// インデックス付きボタン押下コールバックを設定する。
		/// 全てのボタン押下に反応する。
		/// </summary>
		/// <param name="onClickCallback">押下時のコールバック</param>
		public void SetButtonCallbackWithIndex(UnityAction<int> onClickCallback)
		{
			buttonClickCallbackWithIndex = onClickCallback;
		}

		/// <summary>
		/// 指定したボタンの表示テキストを設定する。
		/// </summary>
		/// <param name="index">ボタンの番号</param>
		/// <param name="title">表示テキスト</param>
		public void SetButtonTitle(int index, string title)
		{
			if (IsExceptionButtonIndex(index)) return;
			m_Buttons[index].SetTitle(title);
		}

		/// <summary>
		/// ボタンの番号が例外かどうか取得する。
		/// </summary>
		/// <param name="index">ボタン番号</param>
		/// <returns>渡した番号が例外である</returns>
		private bool IsExceptionButtonIndex(int index)
		{
			if (index < 0) return true;
			if (m_Buttons != null && index >= m_Buttons.Length) return true;
			if (!m_Buttons[index]) return true;
			return false;
		}

		/// <summary>
		/// ボタン本体を取得する。
		/// 例外の場合nullを返す。
		/// </summary>
		/// <param name="index">ボタンの番号</param>
		/// <returns>ボタン</returns>
		protected DialogButtonBase GetButton(int index)
		{
			if (IsExceptionButtonIndex(index)) return null;
			return m_Buttons[index];
		}

		/// <summary>
		/// ダイアログインスタンス生成直後に呼び出される処理。
		/// </summary>
		protected override void OnInstancedDialog()
		{
			base.OnInstancedDialog();
			buttonClickCallbacks = new UnityAction[m_Buttons.Length];

			// 番号を渡すために初期コールバックを設定
			for (int i = 0; i < m_Buttons.Length; i++)
			{
				if (!m_Buttons[i]) continue;

				// 動的生成なので外部で定義
				int index = i;
				m_Buttons[i].SetClickCallback(() =>
				{
					if (buttonClickCallbacks[index] != null)
					{
						buttonClickCallbacks[index].Invoke();
					}
					else
					{
						defaultOnClickCallback?.Invoke();
					}
					if (buttonClickCallbackWithIndex != null)
					{
						buttonClickCallbackWithIndex.Invoke(index);
					}
					else
					{
						defaultButtonClickCallbackWithIndex?.Invoke(index);
					}
				});
			}
		}

	}

}
