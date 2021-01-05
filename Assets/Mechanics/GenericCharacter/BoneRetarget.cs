using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoneRetarget : MonoBehaviour
{

    [SerializeField] Transform targetArmature;
    [SerializeField] SkinnedMeshRenderer targetRenderer;
    public string[] requiredBones;

    [MyBox.ButtonMethod]
    void SetTargetBones()
    {
        SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
        Transform[] boneArray = thisRenderer.bones;
        requiredBones = new string[boneArray.Length];
        for (int idx = 0; idx < boneArray.Length; ++idx)
        {
            requiredBones[idx] = boneArray[idx].name;

        }
    }

    [MyBox.ButtonMethod]
    // Use this for initialization
    void Reassign()
    {

        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();

        SearchChildren(targetArmature);
        void SearchChildren(Transform bone)
        {
            boneMap[bone.name] = bone;
            foreach (Transform child in bone)
            {
                SearchChildren(child);
            }
        }


        Transform[] boneArray = new Transform[requiredBones.Length];
        for (int idx = 0; idx < requiredBones.Length; ++idx)
        {
            if (!boneMap.TryGetValue(requiredBones[idx], out boneArray[idx]))
            {
                Debug.LogError("failed to get bone: " + requiredBones[idx]);
                Debug.Break();
            }
        }
        SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
        thisRenderer.bones = boneArray; //take effect
    }
    [MyBox.ButtonMethod]
    // Use this for initialization
    void CopyBones()
    {
        SkinnedMeshRenderer thisRenderer = GetComponent<SkinnedMeshRenderer>();
        thisRenderer.bones = targetRenderer.bones; //take effect
    }
}