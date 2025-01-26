using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;

public class Swipeable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform swipeableElement;
    public CanvasGroup swipeableCanvasGroup;
    public RectTransform swipeBackFaceCard;
    public Image swipeBackgroundImage;
    public Color swipeBackgroundColor1;
    public Color swipeBackgroundColor2;
    public feedbackUI feedbackUI;

    
    [Header("Positions de référence")]
    public RectTransform swipeLeftRef;
    public RectTransform swipeMiddleRef;
    public RectTransform swipeRightRef;
    public RectTransform validateLeftRef;
    public RectTransform validateRightRef;

    [Header("Paramètres de transition")]
    public float dragTreshold = 200f;
    public float centerTreshold = 50f;
    public float swipeSpeed = 10f;
    public float validationAnimationDuration = 1f;
    public AnimationCurve validationAnimationCurve;
    public float flipDuration = 0.5f;
    bool isAnimating = false;

    [Header("Audio")]
    public AudioSource audioSourceInvestFeedback;
    public AudioSource audioSourceBuy;
    public AudioSource audioSourceFlip;
    public AudioClip[] buySounds;
    public AudioClip[] flipSounds;

    public InvestFeedbackSO[] investFeedbacks;

    private RectTransform rectTransform;
    private Vector2 startPointerPosition;
    private Vector2 currentPointerPosition;
    private Vector2 initialElementPosition;

    private Vector2 targetPosition;
    private Quaternion targetRotation;

    private bool isDragging = false;

    private Vector2 dragDelta= Vector2.zero;

    private RectTransform currentTarget;
    private int swipeDirection=0;

    private InvestFeedbackSO currentFeedback;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = swipeableElement.anchoredPosition;
        targetRotation = swipeableElement.rotation;

        swipeableElement.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(isAnimating)
            return;
        if (isDragging)
        {
            swipeableElement.anchoredPosition = Vector2.Lerp(swipeableElement.anchoredPosition, targetPosition, swipeSpeed * Time.deltaTime);
            swipeableElement.rotation = Quaternion.Lerp(swipeableElement.rotation, targetRotation, swipeSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isAnimating)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out startPointerPosition))
            return;

        
        dragDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(isAnimating)
            return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out currentPointerPosition))
            return;
        isDragging = true;
        dragDelta = currentPointerPosition - startPointerPosition;
        RectTransform target = null;

        float p=0;
        
        if(dragDelta.x > centerTreshold)
        {
            target = swipeRightRef;
            p = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / dragTreshold);
            feedbackUI.SetTargetPosition(1);
            currentFeedback = GetInvestFeedback(1);
            //feedbackUI.SetText(true, currentFeedback.investSentence);
            swipeDirection=1;

        }
        else if (dragDelta.x < -centerTreshold)
        {

            target = swipeLeftRef;
            p = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / dragTreshold);
            feedbackUI.SetTargetPosition(-1);
            currentFeedback = GetInvestFeedback(-1);
            //feedbackUI.SetText(false, currentFeedback.investSentence);
            swipeDirection=-1;


        }
        else
        {
            target = swipeMiddleRef;
            feedbackUI.SetTargetPosition(0);

            p = 1;
            swipeDirection=0;

        }

        targetPosition = Vector2.Lerp(initialElementPosition, target.anchoredPosition, p);
        targetRotation = Quaternion.Lerp(swipeableElement.rotation, target.rotation, p);

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(isAnimating)
            return;
        isDragging = false;

        if(swipeDirection!=0){
            StartCoroutine(ValidateSwipe());
        }
    }

    private IEnumerator ValidateSwipe()
    {
        
        PlayInvestFeedbackSound();
        


        isAnimating = true;

        bool isBuy = swipeDirection == 1;


        if(isBuy){
            PlayRandomSound(buySounds, audioSourceBuy);
            Player.instance.Money -= 100000;
        }
        else {
            // Player.instance.Money += 100000;
        }
            

        RectTransform target = swipeDirection == 1 ? validateRightRef : validateLeftRef;
        float p = 0;
        float t = 0;
        Vector2 initialPosition = swipeableElement.anchoredPosition;
        Quaternion initialRotation = swipeableElement.rotation;

        while (t < validationAnimationDuration)
        {
            t += Time.deltaTime;
            p = t / validationAnimationDuration;
            swipeableElement.anchoredPosition = Vector2.Lerp(initialPosition, target.anchoredPosition, validationAnimationCurve.Evaluate(p));
            swipeableElement.rotation = Quaternion.Lerp(initialRotation, target.rotation, validationAnimationCurve.Evaluate(p));
            swipeableCanvasGroup.alpha = validationAnimationCurve.Evaluate(1 - p);
            yield return null;
        }

        swipeableElement.anchoredPosition = target.anchoredPosition;
        swipeableElement.rotation = target.rotation;
        swipeableCanvasGroup.alpha = 0;

        StartupInvestCard.Instance.LoadRandomStartup();


        yield return new WaitForSeconds(0.1f);
        // Return new Card !

        swipeBackFaceCard.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        
        // Animate back face card towards 90°
        swipeBackgroundImage.color = swipeBackgroundColor2;
        float t2 = 0;
        while (t2 < flipDuration)
        {
            t2 += Time.deltaTime;
            p = t2 / flipDuration;
            swipeBackFaceCard.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(0, 0, 0)), Quaternion.Euler(new Vector3(0, -90, 0)), validationAnimationCurve.Evaluate(p));
            yield return null;
        }
        feedbackUI.ResetPositions();
        PlayRandomSound(flipSounds, audioSourceFlip);
        // Animate SwipeElement card towards 0°
        swipeableElement.anchoredPosition = swipeMiddleRef.anchoredPosition;
        swipeableElement.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        swipeableCanvasGroup.alpha = 1;
        t2 = 0;
        while (t2 < flipDuration)
        {
            t2 += Time.deltaTime;
            p = t2 / flipDuration;
            swipeableElement.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(0, 90, 0)), Quaternion.Euler(new Vector3(0, 0, 0)), validationAnimationCurve.Evaluate(p));
            yield return null;
        }

        swipeBackgroundImage.color = swipeBackgroundColor1;
        isAnimating = false;
        
    }

    public InvestFeedbackSO GetInvestFeedback(int positiveness){
        // filter feedbacks corresponding to positiveness
        InvestFeedbackSO[] filteredFeedbacks = Array.FindAll(investFeedbacks, feedback => feedback.positiveness == positiveness);
        // get a random feedback
        return filteredFeedbacks[UnityEngine.Random.Range(0, filteredFeedbacks.Length)];

    }

    public void PlayInvestFeedbackSound()
    {
        
        audioSourceInvestFeedback.PlayOneShot(currentFeedback.investSound);
        
    }

    public void PlayRandomSound(AudioClip[] sounds, AudioSource audioSource)
    {
        audioSource.PlayOneShot(sounds[UnityEngine.Random.Range(0, sounds.Length)]);
    }
}
