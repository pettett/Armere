using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnableBody : MonoBehaviour
{
	public AsyncOperationHandle<GameObject> prefabHandle;
	protected bool inited = false;
	private void Start()
	{
		if (TryGetComponent<SimpleHealth>(out SimpleHealth h))
		{
			h.onDeathEvent.AddListener(Destroy);

		}
	}

	public virtual void Init()
	{
		inited = true;
	}


	public void Destroy()
	{
		GameObjectSpawner.Despawn(this);
	}
}