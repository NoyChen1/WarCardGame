using UnityEngine;

public class SfxSystem : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioClip playerWinSfx;
    [SerializeField] private AudioClip opponentWinSfx;
    [SerializeField] private AudioClip warSfx;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    void OnEnable()
    {
        Signals.WarMoment += OnWar;
        Signals.RoundResolved += OnRoundResolved;
    }
    void OnDisable()
    {
        Signals.WarMoment -= OnWar;
        Signals.RoundResolved -= OnRoundResolved;
    }

    private void OnWar()
    {
        Play(warSfx);
    }
    void OnRoundResolved(RoundResolution rr)
    {
        if (rr.Outcome == TurnOutcome.PlayerWins)
        {
            Play(playerWinSfx);
        }
        else if (rr.Outcome == TurnOutcome.OpponentWins)
        {
            Play(opponentWinSfx);
        }
    }

    void Play(AudioClip clip)
    {
        if (sfx && clip) sfx.PlayOneShot(clip, volume);
    }
}
