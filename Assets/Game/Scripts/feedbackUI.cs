using System.Collections;
using UnityEngine;

public class feedbackUI : MonoBehaviour
{

    public RectTransform feedbackInvestOK;
    public CanvasGroup feedbackInvestOKCanvasGroup;
    public RectTransform feedbackInvestNO;
    public CanvasGroup feedbackInvestNOCanvasGroup;
    public float feedbackTopPos = 350f;
    public float feedbackDownPos = 0f;
    public float feedbackAnimationSpeed = 1f;
    private float targetPositionOK;
    private float targetAlphaOK;
    private float targetPositionNO;
    private float targetAlphaNO;

    public void Start(){
        ResetPositions();
    }
    
    void Update() {
        feedbackInvestOK.anchoredPosition = new Vector2(feedbackInvestOK.anchoredPosition.x, Mathf.Lerp(feedbackInvestOK.anchoredPosition.y, targetPositionOK, Time.deltaTime * feedbackAnimationSpeed));
        feedbackInvestNO.anchoredPosition = new Vector2(feedbackInvestNO.anchoredPosition.x, Mathf.Lerp(feedbackInvestNO.anchoredPosition.y, targetPositionNO, Time.deltaTime * feedbackAnimationSpeed));
        feedbackInvestOKCanvasGroup.alpha = Mathf.Lerp(feedbackInvestOKCanvasGroup.alpha, targetAlphaOK, Time.deltaTime * feedbackAnimationSpeed);
        feedbackInvestNOCanvasGroup.alpha = Mathf.Lerp(feedbackInvestNOCanvasGroup.alpha, targetAlphaNO, Time.deltaTime * feedbackAnimationSpeed);
    }

    public void ResetPositions(){
        feedbackInvestOK.gameObject.SetActive(true);
        feedbackInvestNO.gameObject.SetActive(true);
        feedbackInvestOK.anchoredPosition = new Vector2(feedbackInvestOK.anchoredPosition.x, feedbackTopPos);
        feedbackInvestNO.anchoredPosition = new Vector2(feedbackInvestNO.anchoredPosition.x, feedbackTopPos);
        targetPositionOK=feedbackTopPos;
        targetPositionNO=feedbackTopPos;
    }

    public void SetTargetPosition(int targetPOS)
    {
        if(targetPOS==1){
            targetPositionOK=feedbackDownPos;
            targetPositionNO=feedbackTopPos;
            targetAlphaOK=1;
            targetAlphaNO=0;
        }
        else if (targetPOS==-1)
        {
            targetPositionOK=feedbackTopPos;
            targetPositionNO=feedbackDownPos;
            targetAlphaOK=0;
            targetAlphaNO=1;
        }
        else if (targetPOS==0){
            targetPositionOK=feedbackTopPos;
            targetPositionNO=feedbackTopPos;
            targetAlphaOK=0;
            targetAlphaNO=0;
        }
    }

}
