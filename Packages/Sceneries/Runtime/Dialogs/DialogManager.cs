using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using Pspkurara.Singleton;
using System.Threading;
using Pspkurara.Sceneries.Loaders;

namespace Pspkurara.Sceneries.Dialogs
{

	/// <summary>
	/// ダイアログ管理を行うマネージャー。
	/// </summary>
	public sealed class DialogManager : SingletonUIBehaviour<DialogManager>
	{

		/// <summary>
		/// ダイアログの各要素に値を当て込む初期化処理。
		/// </summary>
		/// <typeparam name="T">ダイアログインスタンスの型</typeparam>
		/// <param name="dialogInstance">ダイアログインスタンス本体</param>
		public delegate void ApplyDialogElements<T>(T dialogInstance) where T : Dialog;

		#region instance

		/// <summary>
		/// 画面ルートオブジェクトのタグ。
		/// オブジェクトはひとつである必要がある。
		/// </summary>
		[SerializeField]
		private string m_DialogRootTag = "DialogsRoot";

		/// <summary>
		/// 画面のプレハブパス。
		/// </summary>
		[SerializeField]
		private SceneriesPrefabLoaderBase m_DialogPrefabLoader = null;

		/// <summary>
		/// ダイアログの背景オブジェクトのパス。
		/// 空の場合は使用されない
		/// </summary>
		[SerializeField]
		private string m_DialogBackgroundName = "Background";

		#endregion

		#region instance to static params

		/// <summary>
		/// ダイアログルートオブジェクトのタグ。
		/// オブジェクトはひとつである必要がある。
		/// </summary>
		private static string dialogRootTag {
			get {
				return instance ? instance.m_DialogRootTag : string.Empty;
			}
		}

		#endregion

		#region static

		/// <summary>
		/// ダイアログインスタンスを置くルートオブジェクト。
		/// </summary>
		private static RectTransform m_DialogRoot;

		/// <summary>
		/// ダイアログインスタンスを置くルートオブジェクト。
		/// </summary>
		private static RectTransform dialogRoot {
			get {
				if (m_DialogRoot == null)
				{
					// タグで探す
					var taggedObject = GameObject.FindGameObjectWithTag(dialogRootTag);
					if (taggedObject != null)
					{
						// あったら格納
						m_DialogRoot = taggedObject.transform as RectTransform;
					}
				}
				// キャッシュを返す
				return m_DialogRoot;
			}
		}

		/// <summary>
		/// ダイアログの背景オブジェクト。
		/// </summary>
		private static DialogBackground m_Background;

		/// <summary>
		/// 共通背景を使用する
		/// </summary>
		private static bool useBackground { get { return !string.IsNullOrEmpty(instance.m_DialogBackgroundName); } }

		/// <summary>
		/// 現在表示中のダイアログのリスト。
		/// 表示が切り替わると動的に増減する。
		/// </summary>
		private static List<Dialog> m_ActiveDialogs = new List<Dialog>();

		/// <summary>
		/// ダイアログインスタンスのキャッシュ。
		/// 名前で管理し、生成したオブジェクトを全て格納。
		/// </summary>
		private static Dictionary<string, DialogCacheData> m_DialogCache = new Dictionary<string, DialogCacheData>();

		#endregion

		/// <summary>
		/// ダイアログを生成して表示する。
		/// </summary>
		/// <typeparam name="T">ダイアログの型</typeparam>
		/// <param name="dialogName">ダイアログID</param>
		/// <param name="applyElements">ダイアログの各要素に値を当て込む初期化処理</param>
		/// <param name="cancellationToken">キャンセルトークン</param>
		/// <returns>非同期待機、<see cref="Unita"/>ダイアログの表示進捗ハンドラー</returns>
		public static async UniTask<OpenDialogHandler> ShowDialog<T>(string dialogName, ApplyDialogElements<T> applyElements, CancellationToken cancellationToken = default) where T : Dialog
		{
			// ダイアログのインスタンスを生成及び取得する
			var load = await GetNewDialog(dialogName, cancellationToken);

			// 型を指定したものに変換してチェック
			var result = load as T;

			// ダイアログが正しくインスタンスできなければ終了
			if (!result) return null;

			// ダイアログを表示中リストに追加
			m_ActiveDialogs.Add(result);

			// 背景をロード
			var background = await GetOrCreateBackground(cancellationToken);

			// ソート順を設定。
			result.transform.SetAsLastSibling();
			if (background) background.SetSibilingIndex(result);

			// 初期状態を反映
			applyElements?.Invoke(result);

			// マネージャー側から「閉じた際の動作」をハンドラー経由で渡す
			ManagerToDialogHandler handler;
			// 残りダイアログ数が1つだけの場合
			if (m_ActiveDialogs.Count == 1)
			{
				// 次に閉じたら背景も非表示にする
				handler = new ManagerToDialogHandler(background ? background.Hide : default(UnityAction), RemoveClosedDialog);
				if (background) background.Show();
			}
			else
			{
				// 閉じたあとリストから消去する処理だけ渡す
				handler = new ManagerToDialogHandler(null, RemoveClosedDialog);
			}

			// 表示してダイアログの表示進捗ハンドラーを返す
			return result.Show(handler);
		}

		/// <summary>
		/// ダイアログを生成して表示する。
		/// </summary>
		/// <typeparam name="T">ダイアログの型</typeparam>
		/// <param name="dialogName">ダイアログID</param>
		/// <param name="applyElements">ダイアログの各要素に値を当て込む初期化処理</param>
		/// <param name="cancellationToken">キャンセルトークン</param>
		/// <param name="closeWithCancellation">キャンセルで自動で閉じさせる</param>
		/// <param name="waitForCloseComplete">閉きるまで完全に待機する</param>
		/// <returns>非同期待機、<see cref="Unita"/>ダイアログの表示進捗ハンドラー</returns>
		public static async UniTask ShowDialogAndWait<T>(string dialogName, ApplyDialogElements<T> applyElements, CancellationToken cancellationToken = default, bool closeWithCancellation = true, bool waitForCloseComplete = false) where T : Dialog
		{
			// ダイアログのインスタンスを生成及び取得する
			var handler = await ShowDialog(dialogName, applyElements, cancellationToken);

			// 処理完了まで待つ
			while (handler.isOpening)
			{
				// 待機
				await UniTask.Yield(cancellationToken);
				// キャンセルで閉じるが有効な場合にキャンセルされたら閉じる
				if (closeWithCancellation && cancellationToken.IsCancellationRequested)
				{
					handler.ForceClose();
					break;
				}
			}
			if (waitForCloseComplete)
			{
				// 閉じきるまで待機する
				await UniTask.WaitWhile(() => handler.isOpening);
			}
		}

		/// <summary>
		/// ダイアログインスタンスを生成及び取得する。
		/// </summary>
		/// <param name="dialogName">ダイアログID</param>
		/// <param name="cancellationToken">キャンセルトークン</param>
		/// <returns>非同期待機、<see cref="UniTask{T}.Result"/>に取得したダイアログインスタンス</returns>
		private static async UniTask<Dialog> GetNewDialog(string dialogName, CancellationToken cancellationToken)
		{

			// ダイアログIDが正しくない場合はエラーを出す
			if (string.IsNullOrEmpty(dialogName))
			{
				Debug.LogError("ダイアログIDが空です");
				return null;
			}

			// キーがないなら新しく追加
			if (!m_DialogCache.ContainsKey(dialogName))
			{
				m_DialogCache.Add(dialogName, new DialogCacheData());
			}

			// ダイアログのインスタンス保持対象を取得
			var currentDialog = m_DialogCache[dialogName];

			// 使用していないダイアログを取得
			Dialog dialogInstance = currentDialog.GetUnusingCachedDialog();

			// キャッシュがない場合
			if (!dialogInstance)
			{
				// プレハブをロードする
				// ロード完了まで待つ
				var result = await instance.m_DialogPrefabLoader.LoadPrefab(dialogName, cancellationToken);

				if (cancellationToken.IsCancellationRequested) return null;

				// 成功していたらインスタンスする
				if (result != null)
				{
					// ロードしたプレハブをインスタンスする
					var instancedPrefab = Instantiate(result, dialogRoot);
					instancedPrefab.name = string.Format("{0} ({1})", dialogName, currentDialog.Count + 1);

					// ダイアログインスタンスを取得する
					dialogInstance = instancedPrefab.gameObject.GetComponent<Dialog>();
					dialogInstance.InstancedSetup();

					// 作ったインスタンスを格納
					currentDialog.AddInstance(dialogInstance);
				}
				// 失敗したらエラーを出す
				else
				{
					Debug.LogErrorFormat("ダイアログのロードに失敗しました [{0}]", dialogName);
				}
			}

			// 結果を返す
			return dialogInstance;

		}

		/// <summary>
		/// 背景オブジェクトを生成する。
		/// </summary>
		private static async UniTask<DialogBackground> GetOrCreateBackground(CancellationToken cancelToken)
		{
			// 既にロード済み
			if (m_Background != null) return m_Background;
			// ロード不要
			if (!useBackground) return null;

			// ロード
			var bgPrefab = await instance.m_DialogPrefabLoader.LoadPrefab(instance.m_DialogBackgroundName, cancelToken);
			if (bgPrefab != null)
			{
				// 見つかったら生成
				var bgGameObject = Instantiate(bgPrefab, dialogRoot);
				bgGameObject.name = bgPrefab.name;
				m_Background = bgGameObject.GetComponent<DialogBackground>();
				return m_Background;
			}
			else
			{
				// なければエラー
				Debug.LogErrorFormat("ダイアログ背景プレハブが見つかりません [{0}]", instance.m_DialogBackgroundName);
				return null;
			}
		}

		/// <summary>
		/// 閉じた後のダイアログを表示中リストから破棄する処理。
		/// </summary>
		/// <param name="closedDialog">閉じたダイアログ</param>
		private static void RemoveClosedDialog(Dialog closedDialog)
		{
			// 一覧から削除
			m_ActiveDialogs.Remove(closedDialog);

			// 即時消去オプションが設定されている
			if (closedDialog.immediateDestroy)
			{
				// 消去もする
				Destroy(closedDialog.gameObject);
			}
		}

		#region unity magic methods

		/// <summary>
		/// 生成時処理
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			// NaNチェック
			SceneriesUtility.WarningIncorrectRectTransform(dialogRoot);

			// 破棄防止
			DontDestroyOnLoad(gameObject);

			// 背景を生成
			if (useBackground) GetOrCreateBackground(CancellationToken.None).Forget();
		}

		#endregion

	}

	/// <summary>
	/// ダイアログキャッシュのデータを管理。
	/// </summary>
	internal sealed class DialogCacheData
	{

		/// <summary>
		/// 現在保持しているダイアログのインスタンス郡。
		/// </summary>
		private List<Dialog> dialogInstances = new List<Dialog>();

		/// <summary>
		/// 現在未使用のダイアログインスタンスを取得する。
		/// 見つからなければ<see cref="null"/>を返す。
		/// </summary>
		/// <returns>見つけたダイアログインスタンス</returns>
		public Dialog GetUnusingCachedDialog()
		{
			// ダイアログを全チェック
			foreach (var dialog in dialogInstances)
			{
				// 使用中ならスキップ
				if (dialog.isUsingDialog) continue;
				// 見つけたら返す
				return dialog;
			}
			// 何も見つからなかった
			return null;
		}

		/// <summary>
		/// 生成したダイアログインスタンスをキャッシュに追加
		/// </summary>
		/// <param name="dialog">追加するダイアログ</param>
		public void AddInstance(Dialog dialog)
		{
			dialogInstances.Add(dialog);
		}

		/// <summary>
		/// 現在のダイアログインスタンスの数。
		/// </summary>
		public int Count { get { return dialogInstances.Count; } }

	}

	/// <summary>
	/// <see cref="DialogManager"/>側の閉じる処理を<see cref="Dialog"/>に渡す際のハンドラー。
	/// </summary>
	internal struct ManagerToDialogHandler
	{

		/// <summary>
		/// ハンドラーを生成。
		/// </summary>
		/// <param name="onCloseDialog">閉じ始めの際に呼び出される処理</param>
		/// <param name="onCompletedCloseDialog">閉じきった後に呼び出される処理</param>
		public ManagerToDialogHandler(UnityAction onCloseDialog, UnityAction<Dialog> onCompletedCloseDialog)
		{
			OnCloseDialog = onCloseDialog;
			OnCompletedCloseDialog = onCompletedCloseDialog;
		}

		/// <summary>
		/// 閉じ始めの際に呼び出される処理
		/// </summary>
		public readonly UnityAction OnCloseDialog;

		/// <summary>
		/// 閉じきった後に呼び出される処理
		/// </summary>
		public readonly UnityAction<Dialog> OnCompletedCloseDialog;


	}

}
