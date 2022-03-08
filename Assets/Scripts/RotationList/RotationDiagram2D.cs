using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RotationDiagram2D : MonoBehaviour
{
    public Vector2 SizeData;
    public Sprite[] ImageSprites;
    public float ScaleTimeMax = 1f;
    public float ScaleTimeMin = 0.5f;
    private List<RotationDiagramItem> _ListRotationDiagram;
    private List<ItemPositionData> _ListItemPosData = new List<ItemPositionData>();

    public class ItemPositionData
    {
        public float X;
        public float ScaleTimes;
        public int OrderId;
        public int ItemIdx;
    }

    public struct ItemOrderData
    {
        public int ItemIdx;
        public int OrderId;
    }

    private void Start()
    {
        CreateItems();
        InitPosData();
        SetPosData();
    }

    private void InitPosData()
    {
        for (int i = 0; i < ImageSprites.Length; i++)
        {
            ItemPositionData posData = new ItemPositionData();
            float length = (20 + SizeData.x) * _ListRotationDiagram.Count;
        }
        CalculatePosData(0f);
    }

    private float tmpMove = 0f;
    private void Update()
    {
        tmpMove += 0.001f;
        CalculatePosData(tmpMove);
        SetPosData();
    }

    private GameObject CreateTemplate()
    {
        GameObject template = new GameObject("Template");
        template.AddComponent<RectTransform>().sizeDelta = SizeData;
        template.AddComponent<Image>();
        template.AddComponent<RotationDiagramItem>();
        return template;
    }

    private void CreateItems()
    {
        GameObject template = CreateTemplate();
        RotationDiagramItem temp = null;
        _ListRotationDiagram = new List<RotationDiagramItem>();
        foreach (var sprite in ImageSprites)
        {
            temp = Instantiate(template).GetComponent<RotationDiagramItem>();
            temp.SetParent(transform);
            temp.SetSprite(sprite);
            _ListRotationDiagram.Add(temp);
        }

        Destroy(template);
    }

    
    //moveRadio:0-1
    private void CalculatePosData(float moveRadio)
    {
        List<ItemOrderData> tmpOrderData = new List<ItemOrderData>();
        _ListItemPosData = new List<ItemPositionData>();
        float radioOffset = 1f / ImageSprites.Length;
        float radio = moveRadio - Mathf.Floor(moveRadio);

        for (int i = 0; i < ImageSprites.Length; i++)
        {
            ItemPositionData posData = new ItemPositionData();
            float length = (20 + SizeData.x) * _ListRotationDiagram.Count;
            posData.X = GetX(radio, length);
            posData.ScaleTimes = GetScaleTimes(radio, ScaleTimeMax, ScaleTimeMin);
            radio += radioOffset;
            _ListItemPosData.Add(posData);
            //临时order list
            ItemOrderData orderData = new ItemOrderData();
            orderData.ItemIdx = i;
            tmpOrderData.Add(orderData);
        }

        tmpOrderData = tmpOrderData.OrderBy(u => _ListItemPosData[u.ItemIdx].ScaleTimes).ToList();
        for (int i = 0; i < tmpOrderData.Count; i++)
        {
            _ListItemPosData[tmpOrderData[i].ItemIdx].OrderId = i;
        }
    }


    private void SetPosData()
    {
        for (int i = 0; i < _ListRotationDiagram.Count; i++)
        {
            _ListRotationDiagram[i].SetPosData(_ListItemPosData[i]);
        }
    }

    float GetX(float radio, float length)
    {
        if (radio >= 0 && radio < 0.25f)
        {
            return radio * length;
        }
        else if (radio >= 0.25f && radio < 0.75f)
        {
            return (0.5f - radio) * length;
        }
        else
        {
            return (radio - 1) * length;
        }
    }

    float GetScaleTimes(float radio, float max, float min)
    {
        if (radio < 0.5)
        {
            return max - (max - min) * radio / 0.5f;
        }
        else
        {
            return min + (max - min) * (radio - 0.5f) / 0.5f;
        }
    }


}