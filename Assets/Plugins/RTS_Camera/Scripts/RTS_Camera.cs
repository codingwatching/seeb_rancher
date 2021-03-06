using UnityEngine;

namespace RTS_Cam
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("RTS Camera")]
    public class RTS_Camera : MonoBehaviour
    {

        #region Foldouts

#if UNITY_EDITOR

        public int lastTab = 0;

        public bool movementSettingsFoldout;
        public bool zoomingSettingsFoldout;
        public bool rotationSettingsFoldout;
        public bool heightSettingsFoldout;
        public bool mapLimitSettingsFoldout;
        public bool targetingSettingsFoldout;
        public bool inputSettingsFoldout;

#endif

        #endregion

        private Transform m_Transform; //camera tranform
        public bool useFixedUpdate = false; //use FixedUpdate() or Update()

        #region Movement

        public float keyboardMovementSpeed = 5f; //speed with keyboard movement
        public float screenEdgeMovementSpeed = 3f; //speed with screen edge movement
        public float followingSpeed = 5f; //speed when following a target
        public float rotationSpeed = 3f;
        public float panningSpeed = 10f;
        public float mouseRotationSpeed = 10f;

        #endregion

        #region Height

        public float heightDampening = 5f;
        public LayerMask groundMask = -1; //layermask of ground or other objects that affect height
        public float maxRaycastDistance = 100f;
        [Tooltip("Animation curve sampled between 0 and 1, outputting the vertical rotation of the camera at each height")]
        public AnimationCurve angleByRelativeHeight;

        public float maxHeight = 10f;
        public float minHeight = 15f;
        public float keyboardZoomingSensitivity = 2f;
        public float scrollWheelZoomingSensitivity = 25f;

        private float zoomPos = .5f; //value in range (0, 1) used as t in Matf.Lerp

        #endregion

        #region MapLimits

        public CameraLimit limitMap = CameraLimit.RECTANGLE;
        public float limitX = 50f; //x limit of map
        public float limitY = 50f; //z limit of map
        public float limitRadius = 50f;

        #endregion

        #region Targeting

        public Transform targetFollow; //target to follow
        public Vector3 targetOffset;

        /// <summary>
        /// are we following target
        /// </summary>
        public bool FollowingTarget
        {
            get
            {
                return targetFollow != null;
            }
        }

        #endregion

        #region Input

        public bool useScreenEdgeInput = true;
        public float screenEdgeBorder = 25f;

        public bool useKeyboardInput = true;
        public string horizontalAxis = "Horizontal";
        public string verticalAxis = "Vertical";

        public bool usePanning = true;
        public KeyCode panningKey = KeyCode.Mouse2;

        public bool useKeyboardZooming = true;
        public KeyCode zoomInKey = KeyCode.E;
        public KeyCode zoomOutKey = KeyCode.Q;

        public bool useScrollwheelZooming = true;
        public string zoomingAxis = "Mouse ScrollWheel";

        public bool useKeyboardRotation = true;
        public KeyCode rotateRightKey = KeyCode.X;
        public KeyCode rotateLeftKey = KeyCode.Z;

        public bool useMouseRotation = true;
        public KeyCode mouseRotationKey = KeyCode.Mouse1;

        private Vector2 KeyboardInput
        {
            get { return useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)) : Vector2.zero; }
        }

        private Vector2 MouseInput
        {
            get { return Input.mousePosition; }
        }

        private float ScrollWheel
        {
            get { return Input.GetAxis(zoomingAxis); }
        }

        private Vector2 MouseAxis
        {
            get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
        }

        private int ZoomDirection
        {
            get
            {
                bool zoomIn = Input.GetKey(zoomInKey);
                bool zoomOut = Input.GetKey(zoomOutKey);
                if (zoomIn && zoomOut)
                    return 0;
                else if (!zoomIn && zoomOut)
                    return 1;
                else if (zoomIn && !zoomOut)
                    return -1;
                else
                    return 0;
            }
        }

        private int RotationKeyDirection
        {
            get
            {
                bool rotateRight = Input.GetKey(rotateRightKey);
                bool rotateLeft = Input.GetKey(rotateLeftKey);
                if (rotateLeft && rotateRight)
                    return 0;
                else if (rotateLeft && !rotateRight)
                    return -1;
                else if (!rotateLeft && rotateRight)
                    return 1;
                else
                    return 0;
            }
        }

        #endregion

        #region Unity_Methods

        private void Start()
        {
            m_Transform = transform;
        }

        private void Update()
        {
            if (!useFixedUpdate)
                CameraUpdate();
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
                CameraUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            var center = new Vector3(0, (maxHeight + minHeight) / 2f, 0);
            center += transform.parent?.position ?? Vector3.zero;
            switch (limitMap)
            {
                case CameraLimit.RECTANGLE:
                    Gizmos.color = new Color(.5f, .5f, .5f, .3f);
                    Gizmos.DrawCube(
                        center,
                        new Vector3(limitX * 2, maxHeight - minHeight, limitY * 2));
                    break;
                case CameraLimit.CIRCLE:
                    Gizmos.color = new Color(.5f, .5f, .5f, .3f);
                    var cylinder = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                    Gizmos.DrawMesh(
                        cylinder,
                        center,
                        Quaternion.identity,
                        new Vector3(limitRadius, (maxHeight - minHeight) / 2f, limitRadius));
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region RTSCamera_Methods

        /// <summary>
        /// update camera movement and rotation
        /// </summary>
        private void CameraUpdate()
        {
            if (FollowingTarget)
                FollowTarget();
            else
                Move();

            HeightCalculation();
            Rotation();
            LimitPosition();
        }

        /// <summary>
        /// move camera with keyboard or with screen edge
        /// </summary>
        private void Move()
        {
            if (useKeyboardInput)
            {
                Vector3 desiredMove = new Vector3(KeyboardInput.x, 0, KeyboardInput.y);


                desiredMove *= keyboardMovementSpeed;
                desiredMove *= Time.unscaledDeltaTime;
                desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
                desiredMove = m_Transform.InverseTransformDirection(desiredMove);

                m_Transform.Translate(desiredMove, Space.Self);
            }

            if (useScreenEdgeInput)
            {
                Vector3 desiredMove = new Vector3();

                Rect leftRect = new Rect(0, 0, screenEdgeBorder, Screen.height);
                Rect rightRect = new Rect(Screen.width - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
                Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
                Rect downRect = new Rect(0, 0, Screen.width, screenEdgeBorder);

                desiredMove.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
                desiredMove.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

                desiredMove *= screenEdgeMovementSpeed;
                desiredMove *= Time.unscaledDeltaTime;
                desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
                desiredMove = m_Transform.InverseTransformDirection(desiredMove);

                m_Transform.Translate(desiredMove, Space.Self);
            }

            if (usePanning && Input.GetKey(panningKey) && MouseAxis != Vector2.zero)
            {
                Vector3 desiredMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);

                desiredMove *= panningSpeed;
                desiredMove *= Time.unscaledDeltaTime;
                desiredMove = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f)) * desiredMove;
                desiredMove = m_Transform.InverseTransformDirection(desiredMove);

                m_Transform.Translate(desiredMove, Space.Self);
            }
        }

        /// <summary>
        /// calcualte height
        /// </summary>
        private void HeightCalculation()
        {
            float distanceToGround = DistanceToGround();
            if (useScrollwheelZooming)
                zoomPos += ScrollWheel * Time.unscaledDeltaTime * scrollWheelZoomingSensitivity;
            if (useKeyboardZooming)
                zoomPos += ZoomDirection * Time.unscaledDeltaTime * keyboardZoomingSensitivity;

            zoomPos = Mathf.Clamp01(zoomPos);

            float targetHeight = Mathf.Lerp(minHeight, maxHeight, zoomPos);
            float difference = 0;

            if (distanceToGround != targetHeight)
                difference = targetHeight - distanceToGround;

            m_Transform.position = Vector3.Lerp(m_Transform.position,
                new Vector3(m_Transform.position.x, targetHeight + difference, m_Transform.position.z), Time.unscaledDeltaTime * heightDampening);


            var targetAngle = angleByRelativeHeight.Evaluate(zoomPos);
            var currentRotation = m_Transform.eulerAngles;
            currentRotation.x = Mathf.LerpAngle(currentRotation.x, targetAngle, Time.unscaledDeltaTime * heightDampening);
            m_Transform.eulerAngles = currentRotation;
        }

        /// <summary>
        /// rotate camera
        /// </summary>
        private void Rotation()
        {
            var netRotationInput = 0f;
            if (useKeyboardRotation)
                netRotationInput += RotationKeyDirection * Time.unscaledDeltaTime * rotationSpeed;

            if (useMouseRotation && Input.GetKey(mouseRotationKey))
                netRotationInput += -MouseAxis.x * Time.unscaledDeltaTime * mouseRotationSpeed;

            if (Mathf.Abs(netRotationInput) < 1e-5)
            {
                return;
            }
            var cam = GetComponent<Camera>();
            var centerLookRay = cam.ScreenPointToRay(new Vector3(cam.pixelWidth / 2f, cam.pixelHeight / 2f));
            var rotationAnchor = transform.position;
            Debug.DrawRay(centerLookRay.origin, centerLookRay.direction);
            if (Physics.Raycast(centerLookRay, out var hit, maxRaycastDistance, groundMask))
            {
                rotationAnchor = hit.point;
            }

            transform.RotateAround(rotationAnchor, Vector3.up, netRotationInput);
        }

        /// <summary>
        /// follow targetif target != null
        /// </summary>
        private void FollowTarget()
        {
            Vector3 targetPos = new Vector3(targetFollow.position.x, m_Transform.position.y, targetFollow.position.z) + targetOffset;
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, targetPos, Time.unscaledDeltaTime * followingSpeed);
        }

        /// <summary>
        /// limit camera position
        /// </summary>
        private void LimitPosition()
        {
            switch (limitMap)
            {
                case CameraLimit.RECTANGLE:
                    m_Transform.localPosition = new Vector3(
                        Mathf.Clamp(m_Transform.localPosition.x, -limitX, limitX),
                        m_Transform.localPosition.y,
                        Mathf.Clamp(m_Transform.localPosition.z, -limitY, limitY));
                    break;
                case CameraLimit.CIRCLE:
                    var currentPos = new Vector2(transform.localPosition.x, transform.localPosition.z);
                    var currentDistance = currentPos.magnitude;
                    if (currentDistance <= limitRadius) break;

                    var nextPos = currentPos.normalized * limitRadius;
                    m_Transform.localPosition = new Vector3(nextPos.x, m_Transform.localPosition.y, nextPos.y);

                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// set the target
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Transform target)
        {
            targetFollow = target;
        }

        /// <summary>
        /// reset the target (target is set to null)
        /// </summary>
        public void ResetTarget()
        {
            targetFollow = null;
        }

        /// <summary>
        /// calculate distance to ground
        /// </summary>
        /// <returns></returns>
        private float DistanceToGround()
        {
            Ray ray = new Ray(m_Transform.position, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxRaycastDistance, groundMask))
                return (hit.point - m_Transform.position).magnitude;

            return m_Transform.position.y;
        }

        #endregion
    }

    public enum CameraLimit
    {
        NONE,
        RECTANGLE,
        CIRCLE
    }
}