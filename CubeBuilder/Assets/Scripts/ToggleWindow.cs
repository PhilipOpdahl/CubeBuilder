using UnityEngine;

public class ToggleWindow : MonoBehaviour
{
    public Animator UIWindowAnimator;
    private bool isMinimized = false;

    public void ToggleMinimizeMaximize()
    {
        isMinimized = !isMinimized;
        UIWindowAnimator.SetBool("IsMinimized", isMinimized);
    }
}
