using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraVolumeController
{
    public List<CameraVolume> activeVolumes = new List<CameraVolume>();
    CameraVolume globalVolume;
    CameraProfile oldOverrideProfile;
    CameraProfile overrideProfile;

    readonly CinemachineFreeLook.Orbit[] defaultFreeAimOrbits;

    static CameraVolumeController _s;

    public float overrideProfileBlend = 0;


    public CameraVolumeController()
    {
        // defaultFreeAimOrbits = GameCameras.s.freeLookAim.m_Orbits;
    }


    public IEnumerator ApplyOverrideProfile(CameraProfile overrideProfile, float time)
    {

        float t = 0;
        this.overrideProfile = overrideProfile;
        while (t < time)
        {
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;
            overrideProfileBlend = t / time;
        }
        overrideProfileBlend = 1;
        this.oldOverrideProfile = overrideProfile;
    }



    public static CameraVolumeController s
    {
        get
        {
            return _s ?? (_s = new CameraVolumeController());
        }
    }


    public static void Register(CameraVolume v, bool global)
    {
        if (global)
            s.globalVolume = v;
        else
            s.activeVolumes.Add(v);
    }
    public static void UnRegister(CameraVolume v, bool global)
    {
        if (global && v == s.globalVolume)
            s.globalVolume = null;
        else if (s.activeVolumes.Contains(v))
            s.activeVolumes.Remove(v);
    }
    public static CinemachineFreeLook.Orbit LerpOrbit(CinemachineFreeLook.Orbit a, CinemachineFreeLook.Orbit b, float t)
    {
        return new CinemachineFreeLook.Orbit(Mathf.Lerp(a.m_Height, b.m_Height, t), Mathf.Lerp(a.m_Radius, b.m_Radius, t));
    }
    public static void UpdateVolumeEffect(Vector3 position)
    {

        if (s.overrideProfile != null)
        {
            if (s.overrideProfileBlend == 1)
                ApplyProfile(s.overrideProfile);
            else if (s.oldOverrideProfile != null && s.overrideProfile != null)
            {
                //lerp between override profiles
                ApplyLerpProfile(s.oldOverrideProfile, s.overrideProfile, s.overrideProfileBlend);
                return;
            }
        }

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
                    orbits[0] = LerpOrbit(s.globalVolume.profile.topRig, vol.profile.topRig, e);//Top Orbit
                    orbits[1] = LerpOrbit(s.globalVolume.profile.middleRig, vol.profile.middleRig, e); ;//Mid Orbit
                    orbits[2] = LerpOrbit(s.globalVolume.profile.bottomRig, vol.profile.bottomRig, e); ;//Bottom Orbit
                }
            }
        }

        if (biggestEffect != 0)
            GameCameras.s.freeLook.m_Orbits = orbits;
        else if (s.overrideProfile != null)
        {
            ApplyLerpProfile(s.globalVolume.profile, s.overrideProfile, s.overrideProfileBlend);
        }
        else if (s.oldOverrideProfile != null)
        {
            ApplyLerpProfile(s.oldOverrideProfile, s.globalVolume.profile, s.overrideProfileBlend);
        }
        else
        {
            ApplyProfile(s.globalVolume.profile);
        }

        //Offset by current camera rig
        GameCameras.s.freeLook.m_Orbits[0].m_Height -= GameCameras.s.playerRigOffset;
        GameCameras.s.freeLook.m_Orbits[1].m_Height -= GameCameras.s.playerRigOffset;
        GameCameras.s.freeLook.m_Orbits[2].m_Height -= GameCameras.s.playerRigOffset;

    }

    static void ApplyLerpProfile(CameraProfile a, CameraProfile b, float t)
    {
        GameCameras.s.freeLook.m_Orbits[0] = LerpOrbit(a.topRig, b.topRig, t);//Top Orbit
        GameCameras.s.freeLook.m_Orbits[1] = LerpOrbit(a.middleRig, b.middleRig, t); ;//Mid Orbit
        GameCameras.s.freeLook.m_Orbits[2] = LerpOrbit(a.bottomRig, b.bottomRig, t); ;//Bottom Orbit
    }
    static void ApplyProfile(CameraProfile p)
    {
        GameCameras.s.freeLook.m_Orbits[0] = p.topRig;
        GameCameras.s.freeLook.m_Orbits[1] = p.middleRig;
        GameCameras.s.freeLook.m_Orbits[2] = p.bottomRig;
    }
}
