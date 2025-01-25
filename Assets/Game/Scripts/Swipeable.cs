using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Swipeable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform swipeableElement;
    [Header("Positions de référence")]
    public RectTransform leftPosition;
    public RectTransform middlePosition;
    public RectTransform rightPosition;

    [Header("Paramètres de transition")]
    public float dragTreshold = 200f;
    public float centerTreshold = 50f;
    public float swipeSpeed = 10f;
    

    private RectTransform rectTransform;
    private Vector2 startPointerPosition;
    private Vector2 currentPointerPosition;
    private Vector2 initialElementPosition;

    private Vector2 targetPosition;
    private Quaternion targetRotation;

    private bool isDragging = false;

    private Vector2 dragDelta= Vector2.zero;

    private RectTransform currentTarget;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = swipeableElement.anchoredPosition;
        targetRotation = swipeableElement.rotation;
    }

    private void Update()
    {
        if (isDragging)
        {
            swipeableElement.anchoredPosition = Vector2.Lerp(swipeableElement.anchoredPosition, targetPosition, swipeSpeed * Time.deltaTime);
            swipeableElement.rotation = Quaternion.Lerp(swipeableElement.rotation, targetRotation, swipeSpeed * Time.deltaTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out startPointerPosition))
            return;

        isDragging = true;
        dragDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out currentPointerPosition))
            return;

        
        

        dragDelta = currentPointerPosition - startPointerPosition;
        RectTransform target = null;

        float p=0;
        
        if(dragDelta.x > centerTreshold)
        {
            target = rightPosition;
            p = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / dragTreshold);
        }
        else if (dragDelta.x < -centerTreshold)
        {

            target = leftPosition;
            p = Mathf.Clamp01(Mathf.Abs(dragDelta.x) / dragTreshold);

        }
        else
        {
            target = middlePosition;
            p = 1;
        }
        targetPosition = Vector2.Lerp(initialElementPosition, target.anchoredPosition, p);
        targetRotation = Quaternion.Lerp(swipeableElement.rotation, target.rotation, p);

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

    }


}
