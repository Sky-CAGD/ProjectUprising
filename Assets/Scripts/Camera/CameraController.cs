using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: Controls camera movement
 */

public class CameraController : MonoBehaviour
{
    [Header("Camera Values")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private float fastSpeed;
    [SerializeField] private float moveTime;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float vertMoveSpeed;
    [SerializeField] private Transform followTarget;

    [Header("Edge Scrolling")]
    [SerializeField] private float edgeScrollDist;
    [SerializeField] private float scrollSpeed;

    [Header("Bounding Box")]
    [SerializeField] private Transform boundingBox;
    [SerializeField] private float leftBounds;
    [SerializeField] private float rightBounds;
    [SerializeField] private float topBounds;
    [SerializeField] private float bottomBounds;

    //Camera Position
    private float moveSpeed;
    private Vector3 newPosition;

    //Camera Rotation
    private Quaternion newRotation;

    //Camera Zoom
    private Vector3 newZoom;
    private Vector3 zoomAxis;
    private Transform cameraTransform;

    //Mouse Control
    private Vector3 dragStartPos;
    private Vector3 dragCurrentPos;
    private Vector3 rotateStartPos;
    private Vector3 rotateCurrentPos;

    //Input Actions
    private PlayerInput playerInput;
    private InputAction move;
    private InputAction leftClick;
    private InputAction rightClick;
    private InputAction rotateAxis;
    private InputAction rotatePan;
    private InputAction zoom;
    private InputAction speedUp;
    private InputAction mousePosition;
    private Vector2 mousePos;

    private void Awake()
    {
        //Get references and initialize values
        cameraTransform = Camera.main.transform;
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
        dragStartPos = Vector3.zero;
        rotateStartPos = Vector3.zero;

        //Set up camera looking at rig and proper zoom axis
        cameraTransform = Camera.main.transform;
        cameraTransform.LookAt(transform);
        zoomAxis = cameraTransform.position.normalized;
        zoomAxis.x = 0;

        //Set up player input events
        playerInput = new PlayerInput();

        //Confine Cursor to screen
        Cursor.lockState = CursorLockMode.Confined;

        Vector3 boundsCenter = boundingBox.position;
        leftBounds = boundsCenter.x - (boundingBox.localScale.x / 2);
        rightBounds = boundsCenter.x + (boundingBox.localScale.x / 2);
        topBounds = boundsCenter.z + (boundingBox.localScale.z / 2);
        bottomBounds = boundsCenter.z - (boundingBox.localScale.z / 2);
    }

    /// <summary>
    /// Set up input reading
    /// </summary>
    private void OnEnable()
    {
        move = playerInput.PlayerActions.Move;
        leftClick = playerInput.PlayerActions.LeftClick;
        rightClick = playerInput.PlayerActions.RightClick;
        rotateAxis = playerInput.PlayerActions.RotateAxis;
        rotatePan = playerInput.PlayerActions.RotatePan;
        zoom = playerInput.PlayerActions.Zoom;
        speedUp = playerInput.PlayerActions.SpeedUp;
        mousePosition = playerInput.PlayerActions.MousePosition;

        playerInput.Enable();

        playerInput.PlayerActions.LeftClick.performed += StartEndMousePanning;
        playerInput.PlayerActions.LeftClick.canceled += StartEndMousePanning;
        playerInput.PlayerActions.RotatePan.performed += StartEndMouseRotating;
        playerInput.PlayerActions.RotatePan.canceled += StartEndMouseRotating;

        EventManager.CharacterSelected += CharacterSelected;
        EventManager.CharacterDeselected += CharacterDeselected;
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.PlayerActions.LeftClick.performed -= StartEndMousePanning;
        playerInput.PlayerActions.LeftClick.canceled -= StartEndMousePanning;
        playerInput.PlayerActions.RotatePan.performed -= StartEndMouseRotating;
        playerInput.PlayerActions.RotatePan.canceled -= StartEndMouseRotating;

        EventManager.CharacterSelected -= CharacterSelected;
        EventManager.CharacterDeselected -= CharacterDeselected;
    }

    private void CharacterSelected(Character character)
    {
        followTarget = character.transform;
    }

    private void CharacterDeselected()
    {
        followTarget = null;
    }

    // Update is called once per frame
    private void Update()
    {
        mousePos = mousePosition.ReadValue<Vector2>();

        MouseEdgePanning();
        MouseDragPanning();
        MouseRotating();
        RotateCameraVertically();
        ZoomCamera();
        MoveCamera();
        ApplyNewValues();
    }

    private void ApplyNewValues()
    {
        //Clamp the camera's move position within the bounding box
        newPosition.x = Mathf.Clamp(newPosition.x, leftBounds, rightBounds);
        newPosition.z = Mathf.Clamp(newPosition.z, bottomBounds, topBounds);

        //Smoothly move camera to new positions & rotations
        if (newPosition != Vector3.zero)
            transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * moveTime);

        if (newRotation != Quaternion.identity)
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * moveTime);

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * moveTime);
        cameraTransform.LookAt(transform);
    }

    /// <summary>
    /// Moves the camera to a newPosition, taking user inputs into account
    /// </summary>
    private void MoveCamera()
    {
        //Set move speed based on if speed up (left shift) key is held
        bool isSpeedUpHeld = speedUp.ReadValue<float>() > 0.1f;
        moveSpeed = isSpeedUpHeld ? fastSpeed : baseSpeed;

        //Get the movement input
        Vector2 moveInput = move.ReadValue<Vector2>();
        Vector3 moveDir = (moveInput.x * transform.right) + (moveInput.y * transform.forward);

        //If moving camera, adjust newPosition and stop following any targets
        if (moveDir != Vector3.zero)
        {
            newPosition += (moveDir * moveSpeed);
            followTarget = null;
        }
        //Move to follow target if one is set and camera is not otherwise moving
        else if(followTarget != null)
        {
            newPosition = followTarget.position;
        }
    }

    /// <summary>
    /// Rotates the camera vertically around the rig using user input
    /// </summary>
    private void RotateCameraVertically()
    {
        float rotateInput = rotateAxis.ReadValue<float>();

        if (rotateInput != 0)
            newZoom += new Vector3(0, rotateInput * vertMoveSpeed, 0);
            //newRotation *= Quaternion.Euler(Vector3.up * rotateInput * rotateSpeed);
    }

    /// <summary>
    /// Zooms the camera forward/back along the zoomAxis using user input
    /// </summary>
    private void ZoomCamera()
    {
        float zoomInput = Mathf.Clamp(zoom.ReadValue<float>(), -1, 1);

        if (zoomInput != 0)
            newZoom -= zoomInput * zoomAxis * zoomSpeed;
    }

    /// <summary>
    /// Called for a single frame when left mouse is clicked or released to set the dragStartPos
    /// </summary>
    /// <param name="context"></param>
    private void StartEndMousePanning(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPos = ray.GetPoint(entry);
            }
        }
        else if(context.canceled)
        {
            dragStartPos = Vector3.zero;
        }
    }

    /// <summary>
    /// Continuously reads whether the left mouse is held to pan the camera
    /// </summary>
    private void MouseDragPanning()
    {
        bool isMouseHeld = leftClick.ReadValue<float>() > 0.1f;

        //Continuous left click held to drag camera
        if (isMouseHeld && dragStartPos != Vector3.zero)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(mousePos);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPos = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPos - dragCurrentPos;
            }
        }
    }

    /// <summary>
    /// Handles moving the camera when the mouse pointer reaches the edges of the screen 
    /// </summary>
    private void MouseEdgePanning()
    {
        float rightPanX = Screen.width - edgeScrollDist;
        float leftPanX = edgeScrollDist;
        float topPanY = Screen.height - edgeScrollDist;
        float bottomPanY = edgeScrollDist;

        //Used to get coordinates slightly past edge of screen
        //Mouse will stop tracking if leaving game window
        float beyondEdgeAmt = 5f;

        //Scroll left/right
        if (mousePos.x < leftPanX && mousePos.x > 0 - beyondEdgeAmt)
        {
            newPosition -= (transform.right * scrollSpeed);
            followTarget = null;
        }
        else if(mousePos.x > rightPanX && mousePos.x < Screen.width + beyondEdgeAmt)
        {
            newPosition += (transform.right * scrollSpeed);
            followTarget = null;
        }

        //Scroll up/down
        if (mousePos.y > topPanY && mousePos.y < Screen.height + beyondEdgeAmt)
        {
            newPosition += (transform.forward * scrollSpeed);
            followTarget = null;
        }
        else if (mousePos.y < bottomPanY && mousePos.y > 0 - beyondEdgeAmt)
        {
            newPosition -= (transform.forward * scrollSpeed);
            followTarget = null;
        }
    }

    /// <summary>
    /// Called for a single frame when middle mouse is pressed or released to set the rotateStartPos
    /// </summary>
    /// <param name="context"></param>
    private void StartEndMouseRotating(InputAction.CallbackContext context)
    {
        if (context.performed)
            rotateStartPos = mousePos;
        else if(context.canceled)
            rotateStartPos = Vector3.zero;
    }

    /// <summary>
    /// Continuously reads whether the middle mouse is held to pan the camera
    /// </summary>
    private void MouseRotating()
    {
        bool isRotateHeld = rotatePan.ReadValue<float>() > 0.1f;

        //Continuous middle click held to rotate camera
        if (isRotateHeld && rotateStartPos != Vector3.zero)
        {
            rotateCurrentPos = mousePos;
            Vector3 difference = rotateStartPos - rotateCurrentPos;
            rotateStartPos = rotateCurrentPos;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));           
        }
    }
}
