using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

    public GameObject structureParent;
    private List<Rigidbody> placedCubes = new List<Rigidbody>();
    private List<GameObject> cubes = new List<GameObject>();
    private List<GameObject> loadedCubes = new List<GameObject>();
    private bool isStructureReadyToPlace = false;
    private bool isStructurePlaced = false;

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 currentCameraPosition = camera.transform.position;

        if (Input.GetMouseButton(0) && !isStructureReadyToPlace && (Input.GetMouseButtonDown(0) || currentMousePosition != lastMousePosition || currentCameraPosition != lastCameraPosition) && Time.time >= nextPlaceTime)
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
                cube.transform.SetParent(structureParent.transform);
                cube.GetComponent<Renderer>().material.color = currentColor;
                cube.GetComponent<Rigidbody>().mass = 10.0f;
                placedCubes.Add(cube.GetComponent<Rigidbody>());
                cubes.Add(cube);

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
                    cubes.Remove(hit.transform.gameObject);
                    Destroy(hit.transform.gameObject);
                    nextPlaceTime = Time.time + placeRate;
                }
            }
        }

        lastMousePosition = currentMousePosition;
        lastCameraPosition = currentCameraPosition;

        if (isStructureReadyToPlace)
        {
            PositionStructure();
            if (Input.GetMouseButtonDown(0))
            {
                PlaceStructure();
            }
        }
    }

    public void ActivateGravity()
    {
        foreach (Rigidbody cube in placedCubes)
        {
            if(cube != null)
            {
                cube.isKinematic = false;
            }
        }
        
        foreach (GameObject cube in cubes)
        {
            if(cube != null)
            {
                cube.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
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
    
    public void SaveCubes()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/cubes.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        List<CubeData> data = new List<CubeData>();
        foreach (GameObject cube in cubes)
        {
            if (cube != null)
            {
                Vector3 position = cube.transform.position;
                Color color = cube.GetComponent<Renderer>().material.color;
                data.Add(new CubeData(position, color));
            }
        }

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("Cubes saved!");
    }

    public void LoadCubes()
    {
        Debug.Log("LoadCubes() called.");
        string path = Application.persistentDataPath + "/cubes.fun";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            List<CubeData> data = formatter.Deserialize(stream) as List<CubeData>;
            stream.Close();

            foreach (CubeData cubeData in data)
            {
                Vector3 position = new Vector3(cubeData.position[0], cubeData.position[1], cubeData.position[2]);
                Color color = new Color(cubeData.color[0], cubeData.color[1], cubeData.color[2], cubeData.color[3]);

                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
                cube.GetComponent<Renderer>().material.color = color;
                cube.SetActive(false);
                loadedCubes.Add(cube);
            }
            
            isStructureReadyToPlace = true;
            isStructurePlaced = false;
        }
    }

    public void UpdateStructurePosition(Vector3 position)
    {
        if (!isStructureReadyToPlace || loadedCubes.Count == 0) return;

        Vector3 initialPosition = loadedCubes[0].transform.position;
        foreach (GameObject cube in loadedCubes)
        {
            Vector3 relativePosition = cube.transform.position - initialPosition;
            cube.transform.position = position + relativePosition;
        }
    }

    public void ConfirmStructurePlacement()
    {
        foreach (GameObject cube in loadedCubes)
        {
            cube.SetActive(true);
            cubes.Add(cube);
        }
        
        loadedCubes.Clear();
        isStructureReadyToPlace = false;
    }

    public void PositionStructure()
    {
        if (!isStructureReadyToPlace || loadedCubes.Count == 0) return;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 position = hit.point;

            position.x = Mathf.Round(position.x * 10) / 10;
            position.y = Mathf.Round(position.y * 10) / 10;
            position.z = Mathf.Round(position.z * 10) / 10;

            Vector3 initialPosition = loadedCubes[0].transform.position;
            foreach (GameObject cube in loadedCubes)
            {
                Vector3 relativePosition = cube.transform.position - initialPosition;
                cube.transform.position = position + relativePosition;
                cube.SetActive(true);
            }
        }
    }

    public void PlaceStructure()
    {
        if (!isStructureReadyToPlace || loadedCubes.Count == 0) return;

        foreach (GameObject cube in loadedCubes)
        {
            cubes.Add(cube);
        }

        loadedCubes.Clear();
        isStructureReadyToPlace = false;
    }
}

[System.Serializable]
public class CubeData
{
    public float[] position;
    public float[] color;

    public CubeData(Vector3 position, Color color)
    {
        this.position = new float[3] { position.x, position.y, position.z };
        this.color = new float[4] { color.r, color.g, color.b, color.a };
    }
}
