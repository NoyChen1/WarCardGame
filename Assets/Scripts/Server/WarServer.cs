using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarServer
{
    private readonly ServerConfig _serverConfig;
    private readonly System.Random _rnd = new();

    private readonly Queue<CardData> _player = new();
    private readonly Queue<CardData> _opponent = new();

    public int PlayerCount => _player.Count;
    public int OpponentCount => _opponent.Count;


    public WarServer(ServerConfig serverConfig, List<CardData> fullDeck)
    {
        _serverConfig = serverConfig;
        Initialize(fullDeck);
    }

    private void Initialize(List<CardData> fullDeck)
    {
        var deck = new List<CardData>(fullDeck);
        var rnd = _serverConfig.shuffleSeed == 0 ? new System.Random() : new System.Random(_serverConfig.shuffleSeed);
        for (int i = deck.Count - 1; i >= 0; i--)
        {
            int j = rnd.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        for (int i = 0; i < deck.Count; i++)
        {
            if (i % 2 == 0)
            {
                _player.Enqueue(deck[i]);
            }
            else
            {
                _opponent.Enqueue(deck[i]);
            }
        }
    }

    public async UniTask<RoundResolution> DrawRoundAsync(System.Threading.CancellationToken ct)
    {
        await UniTask.Delay(UnityEngine.Random.Range(_serverConfig.minDelayMs, _serverConfig.maxDelayMs + 1), cancellationToken: ct);
        MaybeThrow();

        var res = new RoundResolution();
        var pot = new List<CardData>();

        if (_player.Count == 0 || _opponent.Count == 0)
        {
            res.GameOver = true;
            res.Outcome = WinnerWhenEmpty();
            res.PlayerCountAfter = _player.Count;
            res.OpponentCountAfter = _opponent.Count;
            return res;
        }

        while (true)
        {
            var p = _player.Dequeue();
            var o = _opponent.Dequeue();
            pot.Add(p); 
            pot.Add(o);

            res.Steps.Add(new RoundStep { IsPlayer = true, Visibility = StepVisibility.FaceUp, Card = p });
            res.Steps.Add(new RoundStep { IsPlayer = false, Visibility = StepVisibility.FaceUp, Card = o });

            int cmp = ((int)p.Rank).CompareTo((int)o.Rank);
            if (cmp > 0) 
            { 
                Award(_player, pot); 
                res.Outcome = TurnOutcome.PlayerWins; 
                break; 
            }
            else if (cmp < 0) 
            { Award(_opponent, pot); 
                res.Outcome = TurnOutcome.OpponentWins; 
                break; 
            }

            // WAR
            if (_player.Count < 2 || _opponent.Count < 2)
            {
                if (_player.Count < 2 && _opponent.Count < 2) 
                { 
                    res.Outcome = TurnOutcome.Draw; 
                    res.GameOver = true; 
                    break; 
                }
                
                if (_player.Count < 2) 
                { 
                    Award(_opponent, pot); 
                    res.Outcome = TurnOutcome.OpponentWins; 
                    res.GameOver = _player.Count == 0; 
                    break; 
                }
                else /*opponent<2*/      
                { 
                    Award(_player, pot); 
                    res.Outcome = TurnOutcome.PlayerWins; 
                    res.GameOver = _opponent.Count == 0; 
                    break; 
                }
            }

            var pDown = _player.Dequeue();
            var oDown = _opponent.Dequeue();
            pot.Add(pDown); 
            pot.Add(oDown);
            res.Steps.Add(new RoundStep { IsPlayer = true, Visibility = StepVisibility.FaceDown, Card = pDown });
            res.Steps.Add(new RoundStep { IsPlayer = false, Visibility = StepVisibility.FaceDown, Card = oDown });
        }

        res.PlayerCountAfter = _player.Count;
        res.OpponentCountAfter = _opponent.Count;
        if (_player.Count == 0 || _opponent.Count == 0)
        {
            res.GameOver = true;
        }

        return res;
    }

    void Award(Queue<CardData> winner, List<CardData> pot) { foreach (var c in pot) winner.Enqueue(c); pot.Clear(); }
    TurnOutcome WinnerWhenEmpty()
    {
        if (_player.Count > _opponent.Count)
        {
            return TurnOutcome.PlayerWins;
        }
        
        if (_opponent.Count > _player.Count)
        {
            return TurnOutcome.OpponentWins;
        }

        return TurnOutcome.Draw;
    }
    void MaybeThrow()
    {
        float r = (float)_rnd.NextDouble();
        if (r < _serverConfig.timeoutChance)
        {
            throw new FakeTimeoutException("Simulated timeout");
        }

        if (r < _serverConfig.timeoutChance + _serverConfig.networkErrorChance)
        {
            throw new FakeNetworkException("Simulated network error");
        }
    }

}
