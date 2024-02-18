using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Current Bonus Visual")]
    [SerializeField] private SpriteRenderer currentBonusSr;
    [SerializeField] private TMP_Text currentBonusText;
    [SerializeField] private float bonusTweenTime = 0.05f;
    private void Awake()
    {
        Instance = this;
    }

    public void SetCurrentBonusText(int currentBonus, Color currentBonusColor)
    {
        if (currentBonus <= 0)
        {
            currentBonusText.DOColor(Color.clear, bonusTweenTime).SetEase(Ease.Linear);
        }
        else
        {
            currentBonusText.DOColor(Color.white, bonusTweenTime).SetEase(Ease.Linear);
        }
        currentBonusText.SetText(currentBonus.ToString());
        currentBonusSr.DOColor(currentBonusColor, bonusTweenTime).SetEase(Ease.Linear);

    }
}
