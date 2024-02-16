using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private TMP_Text currentBonusText;
    private void Awake()
    {
        Instance = this;
    }

    public void SetCurrentBonusText(int currentBonus)
    {
        currentBonusText.SetText(currentBonus.ToString());
    }
}
