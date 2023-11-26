using System.Collections;
using UnityEngine;

public class CardClose : MonoBehaviour
{
    public void Deal(Vector3 pos)
    {
        StartCoroutine(DealCard(pos));
    }

    IEnumerator DealCard(Vector3 pos)
    {
        pos.y = transform.position.y + .1f;

        iTween.MoveTo(gameObject, iTween.Hash("position", pos, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));
        iTween.RotateTo(gameObject, iTween.Hash("y", Random.Range(700, 900), "time", .3f));
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * .5f, "time", .3f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSeconds(.3f);

        gameObject.SetActive(false);
    }

    public void Pick(Vector3 pos)
    {
        StartCoroutine(PickCard(pos));
    }

    IEnumerator PickCard(Vector3 pos)
    {
        pos.y = transform.position.y + .1f;

        iTween.MoveTo(gameObject, iTween.Hash("position", pos, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));
        iTween.RotateTo(gameObject, iTween.Hash("y", 0, "time", .2f));
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * .5f, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSeconds(.2f);

        gameObject.SetActive(false);
    }

    public void FirstPick()
    {
        StartCoroutine(FirstPickGroundCard());
    }

    IEnumerator FirstPickGroundCard()
    {

        iTween.MoveTo(gameObject, iTween.Hash("position", new Vector3(0,0.1f,0.6f), "time", .2f, "easetype", iTween.EaseType.easeOutQuad));
        iTween.RotateTo(gameObject, iTween.Hash("y", 90, "time", .2f));
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * 1.5f, "time", .2f, "easetype", iTween.EaseType.easeOutQuad));

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
    }


}
