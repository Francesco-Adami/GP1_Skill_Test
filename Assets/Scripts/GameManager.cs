using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int score;
    public int totalObjects;
    public int objectCollected;


    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI objectsText;

    public GameObject player;
    public Slider playerSlider;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        playerSlider.maxValue = FindAnyObjectByType<Player>().health.maxHealth;
        playerSlider.value = FindAnyObjectByType<Player>().health.currentHealth;
        player = FindAnyObjectByType<Player>().gameObject;
        totalObjects = FindObjectsOfType<Collectible>().Length;
        SetObjectsUI();
    }

    public void AddObjectCollected()
    {
        objectCollected++;
        SetObjectsUI();
        if (objectCollected >= totalObjects)
        {
            UIManager.instance.ShowUI(UIManager.GameUI.Win);
        }
    }

    private void SetObjectsUI()
    {
        objectsText.text = "OGGETTI RACCOLTI: " + objectCollected + "/" + totalObjects;
    }

    internal void SetPlayerSlider(float currentHealth)
    {
        playerSlider.value = currentHealth;
    }
}
