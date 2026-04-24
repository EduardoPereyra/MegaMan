using UnityEngine;
using System;
using TMPro;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    bool calledNextScene;
    bool inputDetected = false;
    int alphaKeyPressText = 255;
    TextMeshProUGUI tmpTitleText;
    public AudioClip keyPressClip;
    private enum TitleScreenStates
    {
        WaitForInput,
        NextScene
    };
    private TitleScreenStates titleScreenState = TitleScreenStates.WaitForInput;

    string insertKeyPressText = "PRESS ANY KEY";

    string titleText =
@"<font=""megaman_2""><size=18><color=#FFFFFF{0:X2}>{1}</color></size></font>";

    void Awake()
    {
        tmpTitleText = GameObject.Find("TitleText").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        tmpTitleText.alignment = TextAlignmentOptions.Center;
        tmpTitleText.alignment = TextAlignmentOptions.Midline;
        tmpTitleText.fontStyle = FontStyles.UpperCase;

        titleScreenState = TitleScreenStates.WaitForInput;
    }

    void Update()
    {
        switch (titleScreenState)
        {
            case TitleScreenStates.WaitForInput:
                tmpTitleText.text = string.Format(titleText, alphaKeyPressText, insertKeyPressText);
                if (Input.anyKey && !inputDetected)
                {
                    inputDetected = true;
                    StartCoroutine(FlashTitleText());
                    SoundManager.Instance.Play(keyPressClip);
                }
                break;
            case TitleScreenStates.NextScene:
                if (!calledNextScene)
                {
                    GameManager.Instance.StartNextScene(GameManager.GameScenes.IntroScene);
                    calledNextScene = true;
                }
                break;
        }
    }

    private IEnumerator FlashTitleText()
    {
        for (int i = 0; i < 5; i++)
        {
            alphaKeyPressText = 0;
            yield return new WaitForSeconds(0.1f);
            alphaKeyPressText = 255;
            yield return new WaitForSeconds(0.1f);
        }
        alphaKeyPressText = 0;
        yield return new WaitForSeconds(0.1f);
        titleScreenState = TitleScreenStates.NextScene;
    }
}
