using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraVolumeController
{
    public List<CameraVolume> activeVolumes = new List<CameraVolume>();

    readonly CinemachineFreeLook.Orbit[] defaultFreeOrbits;
    readonly CinemachineFreeLook.Orbit[] defaultFreeAimOrbits;

    static CameraVolumeController _s;

    public CameraVolumeController()
    {
        defaultFreeOrbits = GameCameras.s.freeLook.m_Orbits;
        defaultFreeAimOrbits = GameCameras.s.freeLookAim.m_Orbits;
    }

    public static CameraVolumeController s
    {
        get
        {
            return _s ?? (_s = new CameraVolumeController());
        }
    }


    public static void Register(CameraVolume v)
    {
        s.activeVolumes.Add(v);
    }
    public static void UnRegister(CameraVolume v)
    {
        s.activeVolumes.Remove(v);
    }
    public static CinemachineFreeLook.Orbit LerpOrbit(CinemachineFreeLook.Orbit a, CinemachineFreeLook.Orbit b, float t)
    {
        return new CinemachineFreeLook.Orbit(Mathf.Lerp(a.m_Height, b.m_Height, t), Mathf.Lerp(a.m_Radius, b.m_Radius, t));
    }
    public static void UpdateVolumeEffect(Vector3 position)
    {
        float biggestEffect = 0;

        CinemachineFreeLook.Orbit[] orbits = new CinemachineFreeLook.Orbit[3];

        foreach (var vol in s.activeVolumes)
        {
            Vector3 closestPoint = vol.c.ClosestPoint(position);
            float sqrDistance = (closestPoint - position).sqrMagnitude;

            if (sqrDistance < vol.blendDistance * vol.blendDistance)
            {
                float e = 1 - Mathf.Sqrt(sqrDistance) / vol.blendDistance;
                if (e > biggestEffect)
                {
                    biggestEffect = e;
                    orbits[0] = LerpOrbit(s.defaultFreeOrbits[0], vol.topRig, e);//Top Orbit
                    orbits[1] = LerpOrbit(s.defaultFreeOrbits[1], vol.middleRig, e); ;//Mid Orbit
                    orbits[2] = LerpOrbit(s.defaultFreeOrbits[2], vol.bottomRig, e); ;//Bottom Orbit
                }
            }
        }
        if (biggestEffect != 0)
            GameCameras.s.freeLook.m_Orbits = orbits;
        else
            GameCameras.s.freeLook.m_Orbits = s.defaultFreeOrbits;

    }

}
