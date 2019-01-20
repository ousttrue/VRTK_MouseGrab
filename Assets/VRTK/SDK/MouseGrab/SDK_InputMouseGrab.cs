// VR Simulator|Prefabs|0005
namespace VRTK
{
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The `VRSimulatorCameraRig` prefab is a mock Camera Rig set up that can be used to develop with VRTK without the need for VR Hardware.
    /// </summary>
    /// <remarks>
    /// Use the mouse and keyboard to move around both play area and hands and interacting with objects without the need of a hmd or VR controls.
    /// </remarks>
    public class SDK_InputMouseGrab : MonoBehaviour
    {
        /// <summary>
        /// Mouse input mode types
        /// </summary>
        /// <param name="Always">Mouse movement is always treated as mouse input.</param>
        /// <param name="RequiresButtonPress">Mouse movement is only treated as movement when a button is pressed.</param>
        public enum MouseInputMode
        {
            Always,
            RequiresButtonPress
        }

        #region Public fields

        [Tooltip("Hide hands when disabling them.")]
        public bool hideHandsAtSwitch = false;
        [Tooltip("Reset hand position and rotation when enabling them.")]
        public bool resetHandsAtSwitch = true;
        [Tooltip("Whether mouse movement always acts as input or requires a button press.")]
        public MouseInputMode mouseMovementInput = MouseInputMode.Always;
        [Tooltip("Lock the mouse cursor to the game window when the mouse movement key is pressed.")]
        public bool lockMouseToView = true;

        [Header("Adjustments")]

        [Tooltip("Adjust hand movement speed.")]
        public float handMoveMultiplier = 0.002f;
        [Tooltip("Adjust hand rotation speed.")]
        public float handRotationMultiplier = 0.5f;
        [Tooltip("Adjust player movement speed.")]
        public float playerMoveMultiplier = 5;
        [Tooltip("Adjust player rotation speed.")]
        public float playerRotationMultiplier = 0.5f;
        [Tooltip("Adjust player sprint speed.")]
        public float playerSprintMultiplier = 2;

        [Header("Operation Key Bindings")]

        [Tooltip("Key used to enable mouse input if a button press is required.")]
        public KeyCode mouseMovementKey = KeyCode.Mouse1;
        [Tooltip("Key used to toggle control hints on/off.")]
        public KeyCode toggleControlHints = KeyCode.F1;
        [Tooltip("Key used to switch between left and righ hand.")]
        public KeyCode changeHands = KeyCode.Tab;
        [Tooltip("Key used to switch hands On/Off.")]
        public KeyCode handsOnOff = KeyCode.LeftAlt;
        [Tooltip("Key used to switch between positional and rotational movement.")]
        public KeyCode rotationPosition = KeyCode.LeftShift;
        [Tooltip("Key used to switch between X/Y and X/Z axis.")]
        public KeyCode changeAxis = KeyCode.LeftControl;
        [Tooltip("Key used to distance pickup with left hand.")]
        public KeyCode distancePickupLeft = KeyCode.Mouse0;
        [Tooltip("Key used to distance pickup with right hand.")]
        public KeyCode distancePickupRight = KeyCode.Mouse1;
        [Tooltip("Key used to enable distance pickup.")]
        public KeyCode distancePickupModifier = KeyCode.LeftControl;

        [Header("Movement Key Bindings")]

        [Tooltip("Key used to move forward.")]
        public KeyCode moveForward = KeyCode.W;
        [Tooltip("Key used to move to the left.")]
        public KeyCode moveLeft = KeyCode.A;
        [Tooltip("Key used to move backwards.")]
        public KeyCode moveBackward = KeyCode.S;
        [Tooltip("Key used to move to the right.")]
        public KeyCode moveRight = KeyCode.D;
        [Tooltip("Key used to sprint.")]
        public KeyCode sprint = KeyCode.LeftShift;

        [Header("Controller Key Bindings")]
        [Tooltip("Key used to simulate trigger button.")]
        public KeyCode triggerAlias = KeyCode.Mouse1;
        [Tooltip("Key used to simulate grip button.")]
        public KeyCode gripAlias = KeyCode.Mouse0;
        [Tooltip("Key used to simulate touchpad button.")]
        public KeyCode touchpadAlias = KeyCode.Q;
        [Tooltip("Key used to simulate button one.")]
        public KeyCode buttonOneAlias = KeyCode.E;
        [Tooltip("Key used to simulate button two.")]
        public KeyCode buttonTwoAlias = KeyCode.R;
        [Tooltip("Key used to simulate start menu button.")]
        public KeyCode startMenuAlias = KeyCode.F;
        [Tooltip("Key used to switch between button touch and button press mode.")]
        public KeyCode touchModifier = KeyCode.T;
        [Tooltip("Key used to switch between hair touch mode.")]
        public KeyCode hairTouchModifier = KeyCode.H;

        #endregion
        #region Private fields

        private Transform rightHand;
        private Transform leftHand;
        private Transform currentHand;
        private Vector3 oldPos;
        private Transform neck;
        private SDK_ControllerMouseGrab rightController;
        private SDK_ControllerMouseGrab leftController;
        private static GameObject cachedCameraRig;
        private static bool destroyed = false;
        private float sprintMultiplier = 1;

        #endregion

        /// <summary>
        /// The FindInScene method is used to find the `VRMouseGrabCameraRig` GameObject within the current scene.
        /// </summary>
        /// <returns>Returns the found `VRMouseGrabCameraRig` GameObject if it is found. If it is not found then it prints a debug log error.</returns>
        public static GameObject FindInScene()
        {
            if (cachedCameraRig == null && !destroyed)
            {
                cachedCameraRig = VRTK_SharedMethods.FindEvenInactiveGameObject<SDK_InputMouseGrab>();
                if (!cachedCameraRig)
                {
                    VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_SCENE, "VRMouseGrabCameraRig", "SDK_InputMouseGrab", ". check that the `VRTK/Prefabs/VRMouseGrabCameraRig` prefab been added to the scene."));
                }
            }
            return cachedCameraRig;
        }

        private void Awake()
        {
            VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
        }

        private void OnEnable()
        {
            rightHand = transform.Find("RightHand");
            rightHand.gameObject.SetActive(false);
            leftHand = transform.Find("LeftHand");
            leftHand.gameObject.SetActive(false);
            currentHand = rightHand;
            oldPos = Input.mousePosition;
            neck = transform.Find("Neck");
            leftHand.Find("Hand").GetComponent<Renderer>().material.color = Color.red;
            rightHand.Find("Hand").GetComponent<Renderer>().material.color = Color.green;
            rightController = rightHand.GetComponent<SDK_ControllerMouseGrab>();
            leftController = leftHand.GetComponent<SDK_ControllerMouseGrab>();
            rightController.Selected = true;
            leftController.Selected = false;
            destroyed = false;

            var controllerSDK = VRTK_SDK_Bridge.GetControllerSDK() as SDK_MouseGrabController;
            if (controllerSDK != null)
            {
                Dictionary<string, KeyCode> keyMappings = new Dictionary<string, KeyCode>()
                {
                    {"Trigger", triggerAlias },
                    {"Grip", gripAlias },
                    {"TouchpadPress", touchpadAlias },
                    {"ButtonOne", buttonOneAlias },
                    {"ButtonTwo", buttonTwoAlias },
                    {"StartMenu", startMenuAlias },
                    {"TouchModifier", touchModifier },
                    {"HairTouchModifier", hairTouchModifier }
                };
                controllerSDK.SetKeyMappings(keyMappings);
            }
            rightHand.gameObject.SetActive(true);
            leftHand.gameObject.SetActive(true);

            SetMove();
            SetHand();
        }

        private void OnDestroy()
        {
            VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
            destroyed = true;
        }

        private void Update()
        {
            if (mouseMovementInput == MouseInputMode.RequiresButtonPress)
            {
                if (lockMouseToView)
                {
                    Cursor.lockState = Input.GetKey(mouseMovementKey) ? CursorLockMode.Locked : CursorLockMode.None;
                }
                else if (Input.GetKeyDown(mouseMovementKey))
                {
                    oldPos = Input.mousePosition;
                }
            }

            if (Input.GetKeyDown(changeHands))
            {
                if (currentHand.name == "LeftHand")
                {
                    currentHand = rightHand;
                    rightController.Selected = true;
                    leftController.Selected = false;
                }
                else
                {
                    currentHand = leftHand;
                    rightController.Selected = false;
                    leftController.Selected = true;
                }
            }

            {
                UpdateHands();
                UpdateRotation();
                if(Input.GetKeyDown(distancePickupRight) && Input.GetKey(distancePickupModifier))
                {
                    TryPickup(true);
                }
                else if(Input.GetKeyDown(distancePickupLeft) && Input.GetKey(distancePickupModifier))
                {
                    TryPickup(false);
                }
                if(Input.GetKey(sprint))
                {
                    sprintMultiplier = playerSprintMultiplier;
                }
                else
                {
                    sprintMultiplier = 1;
                }
            }

            UpdatePosition();
        }

        private void TryPickup(bool rightHand)
        {
            Ray screenRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if(Physics.Raycast(screenRay, out hit))
            {
                VRTK_InteractableObject io = hit.collider.gameObject.GetComponent<VRTK_InteractableObject>();
                if(io)
                {
                    GameObject hand;
                    if(rightHand)
                    {
                        hand = VRTK_DeviceFinder.GetControllerRightHand();
                    }
                    else
                    {
                        hand = VRTK_DeviceFinder.GetControllerLeftHand();
                    }
                    VRTK_InteractGrab grab = hand.GetComponent<VRTK_InteractGrab>();
                    if(grab.GetGrabbedObject() == null)
                    {
                        hand.GetComponent<VRTK_InteractTouch>().ForceTouch(hit.collider.gameObject);
                        grab.AttemptGrab();
                    }
                }
            }
        }

        float _distance = 2.0f;
        private void UpdateHands()
        {
            var c = Camera.main;
            Ray ray = c.ScreenPointToRay(Input.mousePosition);
            currentHand.position = c.transform.position + ray.direction * _distance;
        }

        const float _rotDegrees = 15.0f;
        Vector3 _rot;
        private void UpdateRotation()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                _rot.x += _rotDegrees;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                _rot.x -= _rotDegrees;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _rot.y -= _rotDegrees;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _rot.y += _rotDegrees;
            }
            transform.rotation = Quaternion.Euler(_rot);
        }

        private void UpdatePosition()
        {
            float moveMod = Time.deltaTime * playerMoveMultiplier * sprintMultiplier;
            if (Input.GetKey(moveForward))
            {
                transform.Translate(transform.forward * moveMod, Space.World);
            }
            else if (Input.GetKey(moveBackward))
            {
                transform.Translate(-transform.forward * moveMod, Space.World);
            }
            if (Input.GetKey(moveLeft))
            {
                transform.Translate(-transform.right * moveMod, Space.World);
            }
            else if (Input.GetKey(moveRight))
            {
                transform.Translate(transform.right * moveMod, Space.World);
            }
        }

        private void SetHand()
        {
            Cursor.visible = false;
            rightHand.gameObject.SetActive(true);
            leftHand.gameObject.SetActive(true);
            oldPos = Input.mousePosition;
            if (resetHandsAtSwitch)
            {
                rightHand.transform.localPosition = new Vector3(0.2f, 1.2f, 0.5f);
                rightHand.transform.localRotation = Quaternion.identity;
                leftHand.transform.localPosition = new Vector3(-0.2f, 1.2f, 0.5f);
                leftHand.transform.localRotation = Quaternion.identity;
            }
        }

        private void SetMove()
        {
            Cursor.visible = true;
            if (hideHandsAtSwitch)
            {
                rightHand.gameObject.SetActive(false);
                leftHand.gameObject.SetActive(false);
            }
        }

        private bool IsAcceptingMouseInput()
        {
            return mouseMovementInput == MouseInputMode.Always || Input.GetKey(mouseMovementKey);
        }

        private Vector3 GetMouseDelta()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }
            else
            {
                Vector3 mouseDiff = Input.mousePosition - oldPos;
                oldPos = Input.mousePosition;
                return mouseDiff;
            }
        }

        private void OnDrawGizmos()
        {
            var c = Camera.main;

            Gizmos.color = Color.green;
            Gizmos.matrix = c.transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, 
                Camera.main.fieldOfView, 
                Camera.main.farClipPlane, 
                Camera.main.nearClipPlane, 
                Camera.main.aspect);
            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(c.transform.position, currentHand.position);
        }
    }
}
