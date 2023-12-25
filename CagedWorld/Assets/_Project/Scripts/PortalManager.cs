using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PortalManager : MonoBehaviour
{
    public bool IsClicked { get; private set; } = false;
    private Vector3 touchPosition;
    private TouchPhase touchPhase;
    [SerializeField] private RenderObjects renderObject;
    [Header("Debug Settings")]
    [SerializeField] private TMP_Text debugInfo;
    [Header("AR Settings")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private float planeMinSize = 3f;
    [SerializeField] private Material disabledPlaneMat;
    private ARPlane chosenPlane;
    private Renderer lensRenderer;
    [Header("Portal Settings")]
    [SerializeField] private GameObject portalPrefab;
    private GameObject portal;
    public bool IsPortalSpawn { get; private set; } = false;
    public bool IsInsidePortal { get; private set; } = false;

    #region Singleton
    public static PortalManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Application.targetFrameRate = 30;
        this.lensRenderer = Camera.main.transform.Find("Lens").GetComponent<Renderer>();
    }
    #endregion

    private void Start()
    {
        this.debugInfo.text = $"Click on an area of minimum {this.planeMinSize}x{this.planeMinSize}";
        this.renderObject.SetActive(false);
    }

    private void Update()
    {
        HandleInput();

        List<ARRaycastHit> hitResults = new();
        if (!IsPortalSpawn)
        {
            if (IsClicked && this.touchPhase == TouchPhase.Began && this.raycastManager.Raycast(this.touchPosition, hitResults, TrackableType.PlaneWithinPolygon))
            {
                Vector3 spawnPos = hitResults[0].pose.position;
                this.chosenPlane = this.planeManager.GetPlane(hitResults[0].trackableId);
                float planeX = this.chosenPlane.size.x;
                float planeY = this.chosenPlane.size.y;
                if (planeX > this.planeMinSize && planeY > this.planeMinSize)
                {
                    IsPortalSpawn = true;
                    this.portal = Instantiate(this.portalPrefab, spawnPos, Quaternion.identity);
                    DisablePlanes();
                }
                else
                {
                    this.debugInfo.text = $"The area is too small ({planeX}|{planeY})";
                }
            }
        }
        else
        {
            this.debugInfo.text = $"Pass through the portal ({IsInsidePortal})";
        }
    }

    private void DisablePlanes()
    {
        foreach (ARPlane plane in this.planeManager.trackables)
        {
            DisablePlane(plane, this.chosenPlane.trackableId == plane.trackableId);
        }
        this.planeManager.enabled = false;
    }

    private void DisablePlane(ARPlane plane, bool isInvisible)
    {
        if (isInvisible)
        {
            plane.GetComponent<Renderer>().material = this.disabledPlaneMat;
        }
        else
        {
            plane.gameObject.SetActive(false);
        }
    }

    private void HandleInput()
    {
        IsClicked = false;
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            this.touchPosition = Input.mousePosition;
            this.touchPhase = TouchPhase.Began;
            IsClicked = true;
        }
#else
        if (Input.touchCount > 0)
        {
            this.touchPosition = Input.GetTouch(0).position;
            this.touchPhase = Input.GetTouch(0).phase;
            IsClicked = true;
        }
#endif
    }

    private bool enteredInPortalOrientation;

    public void OnLensTriggeringPortal(Transform lens, Collider portal, bool onExit)
    {
        if (IsInsidePortal)
        {
            if (!onExit)
            {
                this.enteredInPortalOrientation = IsTowardPortalOrientation(portal.transform, portal.bounds.center - lens.transform.position);
                SetLensActive(true);
            }
            else
            {
                if (enteredInPortalOrientation == IsTowardPortalOrientation(portal.transform, lens.transform.position - portal.bounds.center))
                {
                    IsInsidePortal = false;
                    SetBackPortalActive(false);
                    SetLensActive(false);
                }
                
            }
        }
        else
        {
            if (!onExit)
            {
                this.enteredInPortalOrientation = IsTowardPortalOrientation(portal.transform, portal.bounds.center - lens.transform.position);
                SetLensActive(true);
            }
            else
            {
                if (enteredInPortalOrientation == IsTowardPortalOrientation(portal.transform, lens.transform.position - portal.bounds.center))
                {
                    IsInsidePortal = true;
                    SetBackPortalActive(true);
                }
            }
        }



/*        if (onExit)
        {
            IsInsidePortal = IsTowardPortalOrientation(portal.transform, lens.transform.position - portal.bounds.center);
        }
        else
        {
            IsInsidePortal = IsTowardPortalOrientation(portal.transform, portal.bounds.center - lens.transform.position);
        }
        SetLensActive(IsInsidePortal);
        if (onExit)
        {
            SetBackPortalActive(IsInsidePortal);
        }*/
    }

    private void SetLensActive(bool isActive)
    {
        this.lensRenderer.enabled = isActive;
    }

    private void SetBackPortalActive(bool isActive)
    {
        this.renderObject.SetActive(isActive);
    }

    private bool IsTowardPortalOrientation(Transform portal, Vector3 direction)
    {
        direction.y = 0f;
        direction = direction.normalized;
        return Vector3.Angle(portal.forward, direction) < 90f;
    }
}
