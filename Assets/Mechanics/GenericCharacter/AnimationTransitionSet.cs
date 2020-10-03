using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Animation Transition Set", menuName = "Game/Animation Transition Set", order = 0)]
public class AnimationTransitionSet : ScriptableObject
{

    public AnimationTransition swingSword = new AnimationTransition("Sword Slash", 0.05f, 0.15f, Layers.BaseLayer);
    public AnimationTransition sheathSword = new AnimationTransition("Sheath Sword", 0.05f, 0.15f, Layers.UpperBody);
    public AnimationTransition drawSword = new AnimationTransition("Draw Sword", 0.05f, 0.05f, Layers.UpperBody);
    public AnimationTransition swordWalking = new AnimationTransition("Sword Walking", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition freeMovement = new AnimationTransition("Free Movement", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition swordBackImpact = new AnimationTransition("Back Impact", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition swordFrontImpact = new AnimationTransition("Front Impact", 0.05f, 0.05f, Layers.BaseLayer);
}
