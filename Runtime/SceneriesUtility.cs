using UnityEngine;

namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 画面操作の色々関数。
	/// </summary>
	public static class SceneriesUtility
	{

		/// <summary>
		/// NaNが含まれていたら警告を出す。
		/// </summary>
		/// <param name="rectTransform">調査対象</param>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void WarningIncorrectRectTransform(RectTransform rectTransform)
		{
			if (rectTransform == null) return;
			if (rectTransform.anchoredPosition3D.IsNaN() ||
				rectTransform.sizeDelta.IsNaN() ||
				rectTransform.anchorMin.IsNaN() ||
				rectTransform.anchorMax.IsNaN() ||
				rectTransform.offsetMin.IsNaN() ||
				rectTransform.offsetMax.IsNaN())
			{
				Debug.LogWarningFormat(rectTransform, "指定されたRectTransformの値にNaNが含まれています ( {0} )", rectTransform);
			}
		}

	}

}
