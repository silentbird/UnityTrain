using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickCube : MonoBehaviour,IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var material = GetComponent<MeshRenderer>().material;
        if (material.color == Color.black)
        {
            material.SetColor("_Color", Color.red);
        }
        else
        {
            material.SetColor("_Color", Color.black);
        }
    }
}