using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using System.Threading;

namespace Pspkurara.Sceneries.Transitions
{

	/// <summary>
	/// 遷移アニメーションを担う。
	/// <see cref="PlayableDirector"/>を使ったバージョン。
	/// </summary>
	public sealed class PlayableSceneTransition : SceneTransitionBase
	{

		[SerializeField]
		private PlayableDirector m_EnterTimeline = null;

		[SerializeField]
		private PlayableDirector m_ExitTimeline = null;

		private bool m_IsPlaying = false;

		private CancellationTokenSource cancelToken = null;

		/// <summary>
		/// 再生中かを返す。
		/// </summary>
		public override bool isPlaying { get { return m_IsPlaying; } }

		protected override void Awake()
		{
			if (m_EnterTimeline) m_EnterTimeline.enabled = false;
			if (m_ExitTimeline) m_ExitTimeline.enabled = false;
		}

		/// <summary>
		/// 遷移アニメーション即時適応 (通常再生)
		/// </summary>
		public override void ImmediateApplyInvationAnimation()
		{
			Cancel();
			if (m_EnterTimeline == null) return;
			m_EnterTimeline.enabled = true;
			m_EnterTimeline.time = m_EnterTimeline.duration;
			m_EnterTimeline.Evaluate();
			m_EnterTimeline.enabled = false;
			m_IsPlaying = false;
		}

		/// <summary>
		/// 遷移アニメーション即時適応 (逆再生)
		/// </summary>
		public override void ImmediateApplyDepartureAnimation()
		{
			Cancel();
			if (m_ExitTimeline == null) return;
			m_ExitTimeline.enabled = true;
			m_ExitTimeline.time = m_ExitTimeline.duration;
			m_ExitTimeline.Evaluate();
			m_ExitTimeline.enabled = false;
			m_IsPlaying = false;
		}

		/// <summary>
		/// 遷移アニメーション再生開始 (通常再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		public override async UniTask PlayInvationAnimationAndWaitComplete()
		{
			Cancel();
			if (m_EnterTimeline == null) return;

			cancelToken = new CancellationTokenSource();
			m_IsPlaying = true;
			m_EnterTimeline.enabled = true;
			m_EnterTimeline.time = 0;
			m_EnterTimeline.Play();
			await UniTask.WaitWhile(()=> m_EnterTimeline != null && m_EnterTimeline.time < m_EnterTimeline.duration, cancellationToken: cancelToken.Token);
			m_EnterTimeline.enabled = false;
			m_IsPlaying = false;
			CancelDispose();
		}

		/// <summary>
		/// 遷移アニメーション再生開始 (逆再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		public override async UniTask PlayDepartureAnimationAndWaitComplete()
		{
			Cancel();
			if (m_ExitTimeline == null) return;

			cancelToken = new CancellationTokenSource();
			m_IsPlaying = true;
			m_ExitTimeline.enabled = true;
			m_ExitTimeline.time = 0;
			m_ExitTimeline.Play();
			await UniTask.WaitWhile(() => m_ExitTimeline != null && m_ExitTimeline.time < m_ExitTimeline.duration, cancellationToken: cancelToken.Token);
			m_ExitTimeline.enabled = false;
			m_IsPlaying = false;
			cancelToken?.Dispose();
			cancelToken = null;
			CancelDispose();
		}

		private void Cancel()
		{
			cancelToken?.Cancel();
			CancelDispose();
		}

		private void CancelDispose()
		{
			cancelToken?.Dispose();
			cancelToken = null;
		}

		protected override void OnDestroy()
		{
			Cancel();
		}

		#if UNITY_EDITOR

		protected override void OnValidate()
		{
			// 必要な初期設定を行わせる
			if (m_EnterTimeline)
			{
				m_EnterTimeline.playOnAwake = false;
				m_EnterTimeline.extrapolationMode = DirectorWrapMode.Hold;
			}
			if (m_ExitTimeline)
			{
				m_ExitTimeline.playOnAwake = false;
				m_ExitTimeline.extrapolationMode = DirectorWrapMode.Hold;
			}
		}

		#endif


	}

}
