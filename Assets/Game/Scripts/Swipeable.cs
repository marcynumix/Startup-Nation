using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Globalization;
using NUnit.Framework.Constraints;

public class Swipeable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static Swipeable instance;
    public RectTransform swipeableElement;
    public CanvasGroup swipeableCanvasGroup;

    public RectTransform startupCard;
    public RectTransform gameoverCard;


    public RectTransform swipeBackFaceCard;
    public Image swipeBackgroundImage;
    public Color swipeBackgroundColor1;
    public Color swipeBackgroundColor2;
    public feedbackUI feedbackUI;
    public TMPro.TextMeshProUGUI bidText;

    
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

    public float verticalTresholdRatio=0.5f;
    public float verticalTreshold = 1200f;
    public AnimationCurve verticalTresholdCurve;
    bool isAnimating = false;

    [Header("Audio")]

    public AudioSource audioSourceBuyFeedback;
    public AudioSource audioSourceFlip;
    public AudioSource audioSourceFX;
    public AudioClip[] buySoundsPositive;
    public AudioClip[] buySoundsnegative;
    public AudioClip[] flipSounds;
    public AudioClip[] refusalSounds;
    public AudioClip[] gameOverSounds;
    public AudioClip[] newGameSounds;
    public AudioClip[] winMoneySounds;

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
        instance=this;
        rectTransform = GetComponent<RectTransform>();
        targetPosition = swipeableElement.anchoredPosition;
        targetRotation = swipeableElement.rotation;

        swipeableElement.gameObject.SetActive(true);
        startupCard.gameObject.SetActive(true);
        gameoverCard.gameObject.SetActive(false);
    }

    void Start(){
        PlayRandomSound(newGameSounds, audioSourceFX);
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
            swipeDirection=1;

        }
        else if (dragDelta.x < -centerTreshold)
        {

            target = swipeLeftRef;
            p = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / dragTreshold);
            feedbackUI.SetTargetPosition(-1);
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
        if(swipeDirection==1){
            float pVertical = Mathf.Clamp01(Mathf.Abs(dragDelta.y) / verticalTreshold);
            targetPosition.y += Mathf.Sign(dragDelta.y)*verticalTresholdCurve.Evaluate(pVertical)*verticalTreshold;// * verticalTresholdRatio;
            
            float pBid = Mathf.Clamp(dragDelta.y / verticalTreshold,-1,1);
            // map value -1,1 to 0,1
            pBid = (pBid + 1) / 2;
            
            int maxBid = Player.instance.Money;

            Player.instance.MoneyBid = (int)GetBidPercentage(maxBid, pBid);
            bidText.text = Player.instance.MoneyBid.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));

            if(Player.instance.MoneyBid==Player.instance.Money){
                SetInvestFeedback(2);
            }
            else{
                SetInvestFeedback(1);
            }
        }
        if(swipeDirection==-1){
            SetInvestFeedback(-1);
        }

        targetRotation = Quaternion.Lerp(swipeableElement.rotation, target.rotation, p);

    }
    // write a function that will take min, max, percentage, and will return 1 of 4 values (10%,25%,50%,100%)
    // 0-0.25 -> 10%
    // 0.25-0.5 -> 25%
    // 0.5-0.75 -> 50%
    // 0.75-1 -> 100%
    private float GetBidPercentage(float max, float percentage){
        if(percentage<0.25f)
            return max*0.1f;
        else if(percentage<0.5f)
            return max*0.25f;
        else if(percentage<0.9f)
            return max*0.5f;
        else
            return max;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(isAnimating)
            return;
        isDragging = false;

        if(swipeDirection!=0){
            StartCoroutine(ValidateSwipeCoroutine());
        }
    }

    public void Swipe(){
        StartCoroutine(ValidateSwipeCoroutine());
    }

    private IEnumerator ValidateSwipeCoroutine()
    {     
        bool gameRestarting = Player.instance.isGameOver;
        if(gameRestarting){
            Player.instance.isGameOver=false;
            Player.instance.Money = Player.instance.startMoney;
            
            PlayRandomSound(newGameSounds, audioSourceFX);
            
        }
        

        isAnimating = true;

        bool isBuy = swipeDirection == 1;

        if(!gameRestarting){
            if(isBuy){
                

                // Decide if win or lose money
                StartupData startup = StartupInvestCard.Instance.currentStartup;
                float successRate = startup.SuccessRate;

                bool win = UnityEngine.Random.value < successRate;
                int sign = win ? 1 : -1;
                Player.instance.Money += sign*Player.instance.MoneyBid;

                
                if(win){
                    PlayRandomSound(buySoundsPositive, audioSourceBuyFeedback);
                    PlayRandomSound(winMoneySounds, audioSourceFX);
                }
                else {
                    PlayRandomSound(buySoundsnegative, audioSourceBuyFeedback);
                }
            }
            else {
                // Player.instance.Money += 100000;
                StartupInvestCard.Instance.MutePitch();
            }

            if(!isBuy){
                PlayRandomSound(refusalSounds, audioSourceBuyFeedback);
            }
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
        
        
        if(Player.instance.Money>0){
            
        }
        else {
            Player.instance.isGameOver = true;
            
        }
        yield return StartCoroutine(FlipNewCardCoroutine());

        isAnimating = false;
    }

    public void FlipNewCard(){
        StartCoroutine(FlipNewCardCoroutine());
    }
    public IEnumerator FlipNewCardCoroutine(){
            startupCard.gameObject.SetActive(!Player.instance.isGameOver);
            gameoverCard.gameObject.SetActive(Player.instance.isGameOver);
            if(Player.instance.isGameOver){
                PlayRandomSound(gameOverSounds, audioSourceFX);
            }
            // Animate back face card towards 90°
            swipeBackgroundImage.color = swipeBackgroundColor2;
            float t2 = 0;
            float p=0;
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
            swipeableElement.gameObject.SetActive(true);
            swipeableElement.anchoredPosition = swipeMiddleRef.anchoredPosition;
            swipeableElement.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
            swipeableCanvasGroup.alpha = 1;
            t2 = 0;
            p=0;
            while (t2 < flipDuration)
            {
                t2 += Time.deltaTime;
                p = t2 / flipDuration;
                swipeableElement.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(0, 90, 0)), Quaternion.Euler(new Vector3(0, 0, 0)), validationAnimationCurve.Evaluate(p));
                yield return null;
            }
            swipeBackgroundImage.color = swipeBackgroundColor1;
            if(!Player.instance.isGameOver){
                StartupInvestCard.Instance.PlayPitch(0);
            }
    }

    public void RestartGame(){
        StartCoroutine(RestartGameCoroutine());
    }
    private IEnumerator RestartGameCoroutine(){
        swipeBackgroundImage.color = swipeBackgroundColor1;
        // Animate gameover card towards 90°
        float t2 = 0;
        float p=0;
        while (t2 < flipDuration)
        {
            t2 += Time.deltaTime;
            p = t2 / flipDuration;
            gameoverCard.rotation = Quaternion.Lerp(Quaternion.Euler(new Vector3(0, 0, 0)), Quaternion.Euler(new Vector3(0, -90, 0)), validationAnimationCurve.Evaluate(p));
            yield return null;
        }
        yield return StartCoroutine(FlipNewCardCoroutine());

    }



    public void SetInvestFeedback(int positiveness){
        // Return if positiveness is the same
        if(currentFeedback!=null && currentFeedback.positiveness==positiveness){
            return ;
        }
        InvestFeedbackSO[] filteredFeedbacks = Array.FindAll(investFeedbacks, feedback => feedback.positiveness == positiveness);
        // get a random feedback
        currentFeedback=filteredFeedbacks[UnityEngine.Random.Range(0, filteredFeedbacks.Length)];
        //SetText
        feedbackUI.SetText(positiveness>0, currentFeedback.investSentence);
    }

    public void PlayRandomSound(AudioClip[] sounds, AudioSource audioSource)
    {
        audioSource.PlayOneShot(sounds[UnityEngine.Random.Range(0, sounds.Length)]);
    }
}
