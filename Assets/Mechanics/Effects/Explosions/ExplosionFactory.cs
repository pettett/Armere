using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.VFX;
public class ExplosionFactory : MonoBehaviour
{
	public float createdExplosionRadius = 10;
	public float createdExplosionForce = 500;
	public float createdExplosionUpwardsModifier;
	public float explosionTime = 2f;

	public Vector3EventChannelSO createExplosionEventChannel;
	public Vector3FloatEventChannelSO onExplosionEventChannel;

	public AssetReferenceGameObject explosionPrefab;

	AsyncOperationHandle<GameObject> loadedPrefab;

	public static readonly int explosionSizeRange = Shader.PropertyToID("Explosion Size Range");

	// Start is called before the first frame update
	void Start()
	{
		createExplosionEventChannel.OnEventRaised += CreateExplosion;
	}
	private void OnDestroy()
	{
		createExplosionEventChannel.OnEventRaised -= CreateExplosion;

		if (loadedPrefab.IsValid())
			Addressables.Release(loadedPrefab);
	}

	// Update is called once per frame
	void CreateExplosion(Vector3 position)
	{
		Debug.Log("Creating explosion");
		var effected = Physics.OverlapSphere(position, createdExplosionRadius, -1, QueryTriggerInteraction.Ignore);
		foreach (var rb in effected)
		{
			rb.attachedRigidbody?.AddExplosionForce(createdExplosionForce, position, createdExplosionRadius, createdExplosionUpwardsModifier);
			rb.GetComponent<IExplosionEffector>()?.OnExplosion(position, createdExplosionRadius, createdExplosionForce);
		}

		onExplosionEventChannel?.RaiseEvent(position, createdExplosionRadius);

		StartCoroutine(Explode(position));
	}
	IEnumerator Explode(Vector3 position)
	{
		//Have a single addressable prefab loaded for explosions
		//Only load it on the first explosion
		//TODO: Make it unloaded after certain time w/ no explosions
		if (!loadedPrefab.IsValid())
		{
			loadedPrefab = Addressables.LoadAssetAsync<GameObject>(explosionPrefab);
		}

		if (!loadedPrefab.IsDone)
		{
			yield return loadedPrefab;
		}


		var x = Instantiate(loadedPrefab.Result, position, Quaternion.identity);
		//Takes in diameter... i think?
		x.gameObject.GetComponent<VisualEffect>().SetVector2(explosionSizeRange, new Vector2(createdExplosionRadius * 2, createdExplosionRadius * 4));
		Destroy(x, explosionTime);

	}
}
