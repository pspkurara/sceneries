using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

namespace Pspkurara.Sceneries.Transitions
{

	/// <summary>
	/// 遷移アニメーションを担う。
	/// 継承して使用する。
	/// </summary>
	public abstract class SceneTransitionBase : UIBehaviour, ISceneTransition
	{

		/// <summary>
		/// 複数画面の遷移アニメーションの同時再生を許可する。
		/// </summary>
		[SerializeField]
		private bool m_AllowDuplicatePlayback = false;

		/// <summary>
		/// 複数画面の遷移アニメーションの同時再生を許可する。
		/// </summary>
		public bool allowDuplicatePlayback { get { return m_AllowDuplicatePlayback; } }

		/// <summary>
		/// 再生中かを返す。
		/// </summary>
		public abstract bool isPlaying { get; }

		/// <summary>
		/// 遷移アニメーション即時適応 (通常再生)
		/// </summary>
		public abstract void ImmediateApplyInvationAnimation();

		/// <summary>
		/// 遷移アニメーション即時適応 (逆再生)
		/// </summary>
		public abstract void ImmediateApplyDepartureAnimation();

		/// <summary>
		/// 遷移アニメーション再生開始 (通常再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		public abstract UniTask PlayInvationAnimationAndWaitComplete();

		/// <summary>
		/// 遷移アニメーション再生開始 (逆再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		public abstract UniTask PlayDepartureAnimationAndWaitComplete();

	}

}
