using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Pspkurara.Sceneries
{

	[CreateAssetMenu(fileName ="NewSceneTransitionSetting", menuName ="Pspkurara/Sceneries/Standard Scene Transition Setting", order =10)]
	public sealed class StandardSceneTransitionSetting : ScriptableObject
	{

		[System.Serializable]
		public sealed class TransitionElement
		{

			[SerializeField] private bool m_Enabled = true;
			[SerializeField] private float m_Delay = 0f;
			[SerializeField] private float m_Duration = 1f;
			[SerializeField] private AnimationCurve m_Animation = AnimationCurve.Linear(0, 0, 1, 1);
			[SerializeField] private float m_FromValue;
			[SerializeField] private float m_ToValue;

			private float? minAnimationTime = null;
			private float? maxAnimationTime = null;

			public float delay { get { return m_Enabled ? m_Delay : 0; } }

			public float duration { get { return m_Enabled ? m_Duration + m_Delay : 0; } }

			public float GetEvaluate(float time, float based)
			{
				if (!m_Enabled) return based;
				time -= m_Delay;

				if (!minAnimationTime.HasValue)
				{
					minAnimationTime = m_Animation.keys.FirstOrDefault().time;
				}
				if (!maxAnimationTime.HasValue)
				{
					maxAnimationTime = m_Animation.keys.LastOrDefault().time;
				}
				return Mathf.Lerp(m_FromValue, m_ToValue, m_Animation.Evaluate(Mathf.Clamp(time, minAnimationTime.Value, maxAnimationTime.Value) / m_Duration));
			}

		}

		[System.Serializable]
		public sealed class TransitionElementVector
		{

			[SerializeField] private bool m_Enabled = true;
			[SerializeField] private TransitionElement m_X = new TransitionElement();
			[SerializeField] private TransitionElement m_Y = new TransitionElement();
			[SerializeField] private TransitionElement m_Z = new TransitionElement();

			public float delay { get { return m_Enabled ? Mathf.Max(m_X.delay, m_Y.delay, m_Z.delay) : 0; } }

			public float duration { get { return m_Enabled ? Mathf.Max(m_X.duration, m_Y.duration, m_Z.duration) : 0; } }

			public Vector3 GetEvaluate(float time, Vector3 based)
			{
				if (!m_Enabled) return based;
				return new Vector3(m_X.GetEvaluate(time, based.x), m_Y.GetEvaluate(time, based.y), m_Z.GetEvaluate(time, based.z));
			}

		}

		[SerializeField] private TransitionElement m_Alpha = new TransitionElement();

		[SerializeField] private TransitionElementVector m_Position = new TransitionElementVector();

		[SerializeField] private TransitionElementVector m_Rotation = new TransitionElementVector();

		[SerializeField] private TransitionElementVector m_Scale = new TransitionElementVector();

		public float maxDuration { get { return Mathf.Max(m_Alpha.duration, m_Position.duration, m_Rotation.duration, m_Scale.duration); } }

		public float GetAlphaEvaluate(float time, float based)
		{
			return m_Alpha.GetEvaluate(time, based);
		}

		public Vector3 GetPositionEvaluate(float time, Vector3 based)
		{
			return based + m_Position.GetEvaluate(time, Vector3.zero);
		}

		public Quaternion GetRotationEvaluate(float time, Quaternion based)
		{
			var euler = based.eulerAngles;
			var result = m_Rotation.GetEvaluate(time, euler);
			return Quaternion.Euler(result);
		}

		public Vector3 GetScaleEvaluate(float time, Vector3 based)
		{
			return m_Scale.GetEvaluate(time, based);
		}

	}

}
