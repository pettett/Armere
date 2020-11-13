using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Animal : MonoBehaviour
{
    enum State : byte
    {
        Idle,
        Scared
    }
    [SerializeField] State state;
    public ItemName drop;
    public int count;
    public float scareDistance = 1f;
    public float scareTime = 2f;
    public float speed = 1f;
    public float scareSpeed = 3f;
    public ItemDatabase db;
    public void Attack(float damage)
    {
        //Die
        print("Hit Animal");
        for (int i = 0; i < count; i++)
            ItemSpawner.SpawnItemAsync(drop, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private void Update()
    {
        Vector3 dir = default;
        if (state == State.Idle)
        {
            float n = Mathf.PerlinNoise(transform.position.x, transform.position.z) * 2 * Mathf.PI;
            dir = new Vector3(Mathf.Sin(n), 0, Mathf.Cos(n)) * speed;
        }
        else if (state == State.Scared)
        {
            dir = (transform.position - LevelInfo.currentLevelInfo.playerTransform.position).normalized * scareSpeed;
        }

        if (NavMesh.SamplePosition(transform.position + dir * Time.deltaTime, out NavMeshHit hit, 0.1f, -1))
        {
            transform.position = hit.position;
        }

        //If the animal is too close to the player, start running away
        if (state != State.Scared && (transform.position - LevelInfo.currentLevelInfo.playerTransform.position).sqrMagnitude < scareDistance * scareDistance)
        {
            //run away
            StartCoroutine(Scare());
        }


    }
    IEnumerator Scare()
    {
        state = State.Scared;
        yield return new WaitForSeconds(scareTime);
        state = State.Idle;
    }
}
