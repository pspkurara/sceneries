using Cysharp.Threading.Tasks;

namespace Pspkurara.Sceneries.Transitions
{

	/// <summary>
	/// 遷移アニメーション周りの関数郡。
	/// </summary>
	public static class SceneTransitionUtility
	{

		/// <summary>
		/// 画面遷移アニメーションを再生する。
		/// </summary>
		/// <param name="from">切り替わる前の画面</param>
		/// <param name="to">切り替わった後の画面</param>
		public static async UniTask Transition(ISceneTransition from, ISceneTransition to)
		{
			// 前の画面はない
			if (from == null)
			{
				await to.PlayInvationAnimationAndWaitComplete();
			}
			// 次の画面はない
			else if (to == null)
			{
				await from.PlayDepartureAnimationAndWaitComplete();
			}
			// 両方の画面があり、かつ両方同時に再生して良い
			else if (from.allowDuplicatePlayback && to.allowDuplicatePlayback)
			{
				// 同時に再生
				await UniTask.WhenAll(
					from.PlayDepartureAnimationAndWaitComplete(),
					to.PlayInvationAnimationAndWaitComplete()
					);
			}
			// 両方の画面がある
			else
			{
				// 次の画面が既に見えている可能性があるのですぐに初期化
				to.ImmediateApplyInvationAnimation();
				// 順に再生
				await from.PlayDepartureAnimationAndWaitComplete();
				await to.PlayInvationAnimationAndWaitComplete();
			}
		}

	}

}
