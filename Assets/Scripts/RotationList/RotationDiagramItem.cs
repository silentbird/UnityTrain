using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RotationDiagramItem : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Image _image;

    public Image Image
    {
        get
        {
            _image = GetComponent<Image>();
            return _image;
        }
    }

    private RectTransform _rectTransform;

    public RectTransform Rect
    {
        get
        {
            _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void SetSprite(Sprite sprite)
    {
        Image.sprite = sprite;
    }


    public void SetPosData(RotationDiagram2D.ItemPositionData posData)
    {
        Rect.anchoredPosition = Vector2.right * posData.X;
        Rect.localScale = Vector3.one * posData.ScaleTimes;
        transform.SetSiblingIndex(posData.OrderId);
    }

    private float _deltaX = 0;
    public void OnDrag(PointerEventData eventData)
    {
        _deltaX = eventData.delta.x;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
    }

    public void AddListener(Action action)
    {
        
    }
}