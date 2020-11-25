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
    public AnimationTransition bowWalking = new AnimationTransition("Strafing Movement", 0.05f, 0.05f, Layers.BaseLayer);



    public AnimationTransition freeMovement = new AnimationTransition("Free Movement", 0.05f, 0.05f, Layers.BaseLayer);


    public AnimationTransition startSitting = new AnimationTransition("Start Sit", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition stopSitting = new AnimationTransition("Stop Sit", 0.05f, 0.05f, Layers.BaseLayer);


    public AnimationTransition shieldRaise = new AnimationTransition("Raise Shield", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition shieldLower = new AnimationTransition("Lower Shield", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition shieldImpact = new AnimationTransition("Shield Impact", 0.05f, 0.05f, Layers.BaseLayer);

    public AnimationTransition swordBackImpact = new AnimationTransition("Back Impact", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition swordFrontImpact = new AnimationTransition("Front Impact", 0.05f, 0.05f, Layers.BaseLayer);

    [Header("Ladders")]
    public AnimationTransition ladderClimb = new AnimationTransition("Climbing Ladder", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition stepDownFromLadder = new AnimationTransition("Step Down Exit", 0.05f, 0.05f, Layers.BaseLayer);
    public AnimationTransition climbUpFromLadder = new AnimationTransition("Climb Up Exit", 0.05f, 0.05f, Layers.BaseLayer);

    [Header("Reactions")]
    public AnimationTransition surprised = new AnimationTransition("Surprised", 0.05f, 0.05f, Layers.BaseLayer);


}
