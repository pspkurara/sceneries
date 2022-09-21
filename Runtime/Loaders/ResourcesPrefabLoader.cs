using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Pspkurara.Sceneries.Loaders
{

	/// <summary>
	/// <see cref="Resources"/>からプレハブをロードする。
	/// </summary>
	[CreateAssetMenu(fileName ="NewResourcesPrefabLoader", menuName = "Pspkurara/Sceneries/Prefab Loader (Resouces)")]
	public sealed class ResourcesPrefabLoader : SceneriesPrefabLoaderBase
	{

		[SerializeField] private string m_PrefabPath = "Sceneries/{0}";

		/// <summary>
		/// プレハブをロードする。
		/// </summary>
		/// <param name="name">プレハブ名</param>
		/// <param name="cancelToken">キャンセルトークン</param>
		/// <returns>プレハブ</returns>
		public override async UniTask<GameObject> LoadPrefab(string name, CancellationToken cancelToken)
		{
			return await Resources.LoadAsync<GameObject>(string.Format(m_PrefabPath, name)).WithCancellation(cancelToken) as GameObject;
		}

	}

}
