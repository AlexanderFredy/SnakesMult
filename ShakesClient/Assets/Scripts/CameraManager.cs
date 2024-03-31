using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public void Init(float OffsetY)
    {
        Transform camera = Camera.main.transform;
        camera.parent = transform;
        camera.localPosition = Vector3.up*OffsetY;
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        if (Camera.main == null) return;
        Transform camera = Camera.main.transform;
        camera.parent = null;
    }
}
