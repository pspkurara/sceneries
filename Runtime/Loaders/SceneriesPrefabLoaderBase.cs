using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Pspkurara.Sceneries.Loaders
{

	/// <summary>
	/// プレハブローダー。
	/// 継承して使用する。
	/// </summary>
	public abstract class SceneriesPrefabLoaderBase : ScriptableObject
	{

		/// <summary>
		/// プレハブをロードする。
		/// </summary>
		/// <param name="name">プレハブ名</param>
		/// <param name="cancelToken">キャンセルトークン</param>
		/// <returns>プレハブ</returns>
		public abstract UniTask<GameObject> LoadPrefab(string name, CancellationToken cancelToken);

	}

}
