using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class UITweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum TweenType
    {
        Scale
    }

    [SerializeField] private TweenType tweenType;
    [SerializeField] private RectTransform rectTransformToTween;
    [SerializeField] private float time;
    [SerializeField] private Vector3 from;
    [SerializeField] private Vector3 to;

    [Header("Fire")] 
    [SerializeField] private bool fireMouseOver = false;
    [FormerlySerializedAs("awake")] [SerializeField] private bool enable;

    [Header("Events")] [SerializeField] private UnityEvent onAwake;
    [SerializeField] private UnityEvent onMouseOver;
    [SerializeField] private UnityEvent onTweenComplete;
    [SerializeField] private UnityEvent onReverseTweenComplete;

    private bool _mouseOver = false;


    public void HandleTween()
    {
        switch (tweenType)
        {
            case TweenType.Scale:
                HandleScaleTween();
                break;
        }
    }

    public void HandleReverseTween()
    {
        switch (tweenType)
        {
            case TweenType.Scale:
                HandleReverseScaleTween();
                break;
        }
    }
    

    private void HandleScaleTween()
    {
        rectTransformToTween.localScale = from;
        LeanTween.scale(rectTransformToTween, to, time)
            .setOnComplete(onTweenComplete.Invoke);
    }

    private void HandleReverseScaleTween()
    {
        rectTransformToTween.localScale = to;
        LeanTween.scale(rectTransformToTween, from , time)
            .setOnComplete(onReverseTweenComplete.Invoke);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (fireMouseOver && !_mouseOver)
        {
            _mouseOver = true;
            onMouseOver.Invoke();
            HandleTween();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (fireMouseOver && _mouseOver)
        {
            _mouseOver = false;
            HandleReverseTween();
        }
    }

    private void Awake()
    {
        onAwake.Invoke();
    }

    private void OnEnable()
    {
        if (enable)
        {
            HandleTween();
        }
        
        
    }

    private void OnDisable()
    {
        if (enable)
        {
            HandleReverseTween();
        }
    }
}
