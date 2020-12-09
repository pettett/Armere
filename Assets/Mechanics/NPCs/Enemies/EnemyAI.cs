using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(Health), typeof(WeaponGraphicsController), typeof(Ragdoller))]
public class EnemyAI : AIBase
{

    public enum SightMode { View, Range }

    public SightMode sightMode;
    public ItemName meleeWeapon;
    public EnemyRoutine idleRoutine;
    public bool autoEngage = false;

    [HideInInspector] public Health health;
    [Header("Player Detection")]
    public Vector2 clippingPlanes = new Vector2(0.1f, 10f);
    [MyBox.ConditionalField("sightMode", false, SightMode.View)] [Range(1, 90)] public float fov = 45;
    public Transform eye; //Used for vision frustum calculations
    public LayerMask visionBlockingMask;
    Collider playerCollider;
    public AnimationCurve investigateRateOverDistance = AnimationCurve.EaseInOut(0, 1, 1, 0.1f);

    public AIWaypointGroup waypointGroup;

    [Header("Player Engagement")]

    public float approachDistance = 1;
    float sqrApproachDistance => approachDistance * approachDistance;
    public bool approachPlayer = true;

    public float knockoutTime = 4f;

    Coroutine currentRoutine;

    [System.NonSerialized] public bool investigateOnSight = false;
    [System.NonSerialized] public bool searchOnEvent = false;
    [System.NonSerialized] public bool engageOnAttack = true;

    public AnimationTransitionSet transitionSet;
    [System.NonSerialized] public WeaponGraphicsController weaponGraphics;
    [System.NonSerialized] public AnimationController animationController;
    Matrix4x4 viewMatrix;
    Plane[] viewPlanes = new Plane[6];
    [Header("Indicators")]
    public float height = 1.8f;
    AlertIndicatorUI alert;
    Ragdoller ragdoller;

    Transform lookingAtTarget;
    Vector3 lookingAtOffset = Vector3.up * 1.6f;




    public event System.Action<EnemyAI> onPlayerDetected;

    public void OnDamageTaken(GameObject attacker, GameObject victim)
    {
        //Push the ai back
        if (engageOnAttack)
            ChangeRoutine(Alert(1));
    }

    [MyBox.ButtonMethod()]
    public void Ragdoll()
    {
        ChangeRoutine(Knockout());
    }

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

    public void Die(GameObject attacker, GameObject victim)
    {
        ChangeRoutine(DieRoutine());
    }

    protected override async void Start()
    {
        playerCollider = LevelInfo.currentLevelInfo.player.GetComponent<Collider>();

        health = GetComponent<Health>();
        weaponGraphics = GetComponent<WeaponGraphicsController>();
        animationController = GetComponent<AnimationController>();
        ragdoller = GetComponent<Ragdoller>();

        ragdoller.RagdollEnabled = false;

        health.onTakeDamage += OnDamageTaken;
        health.onDeath += Die;

        base.Start();

        GetComponent<VirtualAudioListener>().onHearNoise += OnNoiseHeard;

        await SetHeldWeapon(meleeWeapon);
    }


    public void OnNoiseHeard(Vector3 position)
    {
        if (searchOnEvent)
        {
            ChangeRoutine(SearchForEvent(position));
        }
    }


    public async Task SetHeldWeapon(ItemName weapon)
    {
        await weaponGraphics.holdables.melee.SetHeld((HoldableItemData)InventoryController.singleton.db[weapon]);
    }

    public virtual void InitEnemy()
    {
        StartBaseRoutine();
    }


    IEnumerator DrawItem(ItemType type)
    {
        yield return weaponGraphics.DrawItem(type, transitionSet);
    }

    IEnumerator SheathItem(ItemType type)
    {
        yield return weaponGraphics.SheathItem(type, transitionSet);
    }


    protected void StartBaseRoutine()
    {
        if (alert != null) Destroy(alert.gameObject);
        if (autoEngage)
        {
            ChangeRoutine(Alert(0));
        }
        else
        {
            ChangeRoutine(idleRoutine.Routine(this));
        }
    }

    IEnumerator DieRoutine()
    {
        investigateOnSight = false;
        searchOnEvent = false;
        engageOnAttack = false;

        foreach (var x in weaponGraphics.holdables)
            x.RemoveHeld();

        ragdoller.RagdollEnabled = true;
        yield return new WaitForSeconds(4);
        Destroy(gameObject);
    }

    public int GetClosestWaypoint()
    {
        //Pick the closest waypoint by non -pathed distance
        int waypoint = 0;
        for (int i = 1; i < waypointGroup.Length; i++)
        {
            if ((transform.position - waypointGroup[waypoint].position).sqrMagnitude > (transform.position - waypointGroup[i].position).sqrMagnitude)
                waypoint = i;
        }
        return waypoint;
    }
    public IEnumerator GoToWaypoint(int index) => GoToTransform(waypointGroup[index]);





    IEnumerator Investigate()
    {
        //Do not re-enter investigate
        investigateOnSight = false;
        engageOnAttack = true;
        searchOnEvent = false;
        if (alert == null || alert.gameObject == null)
            alert = IndicatorsUIController.singleton.CreateAlertIndicator(transform, Vector3.up * height);

        alert.EnableInvestigate(true);
        alert.EnableAlert(false);

        float investProgress = 0;
        //Try to look at the player long enough to alert
        while (true)
        {
            alert.SetInvestigation(investProgress);

            float visibility = ProportionBoundsVisible(playerCollider.bounds);

            if (visibility != 0)
            {
                //can see player
                //Distance is the 0-1 scale where 0 is closestest visiable and 1 is furthest video
                float playerDistance = Mathf.InverseLerp(clippingPlanes.x, clippingPlanes.y, Vector3.Distance(eye.position, playerCollider.transform.position));
                //Invest the player slower if they are further away
                investProgress += Time.deltaTime * investigateRateOverDistance.Evaluate(playerDistance) * visibility;
            }
            else
            {
                investProgress -= Time.deltaTime;
            }


            if (investProgress < -0.5f)
            {
                //Cannot see player
                StartBaseRoutine();

                break;
            }
            else if (investProgress >= 1)
            {
                //Seen player
                ChangeRoutine(Alert(1));
                break;
            }


            yield return null;
        }
    }

    IEnumerator Knockout()
    {
        ragdoller.RagdollEnabled = true;
        yield return new WaitForSeconds(knockoutTime);
        ragdoller.RagdollEnabled = false;
    }

    IEnumerator Alert(float waitTime)
    {
        lookingAtTarget = playerCollider.transform;

        onPlayerDetected?.Invoke(this);


        animationController.TriggerTransition(transitionSet.surprised);
        yield return new WaitForSeconds(1);

        if (weaponGraphics.holdables.melee.sheathed)
            yield return DrawItem(ItemType.Melee);


        //If alert is null create one
        alert = alert ?? IndicatorsUIController.singleton.CreateAlertIndicator(transform, Vector3.up * height);

        alert.EnableInvestigate(false);
        alert.EnableAlert(true);
        yield return new WaitForSeconds(1);
        //print("Alerted");
        Destroy(alert.gameObject);
        ChangeRoutine(EngagePlayer());
    }

    public void ForceEngage()
    {
        ChangeRoutine(Alert(0));
    }


    IEnumerator SearchForEvent(Vector3 eventPos)
    {
        investigateOnSight = true;
        searchOnEvent = true;//Go to more recent events than this


        /*
        Investigate routine:
            Go to close enough distance to event
            Rotate to event
            Wait there, looking around a bit
            go back to what we were doing before
        */

        debugText.SetText("Searching");
        yield return RotateTo(Quaternion.LookRotation(eventPos - transform.position), agent.angularSpeed);
        debugText.SetText("Searching - looking");
        yield return new WaitForSeconds(3);
    }

    IEnumerator EngagePlayer()
    {
        //Once they player has attacked or been seen, do not stop engageing until circumstances change
        engageOnAttack = false;
        investigateOnSight = false;
        searchOnEvent = false;
        agent.isStopped = true;

        Vector3 directionToPlayer;
        Health playerHealth = playerCollider.GetComponent<Health>();
        bool movingToCatchPlayer = false;
        lookingAtTarget = playerCollider.transform;
        //Stop attacking the player after it has died
        while (!playerHealth.dead)
        {
            directionToPlayer = playerCollider.transform.position - transform.position;
            if (approachPlayer && directionToPlayer.sqrMagnitude > sqrApproachDistance)
            {
                if (!movingToCatchPlayer)
                {
                    movingToCatchPlayer = true;
                    yield return new WaitForSeconds(0.1f);
                }

                agent.Move(directionToPlayer.normalized * Time.deltaTime * agent.speed);
            }
            else if (movingToCatchPlayer)
            {
                movingToCatchPlayer = false;
                //Small delay to adjust to stopped movement
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                //Within sword range of player
                //Swing sword
                yield return SwingSword();
            }

            directionToPlayer.y = 0;
            transform.forward = directionToPlayer;

            //TODO: Test to see if the player is still in view


            yield return null;
        }
        //Once the player has died, return to normal routine to stop end looking janky
        StartBaseRoutine();
    }


    IEnumerator SwingSword()
    {
        agent.isStopped = true; //Stop the player moving
        //swing the sword

        //This is easier. Animation graphs suck
        animationController.TriggerTransition(transitionSet.swingSword);

        WeaponTrigger trigger = null;


        void AddTrigger()
        {
            //Add collider and trigger logic to the blade object
            trigger = weaponGraphics.holdables.melee.gameObject.GetComponent<WeaponTrigger>();

            trigger.enableTrigger = true;

            if (!trigger.inited)
            {
                trigger.Init(((MeleeWeaponItemData)InventoryController.singleton.db[meleeWeapon]).hitSparkEffect);
                trigger.weaponItem = meleeWeapon;
                trigger.controller = gameObject;
            }

        }

        void RemoveTrigger()
        {
            //Clean up the trigger detection of the sword

            trigger.enableTrigger = false;
            onSwingStateChanged = null;
        }

        onSwingStateChanged = (bool on) =>
        {
            if (on) AddTrigger();
            else RemoveTrigger();
        };

        yield return new WaitForSeconds(1);
    }
    //Triggered by animations
    public System.Action<bool> onSwingStateChanged;
    public void SwingStart() => onSwingStateChanged?.Invoke(true);
    public void SwingEnd() => onSwingStateChanged?.Invoke(false);


    void ChangeRoutine(IEnumerator newRoutine)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(newRoutine);
    }

    public bool CanSeeBounds(Bounds b)
    {
        if (sightMode == SightMode.View)
        {
            viewMatrix = Matrix4x4.Perspective(fov, 1, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1));
            GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, viewPlanes);
            return GeometryUtility.TestPlanesAABB(viewPlanes, b);
        }
        else if (sightMode == SightMode.Range)
        {
            float sqrDistance = (b.center - eye.position).sqrMagnitude;
            if (sqrDistance < clippingPlanes.y * clippingPlanes.y && sqrDistance > clippingPlanes.x * clippingPlanes.x)
            {
                return true;
            }
            else return false;
        }
        return false;
    }
    public float ProportionBoundsVisible(Bounds b)
    {
        if (sightMode == SightMode.View)
        {
            viewMatrix = Matrix4x4.Perspective(fov, 1, clippingPlanes.x, clippingPlanes.y) * Matrix4x4.Scale(new Vector3(1, 1, -1));
            GeometryUtility.CalculateFrustumPlanes(viewMatrix * eye.worldToLocalMatrix, viewPlanes);

            float visibility = 0;
            int samples = 2;

            for (int i = 0; i < samples; i++)
            {
                Vector3 testPoint = b.center;
                testPoint.y += b.size.y * (i / (samples - 1f)) - b.extents.y;

                foreach (var plane in viewPlanes)
                {
                    if (!plane.GetSide(testPoint))
                    {
                        //This point is not inside frustum, ignore it
                        goto SkipPoint;
                    }
                }

                //Line cast to point
                if (!Physics.Linecast(eye.position, testPoint, out RaycastHit hit, visionBlockingMask, QueryTriggerInteraction.Ignore))
                {
                    //Add to visibility
                    visibility += 1f / samples;

                }

            SkipPoint:
                continue;

            }

            return visibility;
        }

        return 1;
    }

    private void Update()
    {
        //Test if the enemy can see the player at this point
        if (investigateOnSight)
        {
            var b = playerCollider.bounds;
            if (ProportionBoundsVisible(b) != 0)
            {
                //can see the player, interrupt current routine
                ChangeRoutine(Investigate());
            }
        }

        float speed = Mathf.Sign(agent.velocity.sqrMagnitude);

        anim.SetBool("Idle", speed == 0);
        anim.SetFloat("InputVertical", agent.velocity.sqrMagnitude / (agent.speed * agent.speed), 0.01f, Time.deltaTime);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (lookingAtTarget != null)
            LookAtPlayer(lookingAtTarget.position + lookingAtOffset);
    }

    private void OnDrawGizmos()
    {

        if (playerCollider != null)
        {
            var b = playerCollider.bounds;

            float visibility = ProportionBoundsVisible(b);
            if (CanSeeBounds(b))
            {
                Gizmos.color = new Color(visibility, 0, 0);
                Gizmos.DrawWireCube(b.center, b.size);
            }



            if (sightMode == SightMode.View)
            {
                Gizmos.color = Color.white;
                Gizmos.matrix = eye.localToWorldMatrix;
                Gizmos.DrawFrustum(Vector3.zero, fov, clippingPlanes.y, clippingPlanes.x, 1f);
            }
            else if (sightMode == SightMode.Range)
            {
                Gizmos.DrawWireSphere(eye.position, clippingPlanes.x);
                Gizmos.DrawWireSphere(eye.position, clippingPlanes.y);
            }
        }
    }
}