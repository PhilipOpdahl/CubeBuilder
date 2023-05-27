using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubePlacer : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject grid;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == grid.transform)
                {
                    Vector3 position = hit.point;
                    position.x = Mathf.Round(position.x);
                    position.y = Mathf.Round(position.y);
                    position.z = Mathf.Round(position.z);

                    Instantiate(cubePrefab, position, Quaternion.identity);
                }
            }
        }
    }
}
