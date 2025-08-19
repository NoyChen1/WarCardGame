using Cysharp.Threading.Tasks;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private ServerConfig serverConfig;
    [SerializeField] private CardDataBase cardDb;

    [Header("UI")]
    [SerializeField] private Button tapAnywhereButton;
    [SerializeField] private CardView playerCardView;
    [SerializeField] private CardView opponentCardView;

    [Header("Counters/Toast")]
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI opponentCountText;
    [SerializeField] private TextMeshProUGUI toastText;

    [Header("War FX")]
    [SerializeField] private GameObject warBanner;
    [SerializeField, Range(0f, 3f)] private float warPauseSeconds = 1f;
    [SerializeField, Range(0f, 2f)] private float faceUpHoldSeconds = 0.6f;
    [SerializeField, Range(0f, 2f)] private float faceDownPauseSeconds = 0.35f;

    [Header("Flip Timing")]
    [SerializeField, Range(0f, 2f)] private float backHoldSeconds = 0.25f;
    [SerializeField, Range(0.1f, 1.0f)] private float flipDuration = 0.35f;



    private WarServer _server;
    private ServerConnection _connection;
    private bool _busy;


    private void Awake()
    {
        _server = new WarServer(serverConfig, cardDb.allCards);
        _connection = new ServerConnection(_server, timeoutSeconds: 3f, maxRetries: 1);
    }

    private void OnEnable()
    {
        tapAnywhereButton.onClick.AddListener(OnTap);
    }
    private void OnDisable()
    {
        tapAnywhereButton.onClick.RemoveListener(OnTap);
    }

    private async void Start()
    {
        if (playerCardView)
        {
            playerCardView.gameObject.SetActive(false);
        }

        if (opponentCardView)
        {
            opponentCardView.gameObject.SetActive(false);
        }

        await UniTask.NextFrame();
        UpdateCounts(_server.PlayerCount, _server.OpponentCount);
        warBanner.SetActive(false);
    }

    private async void OnTap()
    {
        if (_busy) return;
        _busy = true;
        tapAnywhereButton.interactable = false;

        try
        {
            playerCardView.gameObject.SetActive(true);
            opponentCardView.gameObject.SetActive(true);
            playerCardView.ShowBack();
            opponentCardView.ShowBack();

            var res = await _connection.DrawRoundWithRetryAsync(msg => ShowToast(msg));

            var p = res.Steps.Find(s => s.IsPlayer && s.Visibility == StepVisibility.FaceUp);
            var o = res.Steps.Find(s => !s.IsPlayer && s.Visibility == StepVisibility.FaceUp);

            playerCardView.SetCard(p.Card, false);
            opponentCardView.SetCard(o.Card, false);

            if (backHoldSeconds > 0f)
                await UniTask.Delay((int)(backHoldSeconds * 1000));

            await UniTask.WhenAll(
                playerCardView.FlipTo(true, flipDuration),
                opponentCardView.FlipTo(true, flipDuration)
            );

            await PlayRound(res);
            UpdateCounts(res.PlayerCountAfter, res.OpponentCountAfter);

            if (res.GameOver)
            {
                Signals.OnGameOver(res.Outcome);
                ShowToast(res.Outcome switch
                {
                    TurnOutcome.PlayerWins => "Game Over: You win!",
                    TurnOutcome.OpponentWins => "Game Over: You lose…",
                    _ => "Game Over: Draw"
                });
            }
        }
        catch (System.Exception ex)
        {
            ShowToast($"Failed: {ex.Message}");
            Debug.Log($"Failed: {ex.Message}");

            await Cysharp.Threading.Tasks.UniTask.Delay(1500);

            HideToastAndCards();
        }
        finally
        {
            _busy = false;
            tapAnywhereButton.interactable = true;
        }
    }

    private async UniTask PlayRound(RoundResolution res)
    {
        RoundStep lastUpPlayer = null;
        RoundStep lastUpOpponent = null;

        foreach (var step in res.Steps)
        {
            var view = step.IsPlayer ? playerCardView : opponentCardView;
            view.SetCard(step.Card, step.Visibility == StepVisibility.FaceUp);

            if (step.Visibility == StepVisibility.FaceUp)
            {
                if (step.IsPlayer) lastUpPlayer = step; else lastUpOpponent = step;

                if (lastUpPlayer != null && lastUpOpponent != null)
                {
                    int cmp = ((int)lastUpPlayer.Card.Rank).CompareTo((int)lastUpOpponent.Card.Rank);
                    if (cmp == 0)
                    {
                        // ---- WAR! ----
                        await ShowWarMoment();
                    }
                    else
                    {
                        await UniTask.Delay((int)(faceUpHoldSeconds * 1000));
                    }

                    lastUpPlayer = null;
                    lastUpOpponent = null;
                }
            }
            else
            {
                await UniTask.Delay((int)(faceDownPauseSeconds * 1000));
            }
        }

        ShowToast(res.Outcome switch
        {
            TurnOutcome.PlayerWins => "You Won!",
            TurnOutcome.OpponentWins => "Opponent Won!",
            _ => "Draw / WAR resolved"
        });

        Signals.OnRoundResolved(res);
        await UniTask.Delay(600);

        Signals.OnAfterRoundResolved(res);

        await UniTask.Delay(400);
        HideToastAndCards();

    }


    private async UniTask ShowWarMoment()
    {
        Signals.OnWarMoment();
        if (warBanner != null)
        {
            warBanner.SetActive(true);
        }

        await UniTask.Delay((int)(warPauseSeconds * 1000));

        if (warBanner != null)
        {
            warBanner.SetActive(false);
        }
    }

    private void UpdateCounts(int p, int o)
    {
        playerCountText.text = p.ToString();
        opponentCountText.text = o.ToString();
    }

    private void HideToastAndCards()
    {
        if (toastText)
        {
            toastText.gameObject.SetActive(false);
        }

        if (playerCardView)
        {
            playerCardView.gameObject.SetActive(false);
        }

        if (opponentCardView)
        {
            opponentCardView.gameObject.SetActive(false);
        }
    }

    private void ShowToast(string m)
    {
        if (toastText)
        {
            toastText.gameObject.SetActive(true);
            toastText.text = m;
        }
    }
}
