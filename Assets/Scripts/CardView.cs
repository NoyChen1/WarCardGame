using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class CardView : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _artwork;
    [SerializeField] private Sprite _backSprite;
    [SerializeField] private Sprite _frontSprite;
    [SerializeField] private TextMeshProUGUI _rankTextLU;
    [SerializeField] private TextMeshProUGUI _rankTextRB;

    private CardData _cardData;
    private bool _isFaceUp = true;

    public void SetCard(CardData data, bool faceUp = true)
    {
        _cardData = data;
        _isFaceUp = faceUp;

        if (faceUp)
        {
            _background.sprite = _frontSprite;
            _artwork.gameObject.SetActive(true);
            _artwork.sprite = data.Artwork;
            _rankTextLU.text = GetRankLabel(data.Rank);
            _rankTextRB.text = GetRankLabel(data.Rank);
            _rankTextLU.enabled = _rankTextRB.enabled = true;
        }
        else
        {
            _background.sprite = _backSprite;
            _artwork.gameObject.SetActive(false);
            _rankTextLU.enabled = _rankTextRB.enabled = false;
        }
    }

    public void ShowBack()
    {
        if(_backSprite != null)
        {
            _background.sprite = _backSprite;
            _artwork.gameObject.SetActive(false);
            _rankTextLU. enabled = _rankTextRB.enabled = false;
        }

        _isFaceUp = false;
    }
    public async UniTask MoveTo(Vector2 anchoredPos, float dur = 0.35f)
    {
        var rt = (RectTransform)transform;
        var tween = rt.DOAnchorPos(anchoredPos, dur).SetEase(Ease.InOutQuad);
        await tween.AsyncWaitForCompletion();
    }

    public async UniTask FlipTo(bool faceUp, float dur = 0.35f)
    {
        var t1 = transform.DORotate(new Vector3(0, 90, 0), dur / 2f, RotateMode.Fast);
        await t1.AsyncWaitForCompletion();

        SetCard(_cardData, faceUp);

        var t2 = transform.DORotate(Vector3.zero, dur / 2f, RotateMode.Fast);
        await t2.AsyncWaitForCompletion();
    }


    private string GetRankLabel(Rank rank)
    {
        int r = (int)rank;
        return r <= 10 ? r.ToString() : rank.ToString()[0].ToString(); // J/Q/K/A
    }

    public void ShowFace(bool faceUp)
    {
        if (_cardData == null)
        {
            return;
        }

        SetCard(_cardData, faceUp);
    }

    public CardData GetCardData() => _cardData;
}
