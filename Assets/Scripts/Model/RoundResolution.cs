using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundResolution
{
   public List<RoundStep> Steps = new();
    public TurnOutcome Outcome;
    public int PlayerCountAfter;
    public int OpponentCountAfter;
    public bool GameOver;
    public string Message;
}
