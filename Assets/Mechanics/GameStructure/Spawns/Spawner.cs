using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
public abstract class Spawner : MonoBehaviour
{
	public abstract Task<SpawnableBody> Spawn();


#if UNITY_EDITOR
	protected void DrawSpawnedItem(AssetReferenceGameObject go)
	{

		if (go.editorAsset.TryGetComponent<MeshFilter>(out MeshFilter mf))
		{
			Matrix4x4 trans = Matrix4x4.TRS(transform.position, transform.rotation, go.editorAsset.transform.lossyScale);
			Mesh m = mf.sharedMesh;
			var mats = go.editorAsset.GetComponent<MeshRenderer>().sharedMaterials;

			for (int i = 0; i < m.subMeshCount; i++)
			{
				//Graphics.DrawMesh(m, transform.position, transform.rotation, mats[i], 0);
				Gizmos.color = mats[i].color;
				//Gizmos.DrawMesh(m, i, transform.position, transform.rotation, item.gameObject.editorAsset.transform.lossyScale);



				Graphics.DrawMesh(m, trans, mats[i], 0, null, i, null, true, true, true);
			}
		}
	}
#endif
}