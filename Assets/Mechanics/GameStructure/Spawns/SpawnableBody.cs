using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnableBody : MonoBehaviour
{
    public AsyncOperationHandle<GameObject> prefabHandle;
    private void Start()
    {
        if (TryGetComponent<SimpleHealth>(out SimpleHealth h))
        {
            h.onDeathEvent.AddListener(Destroy);
        }
    }
    public void Destroy()
    {
        GameObjectSpawner.Despawn(this);
    }
}