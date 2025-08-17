using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "War/CardDataBase")]
public class CardDataBase : ScriptableObject
{
    public List<CardData> allCards = new();
}
