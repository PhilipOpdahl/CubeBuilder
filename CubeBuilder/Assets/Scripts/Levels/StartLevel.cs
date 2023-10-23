using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLevel : MonoBehaviour
{

    [SerializeField] LevelData levelData;
    [SerializeField] GameObject outlinePrefab;
    [SerializeField] Transform outlineParent;

    void Start()
    {
        LoadLevel(levelData);
    }

    void LoadLevel(LevelData levelData)
    {
        Vector3 startPosition = new Vector3(0, 0, 0); // Change as needed
        for (int i = 0; i < levelData.relativePositions.Count; i++)
        {
            Vector3 position = startPosition + levelData.relativePositions[i];
            GameObject outlineCube = Instantiate(outlinePrefab, position, Quaternion.identity);
            outlineCube.GetComponent<Renderer>().material.color = levelData.colors[i];
            outlineCube.transform.SetParent(outlineParent);
        }
    }

}
