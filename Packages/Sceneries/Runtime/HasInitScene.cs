using UnityEngine;

namespace Pspkurara.Sceneries {

	/// <summary>
	/// 個別初期化に対応した各画面インスタンス。
	/// </summary>
	/// <typeparam name="T">初期化用データの型</typeparam>
	public abstract class HasInitScene<T> : Scene, ISceneInitialize where T : SceneInitData
	{

		/// <summary>
		/// 画面を初期化する際に呼び出される。
		/// </summary>
		/// <param name="initData">初期化用のデータ</param>
		public void OnInitializeScene(SceneInitData initData)
		{
			// nullチェック
			if (initData == null)
			{
				Debug.LogErrorFormat(this, "初期化用データがありません。[{0}]", name);
				return;
			}

			// 型変換して確認
			T converted = initData as T;
			if (converted == null)
			{
				Debug.LogErrorFormat(this, "初期化用データの型が画面インスタンスと一致しません。[{0}, {1}]", name, typeof(T).Name);
				return;
			}

			// 初期化をかける
			OnInitializeScene(initData as T);
		}

		/// <summary>
		/// 画面を初期化する際に呼び出される。
		/// </summary>
		/// <param name="initData">初期化用のデータ</param>
		protected abstract void OnInitializeScene(T initData);

	}

}
