using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Pspkurara.Sceneries.Transitions
{

	[RequireComponent(typeof(CanvasGroup))]
	[RequireComponent(typeof(RectTransform))]
	public sealed class StandardSceneTransition : SceneTransitionBase
	{

		[SerializeField] private StandardSceneTransitionSetting m_Setting = null;

		private CanvasGroup m_CanvasGroup = null;

		public CanvasGroup canvasGroup { get { if (!m_CanvasGroup) m_CanvasGroup = GetComponent<CanvasGroup>(); return m_CanvasGroup; } }

		private RectTransform m_RectTransform = null;
		public RectTransform rectTransform { get { if (!m_RectTransform) m_RectTransform = transform as RectTransform; return m_RectTransform; } }

		private bool m_IsPlaying = false;

		private CancellationTokenSource cancelToken = null;

		private float time;

		private Vector3? m_InitialPosition;

		/// <summary>
		/// 再生中かを返す。
		/// </summary>
		public override bool isPlaying { get { return m_IsPlaying; } }

		private void SetDirty()
		{
			canvasGroup.alpha = m_Setting.GetAlphaEvaluate(time, canvasGroup.alpha);

			if (!m_InitialPosition.HasValue) m_InitialPosition = rectTransform.anchoredPosition3D;
			rectTransform.anchoredPosition3D = m_Setting.GetPositionEvaluate(time, m_InitialPosition.Value);

			rectTransform.localRotation = m_Setting.GetRotationEvaluate(time, rectTransform.localRotation);
			rectTransform.localScale = m_Setting.GetScaleEvaluate(time, rectTransform.localScale);
		}

		/// <summary>
		/// 遷移アニメーション即時適応 (通常再生)
		/// </summary>
		public override void ImmediateApplyInvationAnimation()
		{
			Cancel();
			if (m_Setting == null) return;
			time = m_Setting.maxDuration;
			SetDirty();
			m_IsPlaying = false;
		}

		/// <summary>
		/// 遷移アニメーション即時適応 (逆再生)
		/// </summary>
		public override void ImmediateApplyDepartureAnimation()
		{
			Cancel();
			if (m_Setting == null) return;
			time = 0;
			SetDirty();
			m_IsPlaying = false;
		}

		/// <summary>
		/// 遷移アニメーション再生開始 (通常再生)
		/// </summary>
		/// <returns>非同期待機処理</returns>
		public override async UniTask PlayInvationAnimationAndWaitComplete()
		{
			Cancel();
			if (m_Setting == null) return;

			cancelToken = new CancellationTokenSource();
			m_IsPlaying = true;

			float max = m_Setting.maxDuration;
			while (time < max)
			{
				SetDirty();
				await UniTask.Yield();
				time += Time.unscaledDeltaTime;
				if (cancelToken == null || cancelToken.IsCancellationRequested) break;
			}
			time = Mathf.Min(max, time);
			SetDirty();

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
			if (m_Setting == null) return;

			cancelToken = new CancellationTokenSource();
			m_IsPlaying = true;

			while (time >= 0)
			{
				SetDirty();
				await UniTask.Yield();
				time -= Time.unscaledDeltaTime;
				if (cancelToken == null || cancelToken.IsCancellationRequested) break;
			}
			time = Mathf.Max(0, time);
			SetDirty();

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

	}

}
