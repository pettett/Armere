using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteAlways]
public class WaterTrailController : MonoBehaviour
{

    public VisualEffect waterTrail;

    int stopEventID;
    int startEventID;

    // Start is called before the first frame update
    void Start()
    {
        stopEventID = Shader.PropertyToID("EndTrail");
        startEventID = Shader.PropertyToID("StartTrail");
    }

    [MyBox.ButtonMethod]
    public void StartTrail()
    {
        waterTrail?.SendEvent(startEventID);
    }
    [MyBox.ButtonMethod]
    public void StopTrail()
    {
        waterTrail?.SendEvent(stopEventID);
    }
    public void DestroyOnFinish()
    {
        StartCoroutine(DestroyWhenDone());
    }
    IEnumerator DestroyWhenDone()
    {
        yield return new WaitUntil(() => waterTrail.aliveParticleCount == 0);
        Destroy(gameObject);
    }

}
