using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ally : Character
{
    [Header("Ally Settings")]
    [SerializeField] private float healingAmount = 5f; // Quantità di salute da curare
    [SerializeField] private float HealingTimer = 15f; // Raggio di cura
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0, 0); // Offset della posizione

    [Header("DEBUG VARIABLES")]
    [SerializeField] private Player player; // Riferimento al giocatore

    private void Start()
    {
        player = FindObjectOfType<Player>();
        StartCoroutine(HealPlayerRoutine());
    }

    private IEnumerator HealPlayerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(HealingTimer);

            PerformAction();
        }
    }

    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position + positionOffset, characterData.speed * Time.fixedDeltaTime);
    }

    private void HealPlayer()
    {
        if (player != null)
        {
            player.GainHealth(healingAmount);
            Debug.Log("Healed player: " + player.name);
        }
    }

    protected override void PerformAction()
    {
        HealPlayer();
    }
}
