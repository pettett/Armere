﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Patrol Routine", menuName = "Game/Patrol Routine", order = 0)]
public class PatrolRoutine : EnemyRoutine
{
    public float patrolSpeed = 3.5f;
    public float waitTime = 2;

    public bool holdWeaponDrawn = false;
    public bool overrideHeldWeapon = false;
    public ItemName holdingWeapon;

    //Used to light torches
    public bool lightFlammableBody = true;

    public async void SetupWeapon(EnemyAI enemy)
    {
        if (overrideHeldWeapon)
        {
            await enemy.SetHeldWeapon(holdingWeapon);

        }
        if (lightFlammableBody && enemy.weaponGraphics.holdables.melee.gameObject.TryGetComponent<FlammableBody>(out var f))
        {
            f.Light();
        }
    }

    public override IEnumerator Routine(EnemyAI enemy)
    {


        SetupWeapon(enemy);
        enemy.animationController.TriggerTransition(enemy.transitionSet.swordWalking);
        enemy.weaponGraphics.holdables.melee.sheathed = !holdWeaponDrawn;

        //Pick the closest waypoint by non -pathed distance
        int waypoint = enemy.GetClosestWaypoint();

        //If the player is seen, switch out of this routine
        enemy.investigateOnSight = true;
        enemy.searchOnEvent = true;
        enemy.agent.speed = patrolSpeed;
        while (true)
        {
            enemy.debugText.SetText($"Patrolling to {waypoint}");
            yield return enemy.GoToWaypoint(waypoint);

            yield return new WaitForSeconds(waitTime);
            waypoint++;
            if (waypoint == enemy.waypointGroup.Length) waypoint = 0;
        }
    }
}