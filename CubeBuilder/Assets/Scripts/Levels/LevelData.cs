using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "CubeBuilder/LevelData", order = 1)]
public class LevelData : ScriptableObject
{
    public List<Vector3> relativePositions;
    public List<Color> colors;
}
