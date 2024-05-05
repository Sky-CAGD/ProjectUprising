using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float speed = 8f;
    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        Vector3 input = InputValues(out int yRotation).normalized;
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView + input.y * 2, 30, 110);
        transform.parent.Translate(input.Flat() * speed * Time.deltaTime);
        transform.parent.Rotate(Vector3.up * yRotation * Time.deltaTime * speed * 4);
    }

    private Vector3 InputValues(out int y)
    {
        //Move and zoom
        Vector3 inputValues = new Vector3();
        inputValues.x = Input.GetAxis("Horizontal");
        inputValues.z = Input.GetAxis("Vertical");
        inputValues.y = -Input.GetAxis("Mouse ScrollWheel");

        //Rotation
        y = 0;
        if (Input.GetKey(KeyCode.Q))
            y = -1;
        else if (Input.GetKey(KeyCode.E))
            y = 1;

        return inputValues;
    }
}
