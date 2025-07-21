using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class FlowLayoutGroup : LayoutGroup
{
    public float SpacingX = 10f;
    public float SpacingY = 10f;
    private float totalPreferredHeight = 0f;


    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        CalcPositions();
    }

    public override void CalculateLayoutInputVertical()
    {
        CalcPositions();
    }

    public override void SetLayoutHorizontal()
    {
        SetChildrenPositions();
    }

    public override void SetLayoutVertical()
    {
        SetChildrenPositions();
    }

    private void CalcPositions()
    {
        float parentWidth = rectTransform.rect.width;

        float x = padding.left;
        float y = padding.top;
        float maxHeightThisLine = 0f;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            float w = LayoutUtility.GetPreferredWidth(child) + SpacingX;
            float h = LayoutUtility.GetPreferredHeight(child) + SpacingY;

            if (x + w > parentWidth - padding.right)
            {
                // Wrap to new line
                x = padding.left;
                y += maxHeightThisLine;
                maxHeightThisLine = 0f;
            }

            child.anchoredPosition = new Vector2(x, -y);
            x += w;
            maxHeightThisLine = Mathf.Max(maxHeightThisLine, h);
        }

        totalPreferredHeight = y + maxHeightThisLine + padding.bottom;
    }

    private void SetChildrenPositions()
    {
        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform child = rectChildren[i];
            SetChildAlongAxis(child, 0, child.anchoredPosition.x);
            SetChildAlongAxis(child, 1, -child.anchoredPosition.y);
        }
    }


public override float minHeight => totalPreferredHeight;
public override float preferredHeight => totalPreferredHeight;
public override float flexibleHeight => -1;

}
