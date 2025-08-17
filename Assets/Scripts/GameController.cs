using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
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

    [Header("Slots")]
    [SerializeField] private RectTransform cardsLayer;
    [SerializeField] private RectTransform playerDeckSlot;
    [SerializeField] private RectTransform opponentDeckSlot;
    [SerializeField] private RectTransform tableLeftSlot;
    [SerializeField] private RectTransform tableRightSlot;

    private WarServer _server;
    private ServerConnection _connection;
    private bool _busy;


    private void Awake()
    {
        _server = new WarServer(serverConfig, cardDb.allCards);
        _connection = new ServerConnection(_server, timeoutSeconds: 3f, maxRetries: 1);
        UpdateCounts(_server.PlayerCount, _server.OpponentCount);
    }

    void OnEnable() => tapAnywhereButton.onClick.AddListener(OnTap);
    void OnDisable() => tapAnywhereButton.onClick.RemoveListener(OnTap);

    private async void Start()
    {
        await UniTask.NextFrame();
        playerCardView.ShowBack();
        opponentCardView.ShowBack();
    }
    private async void OnTap()
    {
        if (_busy)
        {
            return;
        }
        _busy = true;
        tapAnywhereButton.interactable = false;

        try
        {
            var res = await _connection.DrawRoundWithRetryAsync(msg => ShowToast(msg));
            await PlayRound(res);
            UpdateCounts(res.PlayerCountAfter, res.OpponentCountAfter);

            if (res.GameOver)
            {
                ShowToast(res.Outcome switch
                {
                    TurnOutcome.PlayerWins => "Game Over: You win!",
                    TurnOutcome.OpponentWins => "Game Over: You lose…",
                    _ => "Game Over: Draw"
                });
                return;
            }
        }
        catch (System.Exception ex)
        {
            ShowToast($"Falied: {ex.Message}");
            Debug.Log($"[GameController] Falied: {ex.Message}");
        }
        finally
        {
            _busy = false;
            tapAnywhereButton.interactable = true;
        }
    }


    private async UniTask PlayRound(RoundResolution res)
    {
        foreach (var step in res.Steps)
        {
            var view = step.IsPlayer ? playerCardView : opponentCardView;
            view.SetCard(step.Card, step.Visibility == StepVisibility.FaceUp);

            //TODO: move/flip
            await UniTask.Delay(120);
        }

        ShowToast(res.Outcome switch
        {
            TurnOutcome.PlayerWins => "You win the round!",
            TurnOutcome.OpponentWins => "Opponent wins the round!",
            _ => "Draw / WAR resolved"
        });
    }


    void PlaceAt(CardView v, RectTransform slot)
    {
        var rt = (RectTransform)v.transform;
        if (rt.parent == cardsLayer && slot.parent == cardsLayer)
            rt.anchoredPosition = slot.anchoredPosition;
        else
            rt.anchoredPosition = LocalPosIn(cardsLayer, slot);
    }

    Vector2 GetPos(RectTransform slot)
    {
        var rt = (RectTransform)playerCardView.transform;
        if (rt.parent == cardsLayer && slot.parent == cardsLayer) return slot.anchoredPosition;
        return LocalPosIn(cardsLayer, slot);
    }

    static Vector2 LocalPosIn(RectTransform targetParent, RectTransform source)
    {
        var screen = RectTransformUtility.WorldToScreenPoint(null, source.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screen, null, out var local);
        return local;
    }

    void UpdateCounts(int p, int o) 
    { 
        playerCountText.text = p.ToString(); 
        opponentCountText.text = o.ToString(); 
    }

    void ShowToast(string m)
    {
        if (toastText)
        {
            toastText.text = m;
        }
    }

}
