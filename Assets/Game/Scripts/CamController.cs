using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] Transform camDef;
    [SerializeField] Transform camStart;
    [SerializeField] Transform camGameplay;
    [SerializeField] Transform camTransform;

    private void Awake()
    {
        camTransform.parent = camDef;
        camTransform.localPosition = Vector3.zero;
        camTransform.localEulerAngles = Vector3.zero;
    }

    public void CamStart()
    {
        camTransform.parent = camStart;
        CamTween(3.5f);
    }

    public void CamGameplay()
    {
        camTransform.parent = camGameplay;
        CamTween(3);
    }

    void CamTween(float tTime)
    {
        iTween.MoveTo(camTransform.gameObject, iTween.Hash("position", Vector3.zero, "islocal", true, "time", tTime, "easetype", iTween.EaseType.easeInOutQuad));
        iTween.RotateTo(camTransform.gameObject, iTween.Hash("rotation", Vector3.zero, "islocal", true, "time", tTime, "easetype", iTween.EaseType.easeInOutQuad));
    }
}
