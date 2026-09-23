using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu"){
            SetupCreditsUI();
        } else if (sceneName == "GameOver"){
            SetupGameOverStatsUI();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11) || ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))){
            UIController.ToggleDisplayMode();
        }
    }

    private void SetupCreditsUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        TMP_FontAsset font = null;
        TMP_Text existingTmp = canvas.GetComponentInChildren<TMP_Text>();
        if (existingTmp != null) font = existingTmp.font;

        GameObject creditGo = new GameObject("CreditsSection");
        creditGo.transform.SetParent(canvas.transform, false);
        RectTransform rt = creditGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(40f, 35f);
        rt.sizeDelta = new Vector2(460f, 60f);

        Image bg = creditGo.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.08f, 0.18f, 0.85f);
        bg.raycastTarget = false;

        GameObject textGo = new GameObject("CreditsText");
        textGo.transform.SetParent(creditGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 6f);
        textRt.offsetMax = new Vector2(-16f, -6f);

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSize = 22f;
        tmp.lineSpacing = 10f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.text = "<color=#94A3B8>CREDITS</color>\n<b><color=#FFFFFF>Assets by Code Liboratory</color></b>";
    }

    private void SetupGameOverStatsUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        TMP_FontAsset font = null;
        TMP_Text existingTmp = canvas.GetComponentInChildren<TMP_Text>();
        if (existingTmp != null) font = existingTmp.font;

        float finalTime = PlayerPrefs.GetFloat("FinalSurvivalTime", 0f);
        float finalDist = PlayerPrefs.GetFloat("FinalDistance", 0f);
        int finalScore = PlayerPrefs.GetInt("FinalScore", 0);

        int minutes = Mathf.FloorToInt(finalTime / 60f);
        int seconds = Mathf.FloorToInt(finalTime % 60f);

        string rank;
        if (finalScore >= 40000 || finalTime >= 180f) rank = "<color=#F43F5E>RANK S+ (APEX)</color>";
        else if (finalScore >= 25000 || finalTime >= 120f) rank = "<color=#FDE047>RANK S</color>";
        else if (finalScore >= 15000 || finalTime >= 75f) rank = "<color=#38BDF8>RANK A</color>";
        else if (finalScore >= 8000 || finalTime >= 40f) rank = "<color=#4ADE80>RANK B</color>";
        else rank = "<color=#94A3B8>RANK C</color>";

        GameObject statsGo = new GameObject("GameOverStatsCard");
        statsGo.transform.SetParent(canvas.transform, false);
        RectTransform rt = statsGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 40f);
        rt.sizeDelta = new Vector2(580f, 75f);

        Image bg = statsGo.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.08f, 0.18f, 0.92f);
        bg.raycastTarget = false;

        GameObject textGo = new GameObject("StatsText");
        textGo.transform.SetParent(statsGo.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(16f, 4f);
        textRt.offsetMax = new Vector2(-16f, -4f);

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14f;
        tmp.fontSizeMax = 22f;
        tmp.fontSize = 20f;
        tmp.lineSpacing = 8f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        tmp.text = string.Format("<b><color=#94A3B8>TIME</color>  <color=#FFFFFF>{0:00}:{1:00}</color>      <color=#94A3B8>DISTANCE</color>  <color=#38BDF8>{2:N0} KM</color></b>\n<b><color=#94A3B8>SCORE</color>  <color=#FFFFFF>{3:N0}</color>      {4}</b>",
            minutes, seconds, finalDist, finalScore, rank);

        RepositionGameOverElements(canvas);
        StartCoroutine(LateReposition(canvas));
    }

    private void RepositionGameOverElements(Canvas canvas)
    {
        if (canvas == null) return;

        // Reposition direct children of Canvas (titles and stats card)
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            Transform child = canvas.transform.GetChild(i);
            string n = child.gameObject.name.ToLowerInvariant();
            RectTransform crt = child.GetComponent<RectTransform>();
            if (crt == null) continue;

            if (n == "title")
            {
                crt.anchoredPosition = new Vector2(0f, 320f);
            }
            else if (n == "title (1)")
            {
                crt.anchoredPosition = new Vector2(0f, 185f);
            }
            else if (n == "gameoverstatscard")
            {
                crt.anchoredPosition = new Vector2(0f, 40f);
                crt.sizeDelta = new Vector2(580f, 75f);
            }
        }

        // Reposition buttons and ensure their child text labels stay centered inside the button pills
        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            if (btn == null) continue;

            TMP_Text bTmp = btn.GetComponentInChildren<TMP_Text>();
            if (bTmp != null)
            {
                RectTransform btrt = bTmp.rectTransform;
                btrt.anchorMin = Vector2.zero;
                btrt.anchorMax = Vector2.one;
                btrt.offsetMin = Vector2.zero;
                btrt.offsetMax = Vector2.zero;
                btrt.anchoredPosition = Vector2.zero;
            }

            RectTransform brt = btn.GetComponent<RectTransform>();
            if (brt == null) continue;

            string bName = btn.gameObject.name.ToLowerInvariant();
            string bText = bTmp != null ? bTmp.text.Trim().ToLowerInvariant() : "";

            if (bName.Contains("(1)") || bText.Contains("quit"))
            {
                brt.anchoredPosition = new Vector2(0f, -220f);
            }
            else
            {
                brt.anchoredPosition = new Vector2(0f, -100f);
            }
        }
    }

    private System.Collections.IEnumerator LateReposition(Canvas canvas)
    {
        yield return null;
        RepositionGameOverElements(canvas);
        yield return new WaitForSecondsRealtime(0.08f);
        RepositionGameOverElements(canvas);
    }

    public void NewGame(){
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame(){
        Application.Quit();
    }
}
