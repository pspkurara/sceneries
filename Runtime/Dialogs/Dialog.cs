using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using Pspkurara.Sceneries.Transitions;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// 各ダイアログのインスタンス。
	/// </summary>
	[RequireComponent(typeof(GraphicRaycaster))]
	[RequireComponent(typeof(Canvas))]

	public class Dialog : UIBehaviour
	{

		/// <summary>
		/// ダイアログ消去後即時破棄するか。
		/// </summary>
		[SerializeField]
		private bool m_ImmediateDestroy = false;

		/// <summary>
		/// ダイアログ消去後即時破棄するか。
		/// </summary>
		internal bool immediateDestroy { get { return m_ImmediateDestroy; } }

		/// <summary>
		/// ダイアログが使用中か。
		/// </summary>
		internal bool isUsingDialog { get; private set; }

		/// <summary>
		/// 自身の<see cref="GraphicRaycaster"/>。
		/// <see cref="InstancedSetup"/>で取得される。
		/// </summary>
		protected GraphicRaycaster graphicRaycaster { get; private set; }

		/// <summary>
		/// 自身の<see cref="Canvas"/>。
		/// <see cref="InstancedSetup"/>で取得される。
		/// </summary>
		protected Canvas canvas { get; private set; }

		/// <summary>
		/// 画面遷移アニメーション。
		/// 任意アタッチ。
		/// </summary>
		protected ISceneTransition transition { get; private set; }

		/// <summary>
		/// 閉じるよう命令されたかのフラグ。
		/// 「閉じる」アクションをした際に有効になる。
		/// </summary>
		private bool forceClose = false;

		#region virtual functions

		/// <summary>
		/// ダイアログインスタンス生成直後に呼び出される処理。
		/// </summary>
		protected virtual void OnInstancedDialog(){}

		/// <summary>
		/// ダイアログ表示開始時に呼び出される処理。
		/// これからこの画面になる場合向け。
		/// </summary>
		protected virtual void OnStartInvationDialog(){}

		/// <summary>
		/// ダイアログ表示完了時に呼び出される処理。
		/// これからこの画面になる場合向け。
		/// </summary>
		protected virtual void OnCompletedInvationDialog(){}

		/// <summary>
		/// ダイアログ非表示開始時に呼び出される処理。
		/// 現在この画面の場合向け。
		/// </summary>
		protected virtual void OnStartDepartureDialog(){}

		/// <summary>
		/// ダイアログ非表示完了時に呼び出される処理。
		/// 現在この画面の場合向け。
		/// </summary>
		protected virtual void OnCompletedDepartureDialog(){}

		#endregion

		/// <summary>
		/// 生成時初期化処理を実行する。
		/// </summary>
		internal void InstancedSetup()
		{
			graphicRaycaster = GetComponent<GraphicRaycaster>();
			canvas = GetComponent<Canvas>();
			transition = GetComponent<ISceneTransition>();

			// 必ず初期化時は触れられないようにしておく
			graphicRaycaster.enabled = false;

			// 最初は非表示にする
			canvas.enabled = false;

			// 生成時に呼ばれる処理。
			OnInstancedDialog();
		}

		/// <summary>
		/// ダイアログを表示させる。
		/// 外部から操作する際は返り値のハンドラーを使う。
		/// </summary>
		/// <param name="onCloseDialog">ダイアログを閉じた際の処理ハンドラー</param>
		/// <returns>ダイアログ表示進捗ハンドラー</returns>
		internal OpenDialogHandler Show(ManagerToDialogHandler onCloseDialog)
		{
			OpenDialogHandler handler = new OpenDialogHandler(this);
			ShowInternal(handler, onCloseDialog);
			return handler;
		}

		/// <summary>
		/// ダイアログ表示の内部処理。
		/// 非同期だがハンドラーをすぐ返す必要があるので<see cref="Show(ManagerToDialogHandler)"/>とは分離されている。
		/// </summary>
		/// <param name="handler">ダイアログ表示進捗ハンドラー</param>
		/// <param name="onCloseDialog">ダイアログを閉じた際の処理ハンドラー</param>
		private async void ShowInternal(OpenDialogHandler handler, ManagerToDialogHandler onCloseDialog)
		{
			// 閉じるフラグを初期化
			forceClose = false;

			// 表示中である
			isUsingDialog = true;

			// オブジェクトの表示
			gameObject.SetActive(true);

			// ハンドラー側でも表示フラグを立てる
			handler.isOpening = true;

			// 遷移アニメーションを持つか
			bool hasTransition = transition != null;

			// 遷移アニメーション開始処理
			OnStartInvationDialog();

			// レイアウト用に1フレーム待機
			await UniTask.Yield();

			// 表示させる
			canvas.enabled = true;

			// 遷移アニメーションを動かす
			if (hasTransition) await transition.PlayInvationAnimationAndWaitComplete();

			// 遷移アニメーション完了処理
			OnCompletedInvationDialog();

			// タッチ可能にする
			graphicRaycaster.enabled = true;

			// 閉じるまで待つ
			await UniTask.WaitWhile(() => { return !forceClose; });

			// タッチ不可にする
			graphicRaycaster.enabled = false;

			// 遷移アニメーション開始処理
			OnStartDepartureDialog();

			// 閉じはじめを通知する
			onCloseDialog.OnCloseDialog?.Invoke();

			// 遷移アニメーションを動かす
			if (hasTransition) await transition.PlayDepartureAnimationAndWaitComplete();

			// 非表示にさせる
			canvas.enabled = false;

			// 遷移アニメーション完了処理
			OnCompletedDepartureDialog();

			// ハンドラー側は表示フラグをオフにする
			handler.isOpening = false;

			// オブジェクトを非表示
			gameObject.SetActive(false);

			// 使用中フラグもオフ
			isUsingDialog = false;

			// 閉じきった通知をする
			onCloseDialog.OnCompletedCloseDialog?.Invoke(this);
		}

		/// <summary>
		/// 閉じた通知を行う。
		/// </summary>
		public void Close()
		{
			// フラグを立てる
			forceClose = true;
		}

	}

}
