using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Type = System.Type;

namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 画面操作の拡張メソッド郡。
	/// </summary>
	internal static class SceneriesExtensions
	{

		/// <summary>
		/// 画面の個別初期化を行う。
		/// </summary>
		/// <param name="sceneries">対象の画面インスタンス</param>
		/// <param name="initData">初期化用データ</param>
		public static void SetInitData(this Scene sceneries, SceneInitData initData)
		{
			if (sceneries == null) return;
			var sceneInit = sceneries as ISceneInitialize;
			if (sceneInit != null)
			{
				sceneInit.OnInitializeScene(initData);
			}
			else if(initData != null)
			{
				// 画面の初期化に非対応の場合は警告を出す。
				Debug.LogWarningFormat(sceneries, "画面の初期化に対応していません。( {0} )", sceneries.GetType());
			}
		}

		/// <summary>
		/// 画面インスタンスの表示 / 非表示を切り替える。
		/// </summary>
		/// <param name="sceneries">対象の画面インスタンス</param>
		/// <param name="currentActiveSceneries">現在アクティブな画面インスタンス</param>
		/// <param name="handler">マネージャーから渡されるデータハンドラー</param>
		public static async UniTask SetActive(this Scene sceneries, Scene currentActiveSceneries, ManagerToSceneHandler handler)
		{
			if (sceneries == null) return;
			await sceneries.SetSceneActive(sceneries == currentActiveSceneries ? null : currentActiveSceneries, handler);
		}

		/// <summary>
		/// 画面インスタンスを削除する
		/// </summary>
		/// <param name="sceneries">対象の画面インスタンス</param>
		public static void Destroy(this Scene sceneries)
		{
			if (sceneries == null) return;
			Object.Destroy(sceneries.gameObject);
		}

		/// <summary>
		/// 各値がNaNかどうか取得する
		/// </summary>
		public static bool IsNaN(this Vector2 vector2)
		{
			return float.IsNaN(vector2.x) || float.IsNaN(vector2.y);
		}

		/// <summary>
		/// 各値がNaNかどうか取得する
		/// </summary>
		public static bool IsNaN(this Vector3 vector2)
		{
			return float.IsNaN(vector2.x) || float.IsNaN(vector2.y) || float.IsNaN(vector2.z);
		}

	}

}
