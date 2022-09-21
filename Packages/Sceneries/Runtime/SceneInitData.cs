using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 画面を初期化する際に設定するデータ。
	/// 継承して、画面ごとに用意する。
	/// </summary>
	public abstract class SceneInitData
	{

		/// <summary>
		/// 履歴に格納する初期化データを返す。
		/// </summary>
		/// <returns>格納するデータ</returns>
		/// <remarks>
		/// 初期化用データのインスタンスを使いまわす必要がない場合はオーバーライド不要。
		/// </remarks>
		protected internal virtual SceneInitData GetInitDataForHistory()
		{
			return this;
		}

	}

}
