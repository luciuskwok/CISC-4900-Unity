using UnityEngine;

public class BodyLabelScript : MonoBehaviour
{
    Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;    
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);

        // transform.rotation = mainCamera.transform.rotation;
    }
}
