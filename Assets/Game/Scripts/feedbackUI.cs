using System.Collections;
using UnityEngine;

public class feedbackUI : MonoBehaviour
{

    public RectTransform feedbackInvestOK;
    public RectTransform feedbackInvestNO;
    public float feedbackTopPos = 350f;
    public float feedbackDownPos = 0f;
    public float feedbackAnimationDuration = 1f;
    public AnimationCurve feedbackAnimationCurve;

    private bool isInvestOK = false;
    private bool show = false;

    void Start()
    {
        // initialize feedback position at top position
        feedbackInvestOK.anchoredPosition = new Vector2(feedbackInvestOK.anchoredPosition.x, feedbackTopPos);
        feedbackInvestNO.anchoredPosition = new Vector2(feedbackInvestNO.anchoredPosition.x, feedbackTopPos);
    }


    public void ToggleFeedback(bool isCenter, bool _isInvestOK, bool _show)
    {
        if(isInvestOK == _isInvestOK && show == _show)
            return; 
        isInvestOK = _isInvestOK;
        show = _show;
        RectTransform feedback = _isInvestOK ? feedbackInvestOK : feedbackInvestNO;
        StartCoroutine(AnimateFeedback(isCenter, _isInvestOK, _show));
    }

    IEnumerator AnimateFeedback(bool isCenter, bool isInvestOK, bool show)
    {
        if(!isCenter) {
            feedbackInvestOK.gameObject.SetActive(isInvestOK);
            feedbackInvestNO.gameObject.SetActive(!isInvestOK);
        }

        RectTransform feedback = isInvestOK ? feedbackInvestOK : feedbackInvestNO;

        Debug.Log("Animate" + feedback.name + " show"+show);

        float t = 0;
        Vector2 startPos = feedback.anchoredPosition;
        Vector2 endPos = show ? new Vector2(feedback.anchoredPosition.x, feedbackDownPos) : new Vector2(feedback.anchoredPosition.x, feedbackTopPos);

        while (t < feedbackAnimationDuration)
        {
            t += Time.deltaTime;
            float factor = feedbackAnimationCurve.Evaluate(t / feedbackAnimationDuration);
            feedbackInvestOK.anchoredPosition = Vector2.Lerp(startPos, endPos, factor);
            feedbackInvestNO.anchoredPosition = Vector2.Lerp(startPos, endPos, factor);
            yield return null;
        }
        feedbackInvestOK.anchoredPosition = endPos;
        feedbackInvestNO.anchoredPosition = endPos;

        if(!show)
            feedback.gameObject.SetActive(false);
    }

}
