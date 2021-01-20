using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Linq;
using UnityEngine.InputSystem;
using System.Threading.Tasks;

public class GameCameras : MonoBehaviour
{
	public static GameCameras s;

	[System.NonSerialized] public CinemachineVirtualCameraBase currentCamera;

	[Header("Cameras")]
	public CinemachineFreeLook freeLook;
	public CinemachineVirtualCamera conversationCamera;

	[Header("Settings")]

	public float defaultTrackingOffset = 1.6f;
	float _playerTrackingOffset = 1.6f;

	public float defaultRigOffset = 0;
	public float playerRigOffset = 0;

	public float cameraTargetXOffset = 0;

	public InputReader inputReader;

	//default to everything but player
	public LayerMask cameraCollisionMask;
	public LayerMask doNotCollideWithCamera;
	public Transform cameraCollisionTarget;
	Camera regularCamera;

	bool controlling = true;
	public bool lockingMouse = true;
	Vector2 mouseDelta;

	//used to change how the height of the camera will change for a short time
	[System.NonSerialized] DebugMenu.DebugEntry<float, float> entry;

	Focusable focused = null;
	public Transform cameraTrackingTarget { get; private set; }

	public Transform cameraTransform;

	public bool cameraColision = true;
	public CinemachineTargetGroup conversationGroup;
	public DuelFocusGroup focusGroup;

	[Header("Vision")]
	public VirtualVision cameraVision;

	public float currentCameraLerp = 1f;
	float cameraLerpSpeed;
	float lastPhysicsLerp = 1f;


	//Previous bias from biasing the bias
	float freeLookBias = 0f;

	public bool m_UpdatingCameraDirection = false;

	public Vector3 FocusTarget => focused.transform.position;

	public Transform freeLookTarget
	{
		get => freeLook.Follow;
		set
		{
			freeLook.Follow = value;
			freeLook.LookAt = value;
		}
	}



	Vector3 CameraHalfExtends
	{
		get
		{
			Vector3 halfExtends;
			halfExtends.y =
				regularCamera.nearClipPlane *
				Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}


	Vector3 targetVel;

	private void Awake()
	{
		s = this;
		currentCamera = freeLook;
		Cinemachine.CinemachineCore.GetInputAxis = GetInputAxis;
	}

	public void Start()
	{
		entry = DebugMenu.CreateEntry("Player", "Direction ({0:0.0} / {1:0.0}) )", 180f, 0f);

		cameraTrackingTarget = LevelInfo.currentLevelInfo.playerTransform.Find("Camera_Track");
		regularCamera = cameraTransform.GetComponent<Camera>();

		freeLookTarget = cameraTrackingTarget;
		cameraCollisionTarget = freeLookTarget;



	}
	private void OnEnable()
	{
		inputReader.changeFocusEvent += OnChangeFocus;
	}
	private void OnDisable()
	{
		inputReader.changeFocusEvent -= OnChangeFocus;
	}
	public void OnChangeFocus(InputActionPhase phase)
	{
		if (phase == InputActionPhase.Performed)
		{

			float minX = focused == null ? 0 : regularCamera.WorldToScreenPoint(focused.transform.position).x;
			float closest = float.MaxValue;
			focused?.OnUnFocus();
			bool updated = false;
			foreach (var focus in Focusable.focusables)
			{
				if (focus.inVision)
				{
					float x = regularCamera.WorldToScreenPoint(focus.transform.position).x;

					if (x > minX && x < closest)
					{
						closest = x;
						focused = focus;
						updated = true;
					}
				}
			}

			if (!updated)
			{
				focused = null;
			}

			if (focused != null)
			{


				focusGroup.mainTarget = cameraTrackingTarget;
				focusGroup.focusTarget = focused.transform;

				cameraColision = false;
				freeLookTarget = focusGroup.transform;
				focused.OnFocus();

				if (!m_UpdatingCameraDirection) EnableRelitiveCameraAim();
				else
				{
					DisableRelitiveCameraAim();
					EnableRelitiveCameraAim();
				}
			}
			else StopFocus();
		}
	}

	public void StopFocus()
	{
		cameraColision = true;
		//cameraCollisionTarget = cameraTrackingTarget;
		//cameraTarget = cameraTrackingTarget;
		if (m_UpdatingCameraDirection) DisableRelitiveCameraAim();

		//SetCameraTargets(cameraTrackingTarget);

		freeLookTarget = cameraTrackingTarget;
		//freeLook.LookAt = cameraTrackingTarget;
		//freeLook.Follow = cameraTrackingTarget;
	}


	public void EnableRelitiveCameraAim()
	{
		m_UpdatingCameraDirection = true;
		//Offset back by the starting angle so that there will be no jump at the start
		freeLookBias -= FlatLookAngle(cameraTrackingTarget.position, focused.transform.position);


		//freeLook.OnTargetObjectWarped(conversationGroup.transform, conversationGroup.transform.position - (cameraTrackingTarget.position * 2 + focused.transform.position) / 3);

		//freeLook.m_BindingMode = Cinemachine.CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
		//freeLook.m_Heading.m_Bias = 0;
	}

	public void DisableRelitiveCameraAim()
	{
		m_UpdatingCameraDirection = false;
		freeLookBias = freeLook.m_Heading.m_Bias;

		//freeLook.m_BindingMode = Cinemachine.CinemachineTransposer.BindingMode.WorldSpace;
		//freeLook.m_Heading.m_Bias = freeLookBias;
	}




	public Vector3 TransformInput(Vector2 input)
	{
		//Rotate input around camera's rotation
		return Quaternion.Euler(0, GameCameras.s.cameraTransform.eulerAngles.y, 0) * new Vector3(input.x, 0, input.y);
	}

	public void DisableControl()
	{
		controlling = false;
		mouseDelta = Vector2.zero;
		GameCameras.s.freeLook.enabled = false;
	}
	public void EnableControl()
	{
		//start the transition from free camare to game camera
		controlling = true;
		GameCameras.s.freeLook.enabled = true;
	}


	float GetInputAxis(string axisName) => axisName switch
	{
		"Mouse X" => -mouseDelta.x,
		"Mouse Y" => -mouseDelta.y,
		_ => 0,
	};


	float FlatLookAngle(Vector3 from, Vector3 to)
	{
		Vector3 dir = cameraTrackingTarget.position - focused.transform.position;
		Vector2 flatDir = new Vector2(dir.x, dir.z);

		return Vector2.SignedAngle(flatDir, Vector2.up);
	}
	//Smootherstep between 0 and 1
	//https://en.wikipedia.org/wiki/Smoothstep
	float SmootherStep(float x)
	{
		// Scale, and clamp x to 0..1 range
		// Evaluate polynomial
		return x * x * x * (x * (x * 6 - 15) + 10);
	}

	public void LateUpdate()
	{
		if (m_UpdatingCameraDirection)
		{

			float angle = FlatLookAngle(cameraTrackingTarget.position, focused.transform.position);
			freeLook.m_Heading.m_Bias = freeLookBias + angle;

			focusGroup.weight = 1 - SmootherStep(Mathf.PingPong(freeLookBias + freeLook.m_XAxis.Value, 180f) / 180f);

			// conversationGroup.m_Targets[0].weight = Mathf.Cos(weight * Mathf.PI);
			// conversationGroup.m_Targets[1].weight = Mathf.Cos((1 - weight) * Mathf.PI);

			//conversationGroup.transform.eulerAngles = Vector3.up * (freeLookBias + angle);
			if (!focused.inVision)
			{
				StopFocus();
			}
		}

		if (controlling)
			mouseDelta = Mouse.current.delta.ReadValue() * SettingsManager.settings.sensitivity * 0.01f;

		Cursor.lockState = lockingMouse ? CursorLockMode.Locked : CursorLockMode.None;
		Cursor.visible = !lockingMouse;


		CameraVolumeController.UpdateVolumeEffect(transform.position);

	}
	private void OnDrawGizmos()
	{

		Gizmos.color = Color.blue;

		if (cameraCollisionTarget != null)
			Gizmos.DrawLine(cameraTransform.position, cameraCollisionTarget.position + new Vector3(0, _playerTrackingOffset, 0));

	}


	public float playerTrackingOffset
	{
		get => _playerTrackingOffset;
		set
		{
			_playerTrackingOffset = value;
			if (freeLook != null)
			{
				freeLook.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
				freeLook.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
				freeLook.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = Vector3.up * value;
			}
		}
	}

	private void Update()
	{

		cameraTrackingTarget.localPosition = Vector3.SmoothDamp(cameraTrackingTarget.localPosition, Vector3.right * cameraTargetXOffset, ref targetVel, 0.1f);

		cameraTransform.position = Vector3.Lerp(cameraCollisionTarget.position + new Vector3(0, _playerTrackingOffset, 0), cameraTransform.parent.position, currentCameraLerp);

		//Stop jittering from desync between high fps and low physics updates
		currentCameraLerp += cameraLerpSpeed * Time.deltaTime * 0.25f;
	}


	private void FixedUpdate()
	{
		if (!cameraColision) return;

		Vector3 halfExtents = CameraHalfExtends;
		Vector3 collisionTarget = cameraCollisionTarget.position + new Vector3(0, _playerTrackingOffset + halfExtents.y, 0);

		float distance = Vector3.Distance(collisionTarget, cameraTransform.parent.position);

		Collider[] cols = new Collider[1];
		int hits = Physics.OverlapBoxNonAlloc(
			cameraTransform.parent.position, halfExtents + new Vector3(0, 0, regularCamera.nearClipPlane),
			cols, cameraTransform.rotation, doNotCollideWithCamera);

		LayerMask l = cameraCollisionMask;
		if (hits > 0)
		{
			//camera is inside a do not collide with camera object, move in front
			l |= doNotCollideWithCamera;

		}

		//Linecast from camera to target to stop collision
		if (Physics.BoxCast(
			collisionTarget, halfExtents, -cameraTransform.forward,
			out RaycastHit hit, cameraTransform.rotation,
			distance - regularCamera.nearClipPlane, l))
		{
			cameraLerpSpeed = lastPhysicsLerp;
			//Hit something
			float newLerp = (hit.distance + regularCamera.nearClipPlane) / distance;

			if (lastPhysicsLerp < newLerp)
			{
				//Gotten closer to 1 so further away, lerp away
				currentCameraLerp = Mathf.Lerp(currentCameraLerp, newLerp, Time.deltaTime * 10);
			}
			else
			{
				//Gotten closer
				currentCameraLerp = newLerp;
			}

			lastPhysicsLerp = currentCameraLerp;


			cameraLerpSpeed -= lastPhysicsLerp;
			//Scale so it is in units per second. Also make negative
			cameraLerpSpeed /= -Time.fixedDeltaTime;

			//Debug.Log(cameraLerpSpeed);
		}
		else
		{
			//Did not hit, move back
			currentCameraLerp = Mathf.Lerp(currentCameraLerp, 1, Time.deltaTime * 10);

			cameraLerpSpeed = 0;
			lastPhysicsLerp = 0;
		}
	}



	public void SetCameraTargets(params Transform[] targets)
	{
		//Setup camera
		//Add all targets including the player
		conversationGroup.m_Targets = new CinemachineTargetGroup.Target[targets.Length];
		for (int i = 0; i < targets.Length; i++)
		{
			conversationGroup.m_Targets[i] = GenerateTarget(targets[i]);
		}
	}
	public void SetCameraTargets(Transform target0, Transform target1, float weight0, float weight1)
	{
		conversationGroup.m_Targets = new CinemachineTargetGroup.Target[2];

		conversationGroup.m_Targets[0] = GenerateTarget(target0, weight0);
		conversationGroup.m_Targets[1] = GenerateTarget(target1, weight1);
	}

	public void SetCameraTargets(Transform target)
	{
		conversationGroup.m_Targets = new CinemachineTargetGroup.Target[1];
		conversationGroup.m_Targets[0] = GenerateTarget(target);
	}

	public void SetCameraTargets(Transform target0, Transform target1) => SetCameraTargets(target0, target1, 1, 1);


	public void SwitchToCamera(CinemachineVirtualCameraBase camera)
	{
		//Make clipping work around the target
		cameraCollisionTarget = camera.transform;
		camera.enabled = true;

		currentCamera.enabled = false;

		currentCamera = camera;
	}

	public void EnableCutsceneCamera() => SwitchToCamera(conversationCamera);
	public void EnableFreeLookAimMode()
	{
		// SwitchToCamera(freeLookAim);
		// freeLookAim.ForceCameraPosition(freeLook.transform.position, freeLook.transform.rotation);

		// //Also set axis values

		// freeLookAim.m_XAxis.Value = freeLook.m_XAxis.Value;

		// //freeLookAim.m_YAxis.Value = freeLook.m_YAxis.Value;
		currentCameraLerp = 0.5f;
	}
	public void DisableFreeLookAimMode()
	{
		currentCameraLerp = 1f;
	}

	public void DisableCutsceneCamera() => SwitchToCamera(freeLook);

	public static CinemachineTargetGroup.Target GenerateTarget(Transform transform, float weight = 1, float radius = 1)
	{
		return new CinemachineTargetGroup.Target() { target = transform, weight = weight, radius = radius };
	}


}
