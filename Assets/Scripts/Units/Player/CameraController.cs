using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : SingletonPattern<CameraController>
{
    [Header("Camera Values")]
    public float baseSpeed;
    public float fastSpeed;
    public float moveTime;
    public float rotateSpeed;
    public float zoomSpeed;
    public Transform followTransform;

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

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = transform.GetChild(0);

        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

        zoomAxis = new Vector3(0, -1, 1);
    }

    // Update is called once per frame
    void Update()
    {  
        if (followTransform != null )
            FollowTransform();

        HandleMouseInput();
        RotateCamera();
        ZoomCamera();
        MoveCamera();
    }

    private void FollowTransform()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
        }

        newPosition = followTransform.position;
    }

    private void MoveCamera()
    {
        moveSpeed = Input.GetKey(KeyCode.LeftShift) ? fastSpeed : baseSpeed;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += (transform.forward * moveSpeed);
            followTransform = null;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += -(transform.forward * moveSpeed);
            followTransform = null;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += (transform.right * moveSpeed);
            followTransform = null;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += -(transform.right * moveSpeed);
            followTransform = null;
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * moveTime);
    }

    private void RotateCamera()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotateSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotateSpeed);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * moveTime);
    }

    private void ZoomCamera()
    {
        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomAxis * zoomSpeed;
        }
        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomAxis * zoomSpeed;
        }

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * moveTime);
    }

    private void HandleMouseInput()
    {
        //Convert scroll wheel input to Zoom
        if(Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAxis * zoomSpeed;
        }

        //Start left click and drag to move camera
        if(Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if(plane.Raycast(ray, out entry))
            {
                dragStartPos = ray.GetPoint(entry);
            }
        }

        //Continuous left click held to drag camera
        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPos = ray.GetPoint(entry);

                newPosition = transform.position + dragStartPos - dragCurrentPos;
            }
        }

        //Start middle click to rotate camera
        if(Input.GetMouseButtonDown(2))
        {
            rotateStartPos = Input.mousePosition;
        }

        //Continuous middle click held to rotate camera
        if ( Input.GetMouseButton(2))
        {
            rotateCurrentPos = Input.mousePosition;
            Vector3 difference = rotateStartPos - rotateCurrentPos;
            rotateStartPos = rotateCurrentPos;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }
}
