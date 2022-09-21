using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using Pspkurara.Sceneries.Transitions;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// ダイアログの背景オブジェクト。
	/// </summary>
	internal sealed class DialogBackground : UIBehaviour
	{

		/// <summary>
		/// 画面遷移アニメーション。
		/// 任意アタッチ。
		/// </summary>
		private ISceneTransition transition { get; set; }

		/// <summary>
		/// 生成時処理
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			transition = GetComponent<ISceneTransition>();
			gameObject.SetActive(false);
		}

		/// <summary>
		/// ソート順を設定。
		/// </summary>
		/// <param name="dialog">現在最も手前にあるダイアログ</param>
		public void SetSibilingIndex(Dialog dialog)
		{
			int activeDialogSibiling = dialog.transform.GetSiblingIndex();
			transform.SetSiblingIndex(activeDialogSibiling - 1);
		}

		/// <summary>
		/// 背景を表示させる。
		/// </summary>
		public async void Show()
		{
			gameObject.SetActive(true);
			if (transition != null) await transition.PlayInvationAnimationAndWaitComplete();
		}

		/// <summary>
		/// 背景を非表示にする。
		/// </summary>
		public async void Hide()
		{
			if (transition != null) await transition.PlayDepartureAnimationAndWaitComplete();
			gameObject.SetActive(false);
		}

	}

}
