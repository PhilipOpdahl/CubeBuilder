using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelDataManager
{
    public static LevelData SaveCurrentLevel(List<GameObject> cubes)
    {
        LevelData newLevelData = ScriptableObject.CreateInstance<LevelData>();
        newLevelData.relativePositions = new List<Vector3>();
        newLevelData.colors = new List<Color>();

        foreach (GameObject cube in cubes)
        {
            newLevelData.relativePositions.Add(cube.transform.position);
            newLevelData.colors.Add(cube.GetComponent<Renderer>().material.color);
        }

        // Optionally save newLevelData to a file or return it for runtime use
        return newLevelData;
    }
}
