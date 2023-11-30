using UnityEngine;

public class DontDestroyOnLoadController : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
