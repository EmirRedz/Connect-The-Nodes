using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class LevelData
{
    public TMP_Text levelText;
    public Image levelImage;
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Camera mainCamera;
    public CinemachineVirtualCamera vcam;
    
    [Header("Camera Adjuster")] 
    [SerializeField] private Vector2 aspectRatio;
    [SerializeField] private float targetOrthoSize;
    
    [Header("Current Bonus Visual")]
    [SerializeField] private SpriteRenderer currentBonusSr;
    [SerializeField] private TMP_Text currentBonusText;
    [SerializeField] private float bonusTweenTime = 0.05f;
    
    [Header("XP System")]
    [SerializeField] private UIBar xpBar;
    [SerializeField] private int maxLevel;
    [SerializeField] private List<Color> levelColors;
    [SerializeField] private int targetXP;
    [SerializeField] private float targetXpLevelMultiplier;
    [SerializeField] private LevelData currentLevelData;
    [SerializeField] private LevelData nextLevelData;
    public static Action OnLevelUp;
    private int currentXP;
    private int currentLevel;

 
    
    private void Awake()
    {
        Instance = this;

        currentLevel = PlayerPrefs.GetInt(PlayerPrefsManager.CURRENT_LEVEL_KEY,0);
        currentXP = PlayerPrefs.GetInt(PlayerPrefsManager.CURRENT_LEVEL_XP,0);
        targetXP = PlayerPrefs.GetInt(PlayerPrefsManager.CURRENT_LEVEL_TARGET_XP,100);
    }
    
    private void Start()
    {
        SetCameraOrthographicSize();
        SetCurrentBonusText(0, Color.clear);
        
        xpBar.SetMaxValue(targetXP);
        xpBar.SetValue(currentXP, targetXP);
        if (currentLevel >= maxLevel-1)
        {
            SetMaxLevelUI();
        }
        else
        {
            SetLevelUI();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddXP(Mathf.RoundToInt(targetXP * 0.5f));
        }
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
    
    public void AddXP(int xp)
    {
        currentXP += xp;
        
        if (currentXP >= targetXP)
        {
            currentLevel++;
            PlayerPrefs.SetInt(PlayerPrefsManager.CURRENT_LEVEL_KEY, currentLevel);
            
            currentLevelData.levelImage.transform.DOKill(true);
            currentLevelData.levelImage.transform.DOPunchScale(Vector3.one * 0.25f, 0.35f);
            
            nextLevelData.levelImage.transform.DOKill(true);
            nextLevelData.levelImage.transform.DOPunchScale(Vector3.one * 0.25f, 0.35f);
            
            if (currentLevel >= maxLevel-1)
            {
                SetMaxLevelUI();
            }
            else
            {
                SetLevelUI();
                if (currentLevel % 6 == 0)
                {
                    OnLevelUp?.Invoke();
                }
            }
        }
        //Level Up
        while (currentXP >= targetXP)
        {
            int newCurrentXP = targetXP - currentXP;
            currentXP = Mathf.Abs(newCurrentXP);
            targetXP += Mathf.RoundToInt(targetXP * targetXpLevelMultiplier);
        }

        xpBar.SetValue(currentXP, targetXP);

        PlayerPrefs.SetInt(PlayerPrefsManager.CURRENT_LEVEL_XP, currentXP);
        PlayerPrefs.SetInt(PlayerPrefsManager.CURRENT_LEVEL_TARGET_XP, targetXP);
        PlayerPrefs.Save();
    }

    private void SetMaxLevelUI()
    {
        currentLevel = maxLevel - 1;
        SetMaxLevelData(currentLevelData);
        SetMaxLevelData(nextLevelData);
    }

    private void SetMaxLevelData(LevelData data)
    {
        data.levelText.SetText("Infinity");
        data.levelText.enableAutoSizing = true;
        data.levelImage.color = levelColors[currentLevel];
    }

    private void SetLevelUI()
    {
        int levelToShow = currentLevel + 1;
        currentLevelData.levelText.SetText(levelToShow.ToString());
        currentLevelData.levelImage.color = levelColors[currentLevel];
        
        nextLevelData.levelText.SetText((levelToShow + 1).ToString());
        nextLevelData.levelImage.color = levelColors[currentLevel+1];

        xpBar.imageFill.color = levelColors[currentLevel];

    }

    private void SetCameraOrthographicSize()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float targetAspectRatio = aspectRatio.x / aspectRatio.y; 

        float currentAspectRatio = screenWidth / screenHeight;
        float scaleFactor = targetAspectRatio / currentAspectRatio;
        float orthographicSize = targetOrthoSize * scaleFactor;

        vcam.m_Lens.OrthographicSize = orthographicSize;
    }
    [Button]
    private void SetLevelColors()
    {
        levelColors.Clear();
        for (int i = 0; i < maxLevel; i++)
        {
            float hue = i / (float)maxLevel;
            Color color = Color.HSVToRGB(hue, 1f, 1f);
            levelColors.Add(color);
        }
    }
}
