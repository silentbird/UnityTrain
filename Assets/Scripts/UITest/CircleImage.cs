using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

public class CircleImage : Image
{
    [SerializeField] private int segements = 3;

    [SerializeField] private float showPercent = 1;

    private List<Vector3> _vertexList = new List<Vector3>();
    private PolygonCollider2D _polygon;

    private PolygonCollider2D PolygonCollider2D
    {
        get
        {
            if (_polygon==null)
            {
                _polygon = GetComponent<PolygonCollider2D>();
            }
            return _polygon;
        }
    }

    //重写渲染图片逻辑
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // base.OnPopulateMesh(vh);
        vh.Clear();
        _vertexList.Clear();
        
        var width = rectTransform.rect.width;
        var height = rectTransform.rect.height;

        Vector4 uv = overrideSprite ? DataUtility.GetOuterUV(overrideSprite) : Vector4.zero;
        Vector2 uvCenter = new Vector2((uv.z - uv.x) * 0.5f, (uv.w - uv.y) * 0.5f);

        //轴心点偏移量
        Vector2 diff = new Vector2((0.5f - rectTransform.pivot.x) * width, (0.5f - rectTransform.pivot.y) * height);
        //圆心坐标
        vh.AddVert(diff, color, uvCenter);


        var radian = 2 * Mathf.PI / segements;
        var tmpRadian = 0f;
        for (int i = 0; i < segements; i++)
        {
            var curVertexPos = new Vector3(diff.x + Mathf.Cos(tmpRadian) * width / 2,
                diff.y + Mathf.Sin(tmpRadian) * width / 2);
            _vertexList.Add(curVertexPos);

            vh.AddVert(curVertexPos, color,
                new Vector2(0.5f + Mathf.Cos(tmpRadian) * 0.5f, 0.5f + Mathf.Sin(tmpRadian) * 0.5f));
            tmpRadian += radian;
        }
        for (int i = 0; i < segements + 1; i++)
        {
            vh.AddTriangle(0, i, i + 1 <= segements ? i + 1 : 1);
        }
    }

    //点击检测
    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera,
        //     out Vector2 localPos);
        // Debug.Log(GetCrossPointNum(localPos));
        // Debug.Log(GetCrossPointNum(localPos) % 2 == 1);
        // return GetCrossPointNum(localPos) % 2 == 1;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera,
            out Vector3 point);
        
        return PolygonCollider2D.OverlapPoint(point);; 
    }

    private int GetCrossPointNum(Vector3 localPos)
    {
        var vertCount = _vertexList.Count;

        var count = 0;
        for (int i = 0; i < vertCount; i++)
        {
            var vert1 = _vertexList[i];
            var vert2 = _vertexList[(i + 1) % vertCount];
            if (isYBetweenVert(vert1, vert2, localPos))
            {
                if (localPos.x < GetX(vert1, vert2, localPos.y))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool isYBetweenVert(Vector3 vert1, Vector3 vert2, Vector3 localPos)
    {
        if (vert1.y > vert2.y)
        {
            return localPos.y > vert2.y && localPos.y < vert1.y;
        }
        else
        {
            return localPos.y > vert1.y && localPos.y < vert2.y;
        }
    }


    private float GetX(Vector3 vert1, Vector3 vert2, float y)
    {
        float k = (vert1.y - vert2.y) / (vert1.x - vert2.x);
        float b = vert1.y - k * vert1.x;
        return (y - b) / k;
    }
}