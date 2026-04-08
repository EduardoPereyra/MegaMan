using System.Collections;
using TMPro;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    TextMeshProUGUI screenMessageText;
    public AudioClip musicClip;

    void Awake()
    {
        screenMessageText = GameObject.Find("ScreenMessage").GetComponent<TextMeshProUGUI>();
    }

    public void CheckpointReached()
    {
        StartCoroutine(CoCheckpointReached());
    }

    private IEnumerator CoCheckpointReached()
    {
        screenMessageText.alignment = TextAlignmentOptions.Center;
        screenMessageText.alignment = TextAlignmentOptions.Top;
        screenMessageText.fontStyle = FontStyles.UpperCase;
        screenMessageText.fontSize = 24;
        screenMessageText.text = "CHECKPOINT REACHED";
        yield return new WaitForSeconds(5f);
        screenMessageText.text = "";
    }
}
