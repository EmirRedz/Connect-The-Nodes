using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    [Header("General stats")]
    public float maxAmount;
    public float minAmount;
    public Image imageFill;
    
    public void SetValue(float value, float maxValue)
    {
        float calc_value = value / maxValue;
        float outputValue = calc_value * (maxAmount - minAmount) + minAmount;
        //imageFill.fillAmount = Mathf.Clamp(outputValue, minAmount, maxAmount);
        var endValue = Mathf.Clamp(outputValue, minAmount, maxAmount);
        imageFill.DOFillAmount(endValue, 0.15f);
    }

    public void SetMaxValue(float maxValue)
    {
        float outputMaxValue = maxValue * (maxAmount - minAmount) + minAmount;
        imageFill.fillAmount = Mathf.Clamp(outputMaxValue,minAmount,maxAmount);
    }
}
