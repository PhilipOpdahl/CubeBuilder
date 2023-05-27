using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10.0f;
    public float rotateSpeed = 1.0f;
    public float scrollSpeed = 10.0f;

    private Vector3 lastMousePos;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        movement = transform.TransformDirection(movement);
        movement.y = 0f;
        transform.position += movement * moveSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(2)) 
        {
            lastMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            transform.Rotate(Vector3.up, delta.x * rotateSpeed, Space.World);

            Vector3 pitchAxis = transform.right;
            transform.Rotate(pitchAxis, -delta.y * rotateSpeed, Space.World);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 scrollMovement = new Vector3(0, -scroll * scrollSpeed, 0);
        transform.position += scrollMovement;
    }
}

