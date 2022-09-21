using UnityEngine;
using UnityEditor;

namespace Pspkurara.Sceneries.Editor
{

	/// <summary>
	/// <see cref="SceneriesManager"/>のインスペクター表示。
	/// </summary>
	[CustomEditor(typeof(SceneriesManager), true)]
	internal sealed class SceneriesManagerInspector : UnityEditor.Editor
	{

		/// <summary>
		/// プレビューウィンドウのラベルの<see cref="GUIStyle"/>。
		/// </summary>
		private GUIStyle m_PreviewLabelStyle = null;

		/// <summary>
		/// プレビューウィンドウのラベルの<see cref="GUIStyle"/>。
		/// </summary>
		private GUIStyle previewLabelStyle {
			get {
				if (m_PreviewLabelStyle == null)
					m_PreviewLabelStyle = new GUIStyle("PreOverlayLabel")
					{
						richText = true,
						alignment = TextAnchor.UpperLeft,
						fontStyle = FontStyle.Normal
					};
				return m_PreviewLabelStyle;
			}
		}

		/// <summary>
		/// プレビューGUIを表示するか。
		/// </summary>
		/// <returns>有効</returns>
		public override bool HasPreviewGUI()
		{
			// 再生中のみ表示
			return Application.isPlaying;
		}

		/// <summary>
		/// リアルタイム再描画を有効にするか。
		/// </summary>
		/// <returns>有効</returns>
		public override bool RequiresConstantRepaint()
		{
			// 再生中のみリアルタイム再描画
			return Application.isPlaying;
		}

		/// <summary>
		/// プレビューGUIの描画。
		/// </summary>
		/// <param name="rect">描画範囲</param>
		/// <param name="background">背景の<see cref="GUIStyle"/></param>
		public override void OnPreviewGUI(Rect rect, GUIStyle background)
		{
			var sceneries = target as SceneriesManager;
			if (!sceneries) return;
			GUI.Label(rect, sceneries.ToString(), previewLabelStyle);
		}

	}

}
