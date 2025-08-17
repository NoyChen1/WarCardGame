using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Card")]
public class CardData : ScriptableObject
{
    public Suit Suit;
    public Rank Rank;
    public Sprite Artwork;
}
