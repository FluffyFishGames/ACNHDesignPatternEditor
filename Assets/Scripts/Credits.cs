using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    public AudioSource Music;
    public KKSlider KK;
    public UnityEngine.UI.Image Background;
    public CanvasGroup KKGroup;
    public CanvasGroup ShineGroup;
    public RectTransform Container;
    public RectTransform CreditsContainer;

    private float ShowPhase = 0f;
    private bool IsShowing = false;
    private bool FinishedShowing = false;
    private bool MoveLeft = false;
    private float LeftPhase = 0f;
    private float CreditsPhase = 0f;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        if (Controller.Instance.Popup.IsOpened)
            Controller.Instance.Popup.Close();
        if (!IsShowing)
        {
            MoveLeft = false;
            gameObject.SetActive(true);
            CreditsPhase = 0f;
            LeftPhase = 0f;
            CreditsContainer.anchoredPosition = new Vector2(0, 0);
            FinishedShowing = false;
            IsShowing = true;
        }
    }

    private void Update()
    {
        if (IsShowing && ShowPhase < 1f)
            ShowPhase = Mathf.Min(1f, ShowPhase + Time.deltaTime);
        if (!IsShowing && ShowPhase > 0f)
            ShowPhase = Mathf.Max(0f, ShowPhase - Time.deltaTime);

        if (MoveLeft && LeftPhase < 1f)
            LeftPhase = Mathf.Min(1f, LeftPhase + Time.deltaTime / 4f);

        Background.color = new Color(0, 0, 0, Mathf.Clamp01((ShowPhase) * 2f));
        float alpha = Mathf.Clamp01((ShowPhase - 0.5f) * 2f);
        KKGroup.alpha = alpha;
        ShineGroup.alpha = alpha;

        if (LeftPhase >= 1f)
        {
            if (CreditsPhase < 1f)
            {
                CreditsPhase = Mathf.Min(1f, CreditsPhase + Time.deltaTime / 160f);
                CreditsContainer.anchoredPosition = new Vector2(0f, CreditsPhase * 6000f);
            }
        }
        Container.anchoredPosition = new Vector2(0f - LeftPhase * 550f, 0f);

        if (IsShowing && !FinishedShowing && ShowPhase == 1f)
        {
            FinishedShowing = true;
            Controller.Instance.Popup.SetText("Hey.\r\nYou know. There are some real\r\nlovely people out there.", false, () =>
            {
                Controller.Instance.Popup.SetText("Thanks to all patrons for the love.\r\nHow about a song\r\nto bring up the beat?", false, () =>
                {
                    StartCoroutine(PlayCredits());
                    return true;
                });
                return false;
            });
        }
        if (!IsShowing && ShowPhase <= 0f)
            gameObject.SetActive(false);

    }
    IEnumerator PlayCredits()
    {
        KK.StopGuitar();
        KK.StopSing();

        yield return new WaitForSeconds(2f);
        Music.Play();

        yield return new WaitForSeconds(8f);
        KK.StartGuitar();
        MoveLeft = true;
        yield return new WaitForSeconds(8f);
        KK.StartSing();
        yield return new WaitForSeconds(23.3f); // 39.3
        KK.Awooo();
        yield return new WaitForSeconds(8.7f); // 48
        KK.StopSing();
        yield return new WaitForSeconds(4f); // 52
        KK.StartSing();
        yield return new WaitForSeconds(3.5f); // 55.5
        KK.StopSing();
        yield return new WaitForSeconds(0.6f); // 56.1
        KK.StartSing();
        yield return new WaitForSeconds(7.3f); // 63.4
        KK.StopSing();
        yield return new WaitForSeconds(4.4f); // 67.8
        KK.StartSing();
        yield return new WaitForSeconds(3.7f); // 71.5
        KK.StopSing();
        yield return new WaitForSeconds(0.6f); // 72.1
        KK.StartSing();
        yield return new WaitForSeconds(7.9f); // 80
        KK.StopSing();
        yield return new WaitForSeconds(16.2f); // 96.2
        KK.SlowSing();
        yield return new WaitForSeconds(12.1f); // 108.3
        //Music.time = 108.3f;
        yield return new WaitForSeconds(19.4f); // 127,7
        KK.Awooo();
        yield return new WaitForSeconds(12.3f); // 140
        KK.StopSing();
        yield return new WaitForSeconds(4.5f); // 144.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 146
        KK.StartGuitar();
        yield return new WaitForSeconds(0.5f); // 146.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 148
        KK.StartGuitar();
        yield return new WaitForSeconds(0.5f); // 148.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 150
        KK.StartGuitar();
        yield return new WaitForSeconds(0.5f); // 150.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 152
        KK.StartGuitar();
        yield return new WaitForSeconds(0.5f); // 152.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 154
        KK.StartGuitar();
        yield return new WaitForSeconds(0.5f); // 154.5
        KK.StopGuitar();
        yield return new WaitForSeconds(1.5f); // 156
        KK.StartGuitar();
        yield return new WaitForSeconds(0.25f); // 156.5
        KK.StopGuitar();
        yield return new WaitForSeconds(8f);
        IsShowing = false;
    }
}
