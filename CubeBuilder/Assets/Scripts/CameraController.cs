using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 1.0f;
    public float scrollSpeed = 2.5f;

    public float minY = -10.0f;
    public float maxY = 10.0f;
    public float minX = -10.0f;
    public float maxX = 10.0f;
    public float minZ = -10.0f;
    public float maxZ = 10.0f;

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

        Vector3 clampedPosition = transform.position;
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        transform.position = clampedPosition;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3((minX + maxX)/2, (minY + maxY)/2, (minZ + maxZ)/2), new Vector3(maxX - minX, maxY - minY, maxZ - minZ));
    }
}

