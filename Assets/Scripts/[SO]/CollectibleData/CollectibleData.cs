using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CollectibleData", menuName = "ScriptableObjects/CollectibleData")]
public class CollectibleData : ScriptableObject
{
    public string objectName;
    public CollectibleType type;
    public float value;
}

public enum CollectibleType
{
    Health,
    MarioStar,
}
