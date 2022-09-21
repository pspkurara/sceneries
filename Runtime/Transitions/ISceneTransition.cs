using Cysharp.Threading.Tasks;

namespace Pspkurara.Sceneries.Transitions
{

	/// <summary>
	/// 遷移アニメーションを担う。
	/// </summary>
	public interface ISceneTransition
	{

		/// <summary>
		/// 複数画面の遷移アニメーションの同時再生を許可する。
		/// </summary>
		bool allowDuplicatePlayback { get; }

		/// <summary>
		/// 遷移アニメーション即時適応 (通常再生)
		/// </summary>
		void ImmediateApplyInvationAnimation();

		/// <summary>
		/// 遷移アニメーション即時適応 (逆再生)
		/// </summary>
		void ImmediateApplyDepartureAnimation();

		/// <summary>
		/// 遷移アニメーション再生開始 (通常再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		UniTask PlayInvationAnimationAndWaitComplete();

		/// <summary>
		/// 遷移アニメーション再生開始 (逆再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		UniTask PlayDepartureAnimationAndWaitComplete();

	}

}
