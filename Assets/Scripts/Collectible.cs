using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private CollectibleData collectibleData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                CollectItem(player);
                Destroy(gameObject);
            }
        }
    }

    private void CollectItem(Player player)
    {
        switch (collectibleData.type)
        {
            case CollectibleType.Health:
                print("Healed by: " + collectibleData.value);
                player.GainHealth(collectibleData.value);
                GameManager.Instance.AddObjectCollected();
                break;
            case CollectibleType.MarioStar:
                player.StartInvincibleRoutine(collectibleData.value);
                print($"Collected {collectibleData.objectName} worth {collectibleData.value}");
                GameManager.Instance.AddObjectCollected();
                break;
        }
    }
}
