using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public abstract class AIBase : SpawnableBody
{
    protected NavMeshAgent agent;
    protected Animator anim;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    protected IEnumerator GoToPosition(Vector3 position)
    {
        agent.SetDestination(position);
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < agent.stoppingDistance * 2 + 0.01f);
    }
    protected void GoToPosition(Vector3 position, System.Action onComplete)
    {
        agent.SetDestination(position);
        StartCoroutine(WaitForAgent(onComplete));
    }

    protected IEnumerator RotateTo(Quaternion rotation, float time)
    {
        float t = 0;
        Quaternion start = transform.rotation;
        while (t < 1)
        {
            yield return new WaitForEndOfFrame();
            transform.rotation = Quaternion.Slerp(start, rotation, t);
            t += Time.deltaTime / time;
        }
        transform.rotation = rotation;
    }

    public static IEnumerator WaitForAgent(NavMeshAgent agent, System.Action onComplete)
    {
        yield return new WaitUntil(() => !agent.pathPending && agent.remainingDistance < agent.stoppingDistance * 2 + 0.01f);
        onComplete?.Invoke();
    }

    IEnumerator WaitForAgent(System.Action onComplete)
    {
        yield return WaitForAgent(agent, onComplete);
    }




    public void LookAtPlayer(Vector3 playerPos)
    {
        anim.SetLookAtPosition(playerPos);
        anim.SetLookAtWeight(1, 0, 1, 1, 0.2f);


        Vector3 flatDir = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - Vector3.Scale(playerPos, new Vector3(1, 0, 1));
        float angle = Vector3.Angle(transform.forward, flatDir);
        //If above angle threshold
        if (angle > 20)
        {

        }

    }
    public void LookAway()
    {
        anim.SetLookAtWeight(0);
    }

}
