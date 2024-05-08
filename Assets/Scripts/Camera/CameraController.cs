using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : SingletonPattern<CameraController>
{
    [Header("Camera Values")]
    public float baseSpeed;
    public float fastSpeed;
    public float moveTime;
    public float rotateSpeed;
    public float zoomSpeed;
    public float vertMoveSpeed;
    public Transform followTarget;

    //Camera Position
    private float moveSpeed;
    private Vector3 newPosition;

    //Camera Rotation
    private Quaternion newRotation;

    //Camera Zoom
    private Vector3 newZoom;
    public Vector3 zoomAxis;
    private Transform cameraTransform;

    //Mouse Control
    private Vector3 dragStartPos;
    private Vector3 dragCurrentPos;
    private Vector3 rotateStartPos;
    private Vector3 rotateCurrentPos;

    //Input Actions
    private PlayerInput playerInput;
    private InputAction move;
    private InputAction click;
    private InputAction rotateAxis;
    private InputAction rotatePan;
    private InputAction zoom;
    private InputAction speedUp;
    private InputAction mousePosition;
    private Vector2 mousePos;

    protected override void Awake()
    {
        base.Awake();

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
    }

    /// <summary>
    /// Set up input reading
    /// </summary>
    private void OnEnable()
    {
        move = playerInput.PlayerActions.Move;
        click = playerInput.PlayerActions.Interact;
        rotateAxis = playerInput.PlayerActions.RotateAxis;
        rotatePan = playerInput.PlayerActions.RotatePan;
        zoom = playerInput.PlayerActions.Zoom;
        speedUp = playerInput.PlayerActions.SpeedUp;
        mousePosition = playerInput.PlayerActions.MousePosition;

        playerInput.Enable();

        playerInput.PlayerActions.Interact.performed += StartEndMousePanning;
        playerInput.PlayerActions.Interact.canceled += StartEndMousePanning;
        playerInput.PlayerActions.RotatePan.performed += StartEndMouseRotating;
        playerInput.PlayerActions.RotatePan.canceled += StartEndMouseRotating;
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.PlayerActions.Interact.performed -= StartEndMousePanning;
        playerInput.PlayerActions.Interact.canceled -= StartEndMousePanning;
        playerInput.PlayerActions.RotatePan.performed -= StartEndMouseRotating;
        playerInput.PlayerActions.RotatePan.canceled -= StartEndMouseRotating;
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = mousePosition.ReadValue<Vector2>();

        MousePanning();
        MouseRotating();
        RotateCameraVertically();
        ZoomCamera();
        MoveCamera();
        ApplyNewValues();
    }

    private void ApplyNewValues()
    {
        //Smoothly move camera to new positions & rotations
        if(newPosition != Vector3.zero)
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
    private void MousePanning()
    {
        bool isMouseHeld = click.ReadValue<float>() > 0.1f;

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
