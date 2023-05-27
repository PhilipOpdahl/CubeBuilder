using UnityEngine;
using UnityEngine.UI;

public class GroundMaterialChanger : MonoBehaviour
{
    public Material[] materials;
    public MeshRenderer groundRenderer;

    public void ChangeMaterial(int index)
    {
        if (index >= 0 && index < materials.Length)
        {
            groundRenderer.material = materials[index];
        }
    }
}
