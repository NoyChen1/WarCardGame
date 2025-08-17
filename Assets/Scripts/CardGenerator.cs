//NOTE: mostely generated with AI

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public static class CardGenerator
{
    private const string FolderPath = "Assets/Cards";

    [MenuItem("Tools/Generate All Cards")]
    public static void GenerateCards()
    {
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
        }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Cards/Cards.png")
                                 .OfType<Sprite>()
                                 .ToArray();

        Sprite heart = sprites.FirstOrDefault(s => s.name == "Heart");
        Sprite diamond = sprites.FirstOrDefault(s => s.name == "Diamond");
        Sprite club = sprites.FirstOrDefault(s => s.name == "Club");
        Sprite spade = sprites.FirstOrDefault(s => s.name == "Spade");

        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in System.Enum.GetValues(typeof(Rank)))
            {
                string cardName = $"{rank}_of_{suit}";
                string assetPath = $"{FolderPath}/{cardName}.asset";

                CardData card = ScriptableObject.CreateInstance<CardData>();
                card.Suit = suit;
                card.Rank = rank;

                switch (suit)
                {
                    case Suit.Hearts: card.Artwork = heart; break;
                    case Suit.Diamonds: card.Artwork = diamond; break;
                    case Suit.Clubs: card.Artwork = club; break;
                    case Suit.Spades: card.Artwork = spade; break;
                }

                AssetDatabase.CreateAsset(card, assetPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 52 cards with artworks!");
    }
}
