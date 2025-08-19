using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Signals
{
    public static event Action RoundStarted;
    public static event Action WarMoment;
    public static event Action<RoundResolution> RoundResolved;
    public static event Action<RoundResolution> AfterRoundResolved;
    public static event Action<TurnOutcome> GameOver;

    public static void OnRoundStarts()
    {
        RoundStarted?.Invoke();
    }

    public static void OnWarMoment()
    {
        WarMoment?.Invoke();
    }
    public static void OnRoundResolved(RoundResolution roundResolution)
    {
        RoundResolved?.Invoke(roundResolution);
    }

    public static void OnAfterRoundResolved(RoundResolution roundResolution)
    {
        AfterRoundResolved?.Invoke(roundResolution);
    }

    public static void OnGameOver(TurnOutcome turnOutcome)
    {
        GameOver?.Invoke(turnOutcome);
    }

}
