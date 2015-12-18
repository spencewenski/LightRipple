using UnityEngine;
using System.Collections;

public class Utility : MonoBehaviour {

	public static float updateTimeRemaining(float currentTimeRemaining) {
        if (currentTimeRemaining < Time.deltaTime) {
            return 0f;
        }
        return currentTimeRemaining - Time.deltaTime;
    }

    public static void hideCursor(bool hidden)
    {
        Cursor.visible = !hidden;
        if (hidden)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

