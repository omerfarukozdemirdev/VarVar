using UnityEngine;

public class FollowWorldObject : MonoBehaviour
{
    [SerializeField] Transform followObject;
    [SerializeField] Vector3 offset;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 wp = mainCamera.WorldToViewportPoint(followObject.position);
        transform.position = mainCamera.ViewportToScreenPoint(wp + offset);
    }
}
