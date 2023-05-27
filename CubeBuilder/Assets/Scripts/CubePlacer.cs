using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CubePlacer : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject grid;
    public Camera camera;
    public float placeRate = 0.1f;
    public Color currentColor = Color.white;

    private float nextPlaceTime = 0f;
    private Vector3 lastMousePosition;
    private Vector3 lastCameraPosition;

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 currentCameraPosition = camera.transform.position;

        if (Input.GetMouseButton(0) && (Input.GetMouseButtonDown(0) || currentMousePosition != lastMousePosition || currentCameraPosition != lastCameraPosition) && Time.time >= nextPlaceTime)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(currentMousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 position;

                if (hit.transform != grid.transform)
                {
                    position = hit.transform.position;

                    Vector3 normal = hit.normal;
                    normal.x = Mathf.Round(normal.x);
                    normal.y = Mathf.Round(normal.y);
                    normal.z = Mathf.Round(normal.z);

                    position += normal * 0.1f; 
                }
                else 
                {
                    position = hit.point; 
                    position.x = Mathf.Round(position.x * 10) / 10;
                    position.y = Mathf.Round(position.y * 10) / 10;
                    position.z = Mathf.Round(position.z * 10) / 10;
                }

                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                cube.GetComponent<Renderer>().material.color = currentColor;

                nextPlaceTime = Time.time + placeRate;
            }
        }

        if (Input.GetMouseButton(1) && (Input.GetMouseButtonDown(1) || currentMousePosition != lastMousePosition || currentCameraPosition != lastCameraPosition) && Time.time >= nextPlaceTime)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(currentMousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Cube"))
                {
                    Destroy(hit.transform.gameObject);
                    nextPlaceTime = Time.time + placeRate;
                }
            }
        }

        lastMousePosition = currentMousePosition;
        lastCameraPosition = currentCameraPosition;
    }

    public void ChangeColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "black":
                currentColor = Color.black;
                break;
            case "white":
                currentColor = Color.white;
                break;
            case "grey":
                currentColor = Color.grey;
                break;
            case "lightgrey":
                currentColor = new Color(0.75f, 0.75f, 0.75f, 1f); // RGB(192, 192, 192)
                break;
            case "darkred":
                currentColor = new Color(0.5f, 0, 0, 1f); // RGB(128, 0, 0)
                break;
            case "red":
                currentColor = Color.red;
                break;
            case "orange":
                currentColor = new Color(1f, 0.5f, 0, 1f); // RGB(255, 127, 0)
                break;
            case "yellow":
                currentColor = Color.yellow;
                break;
            case "green":
                currentColor = Color.green;
                break;
            case "darkgreen":
                currentColor = new Color(0, 0.5f, 0, 1f); // RGB(0, 128, 0)
                break;
            case "lightgreen":
                currentColor = new Color(0.5f, 1f, 0.5f, 1f); // RGB(128, 255, 128)
                break;
            case "blue":
                currentColor = Color.blue;
                break;
            case "darkblue":
                currentColor = new Color(0, 0, 0.5f, 1f); // RGB(0, 0, 128)
                break;
            case "lightblue":
                currentColor = new Color(0.5f, 0.5f, 1f, 1f); // RGB(128, 128, 255)
                break;
            case "indigo":
                currentColor = new Color(0.5f, 0, 1f, 1f); // RGB(128, 0, 255)
                break;
            case "purple":
                currentColor = new Color(0.5f, 0, 0.5f, 1f); // RGB(128, 0, 128)
                break;
            case "lavender":
                currentColor = new Color(0.7f, 0.5f, 0.7f, 1f); // RGB(179, 128, 179)
                break;
            default:
                currentColor = Color.white;
                break;
        }
    }
}
