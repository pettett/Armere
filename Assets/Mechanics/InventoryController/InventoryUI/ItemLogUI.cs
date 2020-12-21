using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Armere.Inventory.UI
{

    public class ItemLogUI : MonoBehaviour
    {
        struct Entry
        {
            public GameObject gameObject;
            public float creationTime;
            public AsyncOperationHandle<GameObject> asyncOperation;

            public Entry(GameObject gameObject, float creationTime, AsyncOperationHandle<GameObject> asyncOperation)
            {
                this.gameObject = gameObject;
                this.creationTime = creationTime;
                this.asyncOperation = asyncOperation;
            }
        }


        public AssetReferenceGameObject entryPrefab;
        public float lastingTime = 1;
        [Range(0, 10)]
        public int maxEntries = 3;
        Queue<Entry> entries;

        private void Start()
        {

            entries = new Queue<Entry>(maxEntries);

            InventoryController.singleton.onItemAdded += OnItemAdded;
        }

        private void OnDestroy()
        {
            InventoryController.singleton.onItemAdded -= OnItemAdded;
        }

        async void OnItemAdded(ItemName item, bool hidden)
        {
            if (!hidden)
            {
                var operation = Addressables.InstantiateAsync(entryPrefab, transform, trackHandle: false);

                GameObject go = await operation.Task;

                go.GetComponent<InventoryItemUI>().SetupItemAsync(InventoryController.singleton.db[item]);

                entries.Enqueue(new Entry(go, Time.time, operation));
            }
        }

        private void Update()
        {
            //Test to remove the top entry
            if (entries.Count > 0 && (entries.Count > maxEntries || Time.time - entries.Peek().creationTime > lastingTime))
            {
                Addressables.ReleaseInstance(entries.Dequeue().asyncOperation);
            }
        }

    }
}