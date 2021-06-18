using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class Spawner : MonoBehaviour
{
	static uint handles = 0;

	public static void LoadAsset<T>(AssetReferenceT<T> reference, System.Action<AsyncOperationHandle<T>> onDone) where T : UnityEngine.Object
	{
		if (reference.RuntimeKeyIsValid())
		{
			var handle = Addressables.LoadAssetAsync<T>(reference);

			handles++;
			//Debug.Log($"Loading, currentley: {handles}");
			OnDone(handle, onDone);
		}
	}
	public static void ReleaseAsset<T>(AsyncOperationHandle<T> reference) where T : UnityEngine.Object
	{
		if (reference.IsValid())
		{
			handles--;
			Addressables.Release(reference);

			//Debug.Log($"Releasing, currentley: {handles}");
		}
	}

	public static void OnDone<T>(AsyncOperationHandle<T> handle, System.Action<AsyncOperationHandle<T>> onDone)
	{
		if (!handle.IsDone)
			handle.Completed += onDone;
		else if (handle.Status == AsyncOperationStatus.Succeeded)
		{
			//Debug.Log($"Immediately done {handle.Result.name}");
			onDone(handle);
		}
	}


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