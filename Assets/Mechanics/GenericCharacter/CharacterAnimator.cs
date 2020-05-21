using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class CharacterAnimator : MonoBehaviour
{
    public Vector3 leftFootPos;
    public Vector3 leftFootTargetPos;
    Quaternion leftFootRotation;

    public float resetDistance = 0.5f;
    Animator anim;
    Coroutine moveRoutine;
    private void Start()
    {
        leftFootPos = transform.position + transform.right * -0.1f + Vector3.up * 0.1f;
        anim = GetComponent<Animator>();
        leftFootRotation = transform.rotation;
    }
    private void Update()
    {
        leftFootTargetPos = transform.position + transform.right * -0.1f + Vector3.up * 0.1f;

        if (moveRoutine == null && (Vector3.SqrMagnitude(leftFootTargetPos - leftFootPos) > resetDistance * resetDistance || Quaternion.Angle(transform.rotation, leftFootRotation) > 60f))
        {
            moveRoutine = StartCoroutine(MoveFoot(leftFootPos, leftFootRotation));
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(leftFootTargetPos, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(leftFootPos, 0.05f);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRotation);
    }

    IEnumerator MoveFoot(Vector3 startPos, Quaternion startRot)
    {
        Vector3 vel = Vector3.zero;
        float t = 0;
        float v = 0;
        while (t < 0.95f)
        {
            yield return new WaitForEndOfFrame();
            t = Mathf.SmoothDamp(t, 1, ref v, 0.25f);
            print(t);
            leftFootRotation = Quaternion.Slerp(startRot, transform.rotation, t);
            leftFootPos = Vector3.SmoothDamp(leftFootPos, leftFootTargetPos, ref vel, 0.25f) + Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.01f;
        }
        leftFootPos = leftFootTargetPos;
        leftFootRotation = transform.rotation;
        moveRoutine = null;
    }

}
