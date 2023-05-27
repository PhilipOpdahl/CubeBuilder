using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CubePlacer : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject grid;
    public float placeRate = 0.1f;
    public Color currentColor = Color.white;

    private float nextPlaceTime = 0f;
    private Vector3 lastMousePosition;

    void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;

        if ((Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && currentMousePosition != lastMousePosition)) && Time.time >= nextPlaceTime)
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

                    position += normal; 
                }
                else 
                {
                    position = hit.point; 
                    position.x = Mathf.Round(position.x);
                    position.y = Mathf.Round(position.y);
                    position.z = Mathf.Round(position.z);
                }

                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                cube.GetComponent<Renderer>().material.color = currentColor;

                nextPlaceTime = Time.time + placeRate;
            }
        }

        lastMousePosition = currentMousePosition;
    }

    public void ChangeColor(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "red":
                currentColor = Color.red;
                break;
            case "blue":
                currentColor = Color.blue;
                break;
            case "green":
                currentColor = Color.green;
                break;
            case "yellow":
                currentColor = Color.yellow;
                break;
            // Add more colors as needed
            default:
                currentColor = Color.white;
                break;
        }
    }
}
