using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int score;
    public int totalObjects;
    public int objectCollected;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI objectsText;
    [SerializeField] private GameObject winText;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        winText.SetActive(false);
        SetObjectsUI();
        totalObjects = FindObjectsOfType<Collectible>().Length;
    }

    public void AddObjectCollected()
    {
        objectCollected++;
        SetObjectsUI();
        if (objectCollected >= totalObjects)
        {
            winText.SetActive(true);
        }
    }

    private void SetObjectsUI()
    {
        objectsText.text = "OGGETTI RACCOLTI: " + objectCollected;
    }
}
