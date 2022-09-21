namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 同じ画面IDの画面をパラメーターによって個別に初期化する際に使用する。
	/// </summary>
	public interface ISceneInitialize
	{

		/// <summary>
		/// 画面を初期化する際に呼び出される。
		/// </summary>
		/// <param name="initData">初期化用のデータ</param>
		void OnInitializeScene(SceneInitData initData);

	}

}
