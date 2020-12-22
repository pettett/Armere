using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Armere.Inventory;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Collider)),
RequireComponent(typeof(Rigidbody))]
public class Arrow : SpawnableBody
{
    bool initialized = false;
    ItemName ammoName;
    Rigidbody rb;
    Collider col;
    bool hit = false;
    public void Initialize(ItemName ammoName, Vector3 velocity, ItemDatabase db)
    {
        enabled = true;
        transform.localScale = Vector3.one;
        //Calibrate components
        this.ammoName = ammoName;
        if (db[this.ammoName].type != ItemType.Ammo)
        {
            //Make sure the item fired is actually ammo
            Debug.LogError("Non - Ammo type fired as arrow");
            return;
        }


        //transform.position = position;

        // GetComponent<MeshFilter>().sharedMesh = db[ammoName].mesh;
        // GetComponent<MeshRenderer>().materials = db[ammoName].materials;

        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        transform.forward = velocity;
        rb.isKinematic = false;

        col = GetComponent<Collider>();
        col.enabled = true;

        //Set initialzed
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            Debug.LogError("Arrow not initialized by update", gameObject);
            return;
        }
        transform.forward = rb.velocity;
    }
    private async void OnCollisionEnter(Collision other)
    {
        if (!other.collider.isTrigger && !hit)
        {

            print($"Arrow hit {other.collider.gameObject.name}");
            hit = true;
            if (other.gameObject.TryGetComponent<Health>(out var h))
            {
                h.Damage(10, gameObject);
            }

            Destroy();
            //Turn arrow into an item if it is permitted
            await ItemSpawner.SpawnItemAsync(ammoName, transform.position, transform.rotation);
        }
    }
}