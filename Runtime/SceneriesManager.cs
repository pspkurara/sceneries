using System.Collections.Generic;
using UnityEngine;
using Pspkurara.Singleton;
using Cysharp.Threading.Tasks;
using Pspkurara.Sceneries.Loaders;
using System.Threading;

namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 画面管理を行うマネージャー。
	/// </summary>
	public sealed class SceneriesManager : InternalSingletonUIBehaviour<SceneriesManager>
	{

		#region const

		/// <summary>
		/// 標準の画面履歴ID。
		/// </summary>
		public const int DefaultSceneriesHistoryId = 0;

		#endregion

		#region instance

		/// <summary>
		/// 画面ルートオブジェクトのタグ。
		/// オブジェクトはひとつである必要がある。
		/// </summary>
		[SerializeField]
		private string m_SceneriesRootTag = "SceneriesRoot";

		/// <summary>
		/// 画面プレハブのパス。
		/// </summary>
		[SerializeField]
		private SceneriesPrefabLoaderBase m_PrefabLoader = null;

		/// <summary>
		/// 画面インスタンスのキャッシュの最大数。
		/// 超えたら最もアクセスが古いものから順に削除される。
		/// </summary>
		[SerializeField]
		private int m_MaxHistoryCount = 5;

		/// <summary>
		/// 初回ロードで呼び出される画面名。
		/// </summary>
		[SerializeField]
		private string m_InitializeLoadSceneriesName = string.Empty;

		/// <summary>
		/// 画面遷移の最中は追加で遷移の呼び出しをしない。
		/// </summary>
		[SerializeField]
		private bool m_SkipCallIsTransitioning = false;

		/// <summary>
		/// 画面のソート順を生成順に固定する。
		/// </summary>
		[SerializeField]
		private bool m_MatchSceneriesOrderWithGenerated = false;

		/// <summary>
		/// 裏側にある画面のアニメーションを行わない。
		/// </summary>
		[SerializeField]
		private bool m_SkipAnimationUnderSceneries = false;

		#endregion

		#region instance to static params

		/// <summary>
		/// 画面ルートオブジェクトのタグ。
		/// オブジェクトはひとつである必要がある。
		/// </summary>
		private static string sceneriesRootTag {
			get {
				return instance ? instance.m_SceneriesRootTag : string.Empty;
			}
		}

		/// <summary>
		/// 画面インスタンスのキャッシュの最大数。
		/// 超えたら最もアクセスが古いものから順に削除される。
		/// </summary>
		private static int maxHistoryCount {
			get {
				return instance ? instance.m_MaxHistoryCount : int.MaxValue;
			}
		}

		/// <summary>
		/// 画面遷移の最中は追加で遷移の呼び出しをしない。
		/// </summary>
		private static bool isSkipCallIsTransitioning {
			get {
				return instance ? instance.m_SkipCallIsTransitioning : false;
			}
		}

		#endregion

		#region static

		/// <summary>
		/// 画面遷移中か。
		/// </summary>
		public static bool isTransitioning { get; private set; }

		/// <summary>
		/// 画面遷移をスキップする必要があるか。
		/// </summary>
		private static bool isForceSkipTransitioning { get { return isSkipCallIsTransitioning && isTransitioning; } }

		/// <summary>
		/// 画面インスタンスを置くルートオブジェクト。
		/// </summary>
		private static RectTransform m_SceneriesRoot;

		/// <summary>
		/// 画面インスタンスを置くルートオブジェクト。
		/// </summary>
		private static RectTransform sceneriesRoot {
			get {
				if(m_SceneriesRoot == null)
				{
					// タグで探す
					var taggedObject = GameObject.FindGameObjectWithTag(sceneriesRootTag);
					if(taggedObject != null)
					{
						// あったら格納
						m_SceneriesRoot = taggedObject.transform as RectTransform;
					}
				}
				// キャッシュを返す
				return m_SceneriesRoot;
			}
		}

		/// <summary>
		/// 画面の履歴。
		/// 画面インスタンスに依存せずすべて保存。
		/// </summary>
		private static Stack<SceneStackData> m_SceneHistory = new Stack<SceneStackData>();

		/// <summary>
		/// 画面インスタンスのキャッシュ。
		/// 一定数を超えると破棄される。
		/// </summary>
		private static Dictionary<string, Scene> m_SceneCache = new Dictionary<string, Scene>();

		/// <summary>
		/// マネージャー側から画面に渡すデータを管理するハンドラー。
		/// </summary>
		private static ManagerToSceneHandler m_ManagerToSceneHandler = new ManagerToSceneHandler();

		#endregion

		/// <summary>
		/// 過去にロードした画面を巻き戻って取得する
		/// </summary>
		/// <param name="backCount">戻る数</param>
		/// <returns>過去にロードした画面名</returns>
		public static string GetSceneHistory(int backCount = 1)
		{
			var history = new Stack<SceneStackData>(m_SceneHistory);
			// 画面履歴を戻していく
			SceneStackData stackData = history.Peek();
			for (int i = 0; i < backCount; i++)
			{
				history.Pop();
				stackData = history.Peek();
			}
			return stackData.SceneriesName;
		}

		/// <summary>
		/// 画面をロードしてアクティブにする。
		/// </summary>
		/// <param name="sceneriesName">画面の名前</param>
		/// <returns>生成された画面履歴ID</returns>
		public static int LoadScene(string sceneriesName)
		{
			return LoadScene(sceneriesName, null);
		}

		/// <summary>
		/// 画面をロードしてアクティブにする。
		/// </summary>
		/// <param name="sceneriesName">画面の名前</param>
		/// <returns>生成された画面履歴ID</returns>
		public static int LoadScene(string sceneriesName, SceneInitData initData)
		{
			int historyId = CreateSceneriesHistoryId(sceneriesName);
			AddSceneHistory(sceneriesName, historyId, initData);
			return historyId;
		}

		/// <summary>
		/// 画面をロードしてアクティブにする。
		/// 現在の画面は<see cref="UndoScene"/>で検知されなくなる。
		/// </summary>
		/// <param name="sceneriesName">画面の名前</param>
		/// <returns>生成された画面履歴ID</returns>
		public static int LoadSceneWithNonUndo(string sceneriesName)
		{
			return LoadSceneWithNonUndo(sceneriesName, null);
		}

		/// <summary>
		/// 画面をロードしてアクティブにする。
		/// 現在の画面は<see cref="UndoScene"/>で検知されなくなる。
		/// </summary>
		/// <param name="sceneriesName">画面の名前</param>
		/// <returns>生成された画面履歴ID</returns>
		public static int LoadSceneWithNonUndo(string sceneriesName, SceneInitData initData)
		{
			int historyId = CreateSceneriesHistoryId(sceneriesName);
			AddSceneHistory(sceneriesName, historyId, initData, 1);
			return historyId;
		}

		/// <summary>
		/// 画面を戻る。
		/// </summary>
		/// <param name="backCount">戻る数</param>
		/// <returns>見つかった画面履歴ID</returns>
		public static int UndoScene(int backCount = 1)
		{
			// 遷移中はスキップ
			if (isForceSkipTransitioning) return DefaultSceneriesHistoryId;

			// 数が不正なら何もしない
			if (backCount <= 0) return DefaultSceneriesHistoryId;

			// 履歴数が空なら何もしない
			if (m_SceneHistory.Count <= 0) return DefaultSceneriesHistoryId;

			// 現在の画面を取っておく
			var current = GetCurrentActiveSceneCache();

			// 画面履歴を戻していく
			SceneStackData stackData = null;
			for (int i = 0; i < backCount; i++)
			{
				m_SceneHistory.Pop();
				stackData = m_SceneHistory.Peek();
			}

			// ひとつも見つからないなら何もしない
			if (stackData == null) return DefaultSceneriesHistoryId;

			// 画面遷移開始
			UndoScene(current, stackData);

			return stackData.Id;
		}

		/// <summary>
		/// 指定した画面IDが見つかるまで画面を戻る。
		/// </summary>
		/// <param name="undoTargetSceneriesName">戻り先の画面ID</param>
		/// <returns>見つかった画面履歴ID</returns>
		public static int UndoSceneFromTarget(string undoTargetSceneriesName)
		{
			// 遷移中はスキップ
			if (isForceSkipTransitioning) return DefaultSceneriesHistoryId;

			// 画面IDが空なら何もしない
			if (string.IsNullOrEmpty(undoTargetSceneriesName)) return DefaultSceneriesHistoryId;

			// 履歴数が空なら何もしない
			if (m_SceneHistory.Count <= 0) return DefaultSceneriesHistoryId;

			// 現在の画面を取っておく
			var current = GetCurrentActiveSceneCache();

			// 画面履歴をすべてチェック
			SceneStackData foundHistory = null;
			// 現在の画面を含めないようにする
			int foundPopCount = -1;
			foreach (var history in m_SceneHistory)
			{ 
				// 数を数える
				foundPopCount++;
				// 一致する画面IDを使用する履歴を発見したら格納
				if (history.SceneriesName != undoTargetSceneriesName) continue;
				foundHistory = history;
				break;
			}

			// ひとつも見つからないなら何もしない
			if (foundHistory == null) return DefaultSceneriesHistoryId;

			// 履歴を消去する
			for (int i = 0; i < foundPopCount; i++)
			{
				m_SceneHistory.Pop();
			}

			// 画面遷移開始
			UndoScene(current, foundHistory);

			return foundHistory.Id;
		}

		/// <summary>
		/// 指定した画面履歴IDが見つかるまで画面を戻る。
		/// </summary>
		/// <param name="sceneriesHistoryId">戻り先の画面履歴ID</param>
		/// <returns>正しく遷移を開始したか</returns>
		public static bool UndoSceneFromTarget(int sceneriesHistoryId)
		{
			// 画面履歴IDが標準値なら何もしない
			if (sceneriesHistoryId == DefaultSceneriesHistoryId) return false;

			// 遷移中はスキップ
			if (isForceSkipTransitioning) return false;

			// 履歴数が空なら何もしない
			if (m_SceneHistory.Count <= 0) return false;

			// 現在の画面を取っておく
			var current = GetCurrentActiveSceneCache();

			// 画面履歴をすべてチェック
			SceneStackData foundHistory = null;
			// 現在の画面を含めないようにする
			int foundPopCount = -1;
			foreach (var history in m_SceneHistory)
			{
				// 数を数える
				foundPopCount++;
				// 一致する画面IDを使用する履歴を発見したら格納
				if (history.Id != sceneriesHistoryId) continue;
				foundHistory = history;
				break;
			}

			// ひとつも見つからないなら何もしない
			if (foundHistory == null) return false;

			// 履歴を消去する
			for (int i = 0; i < foundPopCount; i++)
			{
				m_SceneHistory.Pop();
			}

			// 画面遷移開始
			UndoScene(current, foundHistory);

			return true;
		}

		/// <summary>
		/// 画面をロードしてキャッシュに追加する
		/// すぐにアクティブにはならない
		/// </summary>
		/// <param name="sceneriesName">画面の名前</param>
		public static void LoadSceneForCacheOnly(string sceneriesName)
		{
			GetOrCreateSceneCache(sceneriesName).Forget();
		}

		/// <summary>
		/// 指定した画面に戻る。
		/// </summary>
		/// <param name="currentActiveScene">現在アクティブな画面</param>
		/// <param name="sceneriesHistory">戻りたい画面履歴</param>
		private static async void UndoScene(Scene currentActiveScene, SceneStackData sceneriesHistory)
		{
			// 画面遷移開始
			StartTransition();

			// 画面インスタンスを取得して表示させる
			var scene = await GetOrCreateSceneCache(sceneriesHistory.SceneriesName);

			// インスタンスの取得に失敗したら何もしない
			if (!scene)
			{
				FinishTransition();
				return;
			}

			scene.SetInitData(sceneriesHistory.InitData);

			// 画面の情報を反映
			// 今からアクティブになる画面は裏側に出てくる。
			m_ManagerToSceneHandler.IsActiveScreenIsUnder = true;

			// 画面を切り替える
			await scene.SetActive(currentActiveScene, m_ManagerToSceneHandler);

			// 画面遷移終了
			FinishTransition();
		}

		/// <summary>
		/// 履歴から現在アクティブな画面を取得する。
		/// 現在の履歴を参照するので履歴改変前に呼び出すこと。
		/// </summary>
		/// <returns>現在アクティブな画面</returns>
		private static Scene GetCurrentActiveSceneCache()
		{
			// 履歴数が空なら何もしない (最初の画面だけは残る)
			if (m_SceneHistory.Count <= 0) return null;

			// 現在の画面キーを取得する
			string sceneriesName = m_SceneHistory.Peek().SceneriesName;

			// 該当キーのオブジェクトが存在したら返す
			if (m_SceneCache.ContainsKey(sceneriesName))
			{
				return m_SceneCache[sceneriesName];
			}

			return null;
		}

		/// <summary>
		/// 画面履歴を追加する
		/// </summary>
		/// <param name="sceneriesName">画面ID</param>
		/// <param name="sceneriesHistoryId">画面履歴ID</param>
		/// <param name="initData">画面個別初期化用データ</param>
		/// <param name="removeSceneHistoryCount">直前の画面履歴を消去する数</param>
		private static async void AddSceneHistory(string sceneriesName, int sceneriesHistoryId, SceneInitData initData, int removeSceneHistoryCount = 0)
		{
			// 遷移中はスキップ
			if (isForceSkipTransitioning) return;

			// 画面遷移開始
			StartTransition();

			// 現在の画面を取っておく
			var current = GetCurrentActiveSceneCache();

			// 画面インスタンスを取得して表示させる
			var scene = await GetOrCreateSceneCache(sceneriesName);

			// インスタンスの取得に失敗したら何もしない
			if (!scene)
			{
				FinishTransition();
				return;
			}

			scene.SetInitData(initData);

			// 画面の情報を反映
			// 今からアクティブになる画面は表側に出てくる。
			m_ManagerToSceneHandler.IsActiveScreenIsUnder = false;

			// 画面を切り替える
			await scene.SetActive(current, m_ManagerToSceneHandler);

			// 直前の画面履歴を指定数消す
			for (int i = 0; i < removeSceneHistoryCount; i++)
			{
				m_SceneHistory.Pop();
			}

			// 画面履歴を生成
			var stackData = new SceneStackData();
			stackData.Id = sceneriesHistoryId;
			stackData.SceneriesName = sceneriesName;
			if (initData != null) stackData.InitData = initData.GetInitDataForHistory();

			// 履歴に追加
			m_SceneHistory.Push(stackData);

			// 画面遷移終了
			FinishTransition();
		}

		/// <summary>
		/// 画面履歴IDを生成する。
		/// </summary>
		/// <param name="sceneriesName">画面ID</param>
		/// <returns>画面履歴ID</returns>
		private static int CreateSceneriesHistoryId(string sceneriesName)
		{
			return sceneriesName.GetHashCode() + System.DateTime.UtcNow.GetHashCode();
		}

		/// <summary>
		/// 画面インスタンスをキャッシュから取得して返す。
		/// なければ生成する。
		/// </summary>
		/// <param name="sceneriesName">画面ID</param>
		/// <returns>非同期タスク及び<see cref="UniTask{T}.Result"/></returns>
		private static async UniTask<Scene> GetOrCreateSceneCache(string sceneriesName)
		{
			// 画面IDが正しくない場合はエラーを出す
			if (string.IsNullOrEmpty(sceneriesName))
			{
				Debug.LogError("画面IDが空です");
				return null;
			}

			// キーがあるならそのままロードして返す
			if (m_SceneCache.ContainsKey(sceneriesName))
			{
				return m_SceneCache[sceneriesName];
			}

			// 画面インスタンスの数が規定値をオーバーしているか
			if (m_SceneCache.Count >= maxHistoryCount)
			{
				// 最も古い画面インスタンスを探す
				string removeTargetSceneriesName = null;
				System.DateTime mostOldTime = System.DateTime.UtcNow;
				foreach (var scene in m_SceneCache)
				{
					// 画面インスタンスの最後にアクティブになった日と比較
					if (scene.Value.lastActivatedTime < mostOldTime)
					{
						// 古かったら削除対象に指定
						mostOldTime = scene.Value.lastActivatedTime;
						removeTargetSceneriesName = scene.Key;
					}
				}

				// 画面インスタンスを削除する
				if (removeTargetSceneriesName != null)
				{
					m_SceneCache[removeTargetSceneriesName].Destroy();
					m_SceneCache.Remove(removeTargetSceneriesName);
				}
			}

			// プレハブをロードする
			// ロード完了まで待つ
			var result = await instance.m_PrefabLoader.LoadPrefab(sceneriesName, CancellationToken.None);

			// 結果を格納するために用意
			Scene instancedScene = null;

			// 成功していたらインスタンスする
			if (result != null)
			{
				// ロードしたプレハブをインスタンスする
				var instancedPrefab = Instantiate(result as GameObject, sceneriesRoot);
				instancedPrefab.name = sceneriesName;

				// 画面インスタンスを取得する
				instancedScene = instancedPrefab.gameObject.GetComponent<Scene>();

				// 画面インスタンスがなかったらエラーを出す
				if (!instancedScene)
				{
					Debug.LogErrorFormat("画面インスタンスのコンポーネントが存在しません [{0}]", sceneriesName);
					return instancedScene;
				}

				await instancedScene.InstancedSetup();

				// キャッシュに追加する
				m_SceneCache.Add(sceneriesName, instancedScene);
			}
			// 失敗したらエラーを出す
			else
			{
				Debug.LogErrorFormat("画面のロードに失敗しました [{0}]",sceneriesName);
			}

			// 結果を返す
			return instancedScene;
		}

		/// <summary>
		/// 画面遷移開始。
		/// </summary>
		private static void StartTransition()
		{
			isTransitioning = true;
		}

		/// <summary>
		/// 画面遷移終了。
		/// </summary>
		private static void FinishTransition()
		{
			isTransitioning = false;
		}

		/// <summary>
		/// 画面の履歴情報を取得する。
		/// </summary>
		/// <returns>画面履歴の情報</returns>
		public override string ToString()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			foreach (var history in m_SceneHistory)
			{
				stringBuilder.AppendFormat("{0} / {1} / {2}", history.Id, history.SceneriesName, history.InitData);
				stringBuilder.Append('\n');
			}
			return stringBuilder.ToString();
		}


		#region unity magic methods

		/// <summary>
		/// 生成時処理
		/// </summary>
		protected override void Awake()
		{
			base.Awake();

			// NaNチェック
			SceneriesUtility.WarningIncorrectRectTransform(sceneriesRoot);

			// 破棄防止
			DontDestroyOnLoad(gameObject);

			// 設定初期化
			m_ManagerToSceneHandler.IsMaintainOrder = m_MatchSceneriesOrderWithGenerated;
			m_ManagerToSceneHandler.IsSkipUnderUndoAnimation = m_SkipAnimationUnderSceneries;

			// 初期画面を読み込む。
			if (!string.IsNullOrEmpty(m_InitializeLoadSceneriesName)) LoadScene(m_InitializeLoadSceneriesName);
		}

		#if UNITY_EDITOR

		/// <summary>
		/// インスペクター更新時の処理
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate();

			// いじったら設定反映
			m_ManagerToSceneHandler.IsMaintainOrder = m_MatchSceneriesOrderWithGenerated;
			m_ManagerToSceneHandler.IsSkipUnderUndoAnimation = m_SkipAnimationUnderSceneries;
		}

		#endif

		#endregion

	}

	/// <summary>
	/// 画面ヒストリーのデータパック。
	/// インスタンス時のプリデータ等を格納。
	/// 画面インスタンス自体はプールして使いまわすので各ヒストリー固有のデータをここに入れる。
	/// </summary>
	public sealed class SceneStackData
	{

		/// <summary>
		/// 画面履歴ID。
		/// </summary>
		public int Id;

		/// <summary>
		/// 画面インスタンスの名前。
		/// </summary>
		public string SceneriesName;

		/// <summary>
		/// 画面個別初期化用のパラメーター郡。
		/// </summary>
		public SceneInitData InitData;

	}

	/// <summary>
	/// マネージャー側から画面に渡すデータを管理するハンドラー。
	/// </summary>
	internal sealed class ManagerToSceneHandler
	{

		/// <summary>
		/// 画面のソート順を維持する。
		/// </summary>
		public bool IsMaintainOrder;

		/// <summary>
		/// ソート順を維持された際に裏側にある画面のアニメーションを無効化する。
		/// </summary>
		public bool IsSkipUnderUndoAnimation;

		#region temprary

		/// <summary>
		/// 今からアクティブなる画面は裏側であるか。
		/// </summary>
		public bool IsActiveScreenIsUnder;

		#endregion

	}

}
