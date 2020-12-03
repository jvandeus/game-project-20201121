using UnityEngine;
using System.Collections;

// [RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{

    private Camera thisCamera;
    public CharacterController2D target;
    private Collider2D thisCollider;
    public float verticalOffset;
    public Vector2 focusAreaSize;
    [Range(0.01f, 1f)]
    public float followStiffness = 0.4f;
    [Range(0.01f, 1f)]
    public float focusShiftDeadzone = 0.5f;
    [Range(0.01f, 1f)]
    public float focusShiftAnchor = 0.4f;
    [Range(0.01f, 4f)]
    public float focusShiftIntensity = 1f;
    public float cameraDistance = 10f;
    [Range(0.0f, 60f)]
    public float updatesPerSecond = 0f;

    // a bool to keep track of whether or not we have to keep updating position
    private bool isSnapped = false;
    // keep a copy of the executing follow script
    private IEnumerator followCoroutine;
    // the main focus object
    private FocusArea focusArea;

    //Paralax Vars, commented out for now because we're not there yet and this was causing errors -BWM
    //public GameObject BGParentObject;
    //public GameObject bulletParentObject;

    // [Range(0, 1)]
    // public float BackgroundSpeed;

    void Awake()
    {
        // make sure the camera is negative.
        cameraDistance = - Mathf.Abs(cameraDistance);
        // initialize the bool checks
        isSnapped = false;
        // get the needed components
        thisCamera = GetComponent<Camera>();
        thisCollider = target.GetComponent<Collider2D>();
        // construct the area that will control our camera movements
        focusArea = new FocusArea(thisCollider.bounds, focusAreaSize, focusShiftDeadzone, focusShiftAnchor, focusShiftIntensity, thisCamera);
        // set up the coroutine to follow the target
        followCoroutine = followTarget();
        // stop any running coroutine
        if (followCoroutine != null) {
            StopCoroutine(followCoroutine);
        }
        // start the coroutine to follow the target
        StartCoroutine(followCoroutine);
    }

    void Start()
    {
        // initialize the camera to the center of the area, at the proper distance away from the 2d plane.
        transform.position = new Vector3(thisCollider.bounds.center.x, thisCollider.bounds.center.y, cameraDistance);
    }

    // coroutine to folow the target. this is to replace update calls, but may need to test efficiency at some point.
    IEnumerator followTarget()
    {
        Vector3 cameraFocusPoint = new Vector3();
        Vector3 positionToFocus = new Vector3();
        Vector3 targetPosition = new Vector3();
        float snapDistance = 0.2f;
        float timer = 0f;
        float tickTime = updatesPerSecond > 0f ? 1/updatesPerSecond : 0f;
        bool isStarted = false;

        while (enabled) {
            // increment the timer
            timer += Time.deltaTime;
            // check if we need to update the target
            if (!isStarted || timer >= tickTime) {
                isStarted = true;
                // recalcualte the position the camera should aim for
                focusArea.updatePosition(thisCollider.bounds);
                // the focus point in the focus area is on the same z plane as the target, so we can calculate distances properly.
                cameraFocusPoint = (focusArea.focusPoint + Vector3.up * verticalOffset);
                // get the ray from the camera's current focus position to the point it should be focused on
                positionToFocus = cameraFocusPoint - transform.position;
                // set the new target position, at the appropriate distance away from the focus
                targetPosition = cameraFocusPoint;
                targetPosition.z = cameraDistance;
                // reset the timer
                timer = 0f;
            }

            // smoothly travel toward target position if we are not already there.
            if (positionToFocus.magnitude > snapDistance) {
                isSnapped = false;
                // Set the Camera to 10 units away from 0 in the z axis, which should be the background layer
                transform.position = Vector3.Lerp(transform.position, targetPosition, followStiffness);
            } else {
                if (isSnapped == false) {
                    isSnapped = true;
                    // snap to the focus point if we are close enough.
                    transform.position = targetPosition;
                }
            }

            //Parallax Scrolling
            // cameraFocusPoint = new Vector3(transform.position.x * BackgroundSpeed, transform.position.y * BackgroundSpeed, 0.0f);
            //BGParentObject.transform.position = cameraFocusPoint;
            //bulletParentObject.transform.position = cameraFocusPoint;
            
            // wait for next frame
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        if (enabled) {
            // show the area the camera can be shifted in
            Gizmos.color = new Color(0, 0, 1, .5f);
            Gizmos.DrawCube(focusArea.center, focusAreaSize);
            // show the deadzone
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawCube(focusArea.center, focusAreaSize*focusShiftDeadzone);
            // show the focus point
            Gizmos.color = new Color(0, 1, 0, .5f);
            Gizmos.DrawCube(focusArea.focusPoint, new Vector2(0.4f, 0.4f));
        }
    }

    struct FocusArea
    {
        public Vector2 center
        {
            get { return outerLimits.center; }
            set {
                outerLimits.center = (Vector3)value;
                innerLimits.center = (Vector3)value;
            }
        }
        public Vector2 size
        {
            get { return outerLimits.size; }
            set {
                outerLimits.size = (Vector3)value;
                innerLimits.size = (Vector3)value*deadzoneFraction;
            }
        }
        public Vector3 focusPoint
        {
            get { return focusCenterPoint; }
            set {
                // do nothing for now
            }
        }
        Camera targetCamera;
        Bounds innerLimits;
        Bounds outerLimits;
        Vector3 focusTriggerPoint;
        Vector3 focusCenterPoint;
        float deadzoneFraction;
        float shiftFraction;
        float shiftIntensity;

        public FocusArea(Bounds targetBounds, Vector2 size, float shiftDeadzone, float shiftAnchor, float shiftIntensityIn, Camera aCamera)
        {
            targetCamera = aCamera;
            deadzoneFraction = shiftDeadzone;
            shiftFraction = shiftAnchor;
            shiftIntensity = shiftIntensityIn;
            focusTriggerPoint = targetBounds.center;
            focusCenterPoint = targetBounds.center;
            Vector3 limitSize = new Vector3(size.x, size.y, 1f);
            innerLimits = new Bounds(targetBounds.center, limitSize*deadzoneFraction);
            outerLimits = new Bounds(targetBounds.center, limitSize);
        }

        private Vector3 calcFocusPoint(Bounds targetBounds)
        {
            // For now we will get the distance from target center to mouse. Later we may want to use target bounds (like innerLimits.ClosestPoint(Input.mousePosition))
            Vector3 mousePosition = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            // for now we have to set z to the same as the target for the 2d plane. Later we should adapt to be usable in 3d.
            mousePosition.z = targetBounds.center.z;
            Vector3 focusPoint = Vector3.Lerp(targetBounds.center, mousePosition, shiftFraction);
            return focusPoint;
        }

        //Velocity is added here, at shiftX. Add the mouse calculations to shiftX?
        public void updatePosition(Bounds targetBounds)
        {
            focusTriggerPoint = calcFocusPoint(targetBounds);

            float intersectDistance = 0f;

            innerLimits.center = targetBounds.center;
            outerLimits.center = targetBounds.center;
            if (innerLimits.Contains(focusTriggerPoint) != true) {
                Vector3 centerToFocusVector = (focusTriggerPoint - targetBounds.center);
                Ray focusToCenterRay = new Ray(focusTriggerPoint, -centerToFocusVector);
                Ray centerToFocusRay = new Ray(targetBounds.center, centerToFocusVector);
                Debug.DrawRay(centerToFocusRay.origin, centerToFocusVector, Color.green, 0);
                if (outerLimits.Contains(focusTriggerPoint) != true) {
                    // we want to limit the focus ray origin to the outer bounds, we set the origin of the ray to the bound intersect point.
                    if (outerLimits.IntersectRay(focusToCenterRay, out intersectDistance)) {
                        focusToCenterRay.origin = focusToCenterRay.GetPoint(intersectDistance);
                    }
                }
                // set the focus point to shift the distance from the edge of the bounds to the focus trigger.
                if (innerLimits.IntersectRay(focusToCenterRay, out intersectDistance)) {
                    focusCenterPoint = Vector3.Lerp(targetBounds.center, centerToFocusRay.GetPoint(intersectDistance * shiftIntensity), 1f);
                }
            } else {
                // while in the inner region, the camera will focus on the player.
                focusCenterPoint = targetBounds.center;
            }
        }
    }

}
