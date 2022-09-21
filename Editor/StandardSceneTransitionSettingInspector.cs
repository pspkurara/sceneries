using UnityEngine;
using UnityEditor;

namespace Pspkurara.Sceneries.Editor
{

	/// <summary>
	/// <see cref="StandardSceneTransitionSetting"/>のインスペクター表示。
	/// </summary>
	[CustomEditor(typeof(StandardSceneTransitionSetting),true)]
	internal sealed class StandardSceneTransitionSettingInspector : UnityEditor.Editor
	{

		private const string Enabled = "m_Enabled";
		private const string Delay = "m_Delay";
		private const string Duration = "m_Duration";
		private const string Animation = "m_Animation";
		private const string FromValue = "m_FromValue";
		private const string ToValue = "m_ToValue";

		private GUIContent m_EnabledContent;
		private GUIContent m_DelayContent;
		private GUIContent m_DurationContent;
		private GUIContent m_AnimationContent;
		private GUIContent m_FromValueContent;
		private GUIContent m_ToValueContent;

		private GUIContent m_AlphaContent;
		private GUIContent m_PositionContent;
		private GUIContent m_RotationContent;
		private GUIContent m_ScaleContent;
		private GUIContent m_XContent;
		private GUIContent m_YContent;
		private GUIContent m_ZContent;

		private void OnEnable()
		{
			m_EnabledContent = new GUIContent("Enabled");
			m_DelayContent = new GUIContent("Delay");
			m_DurationContent = new GUIContent("Duration");
			m_AnimationContent = new GUIContent("Animation (0 to 1)");
			m_FromValueContent = new GUIContent("From Value");
			m_ToValueContent = new GUIContent("To Value");
			m_AlphaContent = new GUIContent("Alpha");
			m_PositionContent = new GUIContent("Position");
			m_RotationContent = new GUIContent("Rotation");
			m_ScaleContent = new GUIContent("Scale");
			m_XContent = new GUIContent("X");
			m_YContent = new GUIContent("Y");
			m_ZContent = new GUIContent("Z");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			DrawFloatElement(serializedObject.FindProperty("m_Alpha"), m_AlphaContent, 0, 1, true);
			DrawVectorElement(serializedObject.FindProperty("m_Position"), m_PositionContent, float.MinValue, float.MaxValue);
			DrawVectorElement(serializedObject.FindProperty("m_Rotation"), m_RotationContent, -360*5, 360*5);
			DrawVectorElement(serializedObject.FindProperty("m_Scale"), m_ScaleContent, 0, 1, true);
			serializedObject.ApplyModifiedProperties();
		}


		/// <summary>
		/// <see cref="StandardSceneTransitionSetting.TransitionElement">のインスペクター描画。
		/// </summary>
		/// <param name="property">プロパティ</param>
		/// <param name="label">ラベル</param>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <param name="isSlider">スライダー表示にするか</param>
		private void DrawFloatElement(SerializedProperty property, GUIContent label, float min, float max, bool isSlider = false)
		{
			bool expand = EditorGUILayout.Foldout(property.isExpanded, label, true);
			property.isExpanded = expand;
			if (expand)
			{
				EditorGUI.indentLevel++;
				var ep = property.FindPropertyRelative(Enabled);
				EditorGUILayout.PropertyField(ep, m_EnabledContent);
				bool enabled = GUI.enabled;
				if (enabled) GUI.enabled = ep.boolValue;
				EditorGUILayout.PropertyField(property.FindPropertyRelative(Delay), m_DelayContent);
				EditorGUILayout.PropertyField(property.FindPropertyRelative(Duration), m_DurationContent);
				EditorGUILayout.PropertyField(property.FindPropertyRelative(Animation), m_AnimationContent);
				DrawSliderOrFloatField(property.FindPropertyRelative(FromValue), m_FromValueContent, min, max, isSlider);
				DrawSliderOrFloatField(property.FindPropertyRelative(ToValue), m_ToValueContent, min, max, isSlider);
				GUI.enabled = enabled;
				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// <see cref="StandardSceneTransitionSetting.TransitionElementVector">のインスペクター描画。
		/// </summary>
		/// <param name="property">プロパティ</param>
		/// <param name="label">ラベル</param>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <param name="isSlider">スライダー表示にするか</param>
		private void DrawVectorElement(SerializedProperty property, GUIContent label, float min, float max, bool isSlider = false)
		{
			bool expand = EditorGUILayout.Foldout(property.isExpanded, label, true);
			property.isExpanded = expand;
			if (expand)
			{
				EditorGUI.indentLevel++;
				var ep = property.FindPropertyRelative(Enabled);
				EditorGUILayout.PropertyField(ep, m_EnabledContent);
				bool enabled = GUI.enabled;
				if (enabled) GUI.enabled = ep.boolValue;
				DrawFloatElement(property.FindPropertyRelative("m_X"), m_XContent, min, max, isSlider);
				DrawFloatElement(property.FindPropertyRelative("m_Y"), m_YContent, min, max, isSlider);
				DrawFloatElement(property.FindPropertyRelative("m_Z"), m_ZContent, min, max, isSlider);
				GUI.enabled = enabled;
				EditorGUI.indentLevel--;
			}
		}

		/// <summary>
		/// スライダーかIntFieldで描画する。
		/// </summary>
		/// <param name="property">プロパティ</param>
		/// <param name="label">ラベル</param>
		/// <param name="min">最小値</param>
		/// <param name="max">最大値</param>
		/// <param name="isSlider">スライダー表示にするか</param>
		private void DrawSliderOrFloatField(SerializedProperty property, GUIContent label, float min, float max, bool isSlider)
		{
			if (isSlider)
			{
				property.floatValue = EditorGUILayout.Slider(label, property.floatValue, min, max);
			}
			else
			{
				property.floatValue = EditorGUILayout.FloatField(label, property.floatValue);
				property.floatValue = Mathf.Clamp(property.floatValue, min, max);
			}
		}

	}

}
