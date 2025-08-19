using DG.Tweening;
using UnityEngine;

public class WinDanceSystem : MonoBehaviour
{
    [Header("Deck visuals (RectTransform of the deck images)")]
    [SerializeField] private RectTransform playerDeckVisual;
    [SerializeField] private RectTransform opponentDeckVisual;

    [Header("Timing")]
    [SerializeField, Range(0.1f, 1.5f)] private float winDanceDuration = 0.6f;
    [SerializeField, Range(0.0f, 0.3f)] private float winPunchScale = 0.12f;
    [SerializeField, Range(0f, 20f)] private float winWobbleDegrees = 12f;

    void OnEnable()
    {
        Signals.AfterRoundResolved += OnRoundResolved;
    }
    void OnDisable()
    {
        Signals.AfterRoundResolved -= OnRoundResolved;
    }

    async void OnRoundResolved(RoundResolution rr)
    {
        RectTransform target = rr.Outcome switch
        {
            TurnOutcome.PlayerWins => playerDeckVisual,
            TurnOutcome.OpponentWins => opponentDeckVisual,
            _ => null
        };
        if (!target) return;

        var seq = DOTween.Sequence()
            .Join(target.DOPunchScale(new Vector3(winPunchScale, winPunchScale, 0f),
                                      winDanceDuration, vibrato: 6, elasticity: 0.8f))
            .Join(target.DOPunchRotation(new Vector3(0f, 0f, winWobbleDegrees),
                                         winDanceDuration, vibrato: 6, elasticity: 0.8f));
        await seq.AsyncWaitForCompletion();
    }
}
