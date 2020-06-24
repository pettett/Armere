using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : AIBase
{
    public enum EnemyBehaviour
    {
        Guard,
        Patrol,
    }
    public EnemyBehaviour enemyBehaviour;
    [System.Serializable]
    public class PatrolData
    {
        public float waitTime = 2;
        public Vector3[] path = new Vector3[0];
    }
    [MyBox.ConditionalField("enemyBehaviour", false, EnemyBehaviour.Patrol)] public PatrolData patrolData;

    public Vector2 clippingPlanes = new Vector2(0.1f, 10f);
    [Range(1, 90)] public float fov = 45;
    public Transform eye;
    public Collider playerCollider;


    [Header("UI")]

    public Image alertImage;
    public Image investigateImage;
    public Image investigateProgressImage;

    private void OnValidate()
    {
        if (clippingPlanes.x > clippingPlanes.y)
        {
            //If the lower value is bigger, make the upper value equal
            clippingPlanes.y = clippingPlanes.x;
        }
        else if (clippingPlanes.y < clippingPlanes.x)
        {
            //if the upper value is smaller, make the lower value equal
            clippingPlanes.x = clippingPlanes.y;
        }
    }

    protected override void Start()
    {
        base.Start();
        if (enemyBehaviour == EnemyBehaviour.Patrol)
        {
            StartCoroutine(PatrolRoutine());
        }
    }

    IEnumerator PatrolRoutine()
    {
        int i = 0;
        while (true)
        {
            yield return GoToPosition(patrolData.path[i]);
            yield return new WaitForSeconds(patrolData.waitTime);
            i++;
            if (i == patrolData.path.Length) i = 0;
        }
    }

    IEnumerator Investigate()
    {
        float investProgress;
    }
    IEnumerator Alert()
    {

    }



    Matrix4x4 viewMatrix;
    Plane[] viewPlanes = new Plane[6];
    public bool CanSeeBounds(Bounds b)
    {
        viewMatrix = Matrix4x4.Perspective(fov, 1, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1));
        GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, viewPlanes);
        return GeometryUtility.TestPlanesAABB(viewPlanes, b);
    }

    private void Update()
    {
        //Test if the enemy can see the player at this point
        LookAtPlayer(playerCollider.transform.position);
    }

    private void OnDrawGizmos()
    {
        if (patrolData.path.Length >= 2)
        {
            for (int i = 0; i < patrolData.path.Length - 1; i++)
            {
                Gizmos.DrawLine(patrolData.path[i], patrolData.path[i + 1]);
            }
            Gizmos.DrawLine(patrolData.path[0], patrolData.path[patrolData.path.Length - 1]);
        }
        var b = playerCollider.bounds;
        if (CanSeeBounds(b))
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireCube(b.center, b.size);

        Gizmos.color = Color.white;
        Gizmos.matrix = eye.localToWorldMatrix;
        Gizmos.DrawFrustum(Vector3.zero, fov, clippingPlanes.y, clippingPlanes.x, 1f);

    }
}
