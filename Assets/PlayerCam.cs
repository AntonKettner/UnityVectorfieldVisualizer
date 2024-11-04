using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float sensitivity = 2.0f;
    public float verticalSpeed = 10.0f;

    private float rotationX = 0;

    // Update is called once per frame
    void Update()
    {
        // Mouse movement for looking around
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.Rotate(0, mouseX, 0);
        transform.localEulerAngles = new Vector3(rotationX, transform.localEulerAngles.y, 0);

        // WASD movement
        float horizontal = Input.GetAxis("Horizontal") * moveSpeed;
        float vertical = Input.GetAxis("Vertical") * moveSpeed;
        Vector3 movement = transform.right * horizontal + transform.forward * vertical;

        // Up and down movement with Space and Shift
        if (Input.GetKey(KeyCode.Space))
        {
            movement += Vector3.up * verticalSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            movement += Vector3.down * verticalSpeed;
        }

        // Apply movement
        transform.position += movement * Time.deltaTime;
    }
}