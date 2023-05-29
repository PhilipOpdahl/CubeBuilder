using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class CubePlacer : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject grid;
    public Camera mainCamera;
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

    public float rotationStep = 90f;
    private bool isSelecting = false;
    private Vector3 mousePosition1;
    private List<GameObject> selectedCubes = new List<GameObject>();

    private Dictionary<GameObject, List<GameObject>> cubeConnections = new Dictionary<GameObject, List<GameObject>>();

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 currentCameraPosition = mainCamera.transform.position;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddLayerToSelectedCubes();
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                isSelecting = true;
                mousePosition1 = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                SelectCubesInRect();
                isSelecting = false;
            }
        }
        else
        {
            if (isSelecting)
            {
                isSelecting = false;
                ClearSelection();
            }

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

                    cubeConnections[cube] = new List<GameObject>();
                    float connectionThreshold = 1.1f;
                    foreach (GameObject otherCube in cubes)
                    {
                        if (cube == otherCube || otherCube.tag != "Cube") continue;

                        if (Vector3.Distance(cube.transform.position, otherCube.transform.position) < connectionThreshold)
                        {
                            FixedJoint joint = cube.AddComponent<FixedJoint>();
                            joint.connectedBody = otherCube.GetComponent<Rigidbody>();
                            cubeConnections[cube].Add(otherCube);
                            if (!cubeConnections.ContainsKey(otherCube))
                            {
                                cubeConnections[otherCube] = new List<GameObject>();
                            }
                            cubeConnections[otherCube].Add(cube);
                        }
                    }
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

            if (isStructureReadyToPlace && Input.GetKeyDown(KeyCode.R))
            {
                RotateStructure();
            }
        }
    }

    void OnGUI()
    {
        if (isSelecting) 
        {
            var rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
            Utils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            Utils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    public void SelectCubesInRect()
    {
        if (!isSelecting) return;

        Rect rect = Utils.GetScreenRect(mousePosition1, Input.mousePosition);
        selectedCubes.Clear();

        foreach (GameObject cube in cubes) 
        {
            if (cube == null) continue;
            
            if (rect.Contains(Camera.main.WorldToScreenPoint(cube.transform.position))) 
            {
                selectedCubes.Add(cube);
            }
        }
    }

    public void ChangeSelectedCubeColors(Color newColor)
    {
        foreach (GameObject cube in selectedCubes)
        {
            if (cube != null)
            {
                cube.GetComponent<Renderer>().material.color = newColor;
            }
        }
    }

    public void PlaceCube(Vector3 location)
    {
        GameObject cube = Instantiate(cubePrefab, location, Quaternion.identity);
        cubes.Add(cube);
        cubeConnections[cube] = new List<GameObject>();

        foreach (GameObject otherCube in cubes)
        {
            if (cube == otherCube) continue;

            float connectionThreshold = 1.1f;

            if (Vector3.Distance(cube.transform.position, otherCube.transform.position) < connectionThreshold)
            {
                FixedJoint joint = cube.AddComponent<FixedJoint>();
                joint.connectedBody = otherCube.GetComponent<Rigidbody>();

                cubeConnections[cube].Add(otherCube);
                if (!cubeConnections.ContainsKey(otherCube))
                {
                    cubeConnections[otherCube] = new List<GameObject>();
                }
                cubeConnections[otherCube].Add(cube);
            }
        }
    }

    void ActivateGravity()
    {
        foreach (var cube in cubes)
        {
            if(cube != null)
            {
                var rigidbody = cube.GetComponent<Rigidbody>();
                if(rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                }
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

        if (selectedCubes.Count > 0)
        {
            ChangeSelectedCubeColors(currentColor);
        }
    }
    
    public void SaveCubes()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/cubes.fun";
        FileStream stream = new FileStream(path, FileMode.Create);

        List<CubeData> data = new List<CubeData>();
        
        List<GameObject> cubesToSave = (selectedCubes.Count > 0) ? selectedCubes : cubes;

        foreach (GameObject cube in cubesToSave)
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

        foreach (GameObject cube in loadedCubes)
        {
            cube.GetComponent<Collider>().enabled = false;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 position = hit.point;
            position.x = Mathf.Round(position.x * 10) / 10;
            position.y = Mathf.Round(position.y * 10) / 10;
            position.z = Mathf.Round(position.z * 10) / 10;

            GameObject lowestCube = loadedCubes.OrderBy(cube => cube.transform.position.y).First();
            Vector3 initialPosition = lowestCube.transform.position;
            foreach (GameObject cube in loadedCubes)
            {
                Vector3 relativePosition = cube.transform.position - initialPosition;
                cube.transform.position = position + relativePosition;
                cube.SetActive(true);
            }
        }

        foreach (GameObject cube in loadedCubes)
        {
            cube.GetComponent<Collider>().enabled = true;
        }
    }

    public void PlaceStructure()
    {
        if (!isStructureReadyToPlace || loadedCubes.Count == 0) return;

        foreach (GameObject cube in loadedCubes)
        {
            cube.GetComponent<Collider>().enabled = false;
        }

        foreach (GameObject cube in loadedCubes)
        {
            cubes.Add(cube);
        }

        foreach (GameObject cube in loadedCubes)
        {
            cube.GetComponent<Collider>().enabled = true;
        }

        loadedCubes.Clear();
        isStructureReadyToPlace = false;
    }

    public void RotateStructure()
    {
        if (!isStructureReadyToPlace || loadedCubes.Count == 0) return;

        GameObject lowestCube = loadedCubes.OrderBy(cube => cube.transform.position.y).First();
        Vector3 rotationPoint = lowestCube.transform.position;

        foreach (GameObject cube in loadedCubes)
        {
            cube.transform.RotateAround(rotationPoint, Vector3.up, rotationStep);
        }
    }

    void AddLayerToSelectedCubes()
    {
        if (selectedCubes.Count == 0)
        return;

        List<GameObject> newCubes = new List<GameObject>();

        float highestY = selectedCubes[0].transform.position.y;
        foreach (GameObject cube in selectedCubes)
        {
            if (cube.transform.position.y > highestY)
            {
                highestY = cube.transform.position.y;
            }
        }

        foreach (GameObject cube in selectedCubes)
        {
            if (cube.transform.position.y == highestY)
            {
                float cubeHeight = cube.transform.localScale.y;
                Vector3 newPosition = cube.transform.position + new Vector3(0, cubeHeight, 0);
                GameObject newCube = Instantiate(cubePrefab, newPosition, Quaternion.identity);
                newCube.GetComponent<Renderer>().material.color = cube.GetComponent<Renderer>().material.color;
                newCube.transform.SetParent(structureParent.transform);
                newCube.GetComponent<Rigidbody>().mass = 10.0f;
                placedCubes.Add(newCube.GetComponent<Rigidbody>());
                cubes.Add(newCube);
                newCubes.Add(newCube);
            }
        }

        selectedCubes = newCubes;
    }

    void ClearSelection()
    {
        foreach (GameObject cube in selectedCubes)
        {
            var cubeRenderer = cube.GetComponent<Renderer>();
            cubeRenderer.material.color = Color.white;
        }
        selectedCubes.Clear();
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

public static class Utils
{
    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
}

public class Structure : MonoBehaviour
{
    public List<GameObject> cubes;

    void Start()
    {
        for (int i = 0; i < cubes.Count - 1; i++)
        {
            FixedJoint joint = cubes[i].AddComponent<FixedJoint>();
            joint.connectedBody = cubes[i + 1].GetComponent<Rigidbody>();
        }
    }
}
