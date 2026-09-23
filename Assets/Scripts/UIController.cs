using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    public GameObject pausePanel;


    [Header("Time & Objectives")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject objectivesPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;

    [Header("Arcade Competition HUD")]
    [SerializeField] private TMP_Text scoreComboText;

    [Header("Boss UI")]
    [SerializeField] private Slider bossSlider;
    [SerializeField] private TMP_Text bossText;
    [SerializeField] private GameObject bossContainer;

    [Header("Screen Mode Toggle")]
    private Button screenModeButton;
    private TMP_Text screenModeButtonText;

    [Header("Pause Blur Effect")]
    private GameObject blurBackdrop;
    private RawImage blurRawImage;
    private RenderTexture blurTexture;

    private float tutorialTimer = 15f;
    private const float FADE_DURATION = 3f;
    private string currentBossName = "BOSS";

    void Awake(){
        if (Instance != null){
            Destroy(gameObject);
        } else {
            Instance = this;
        }
    }

    void Start(){
        SetupSliderLabels();
        SetupTutorialUI();
        SetupTimerUI();
        SetupScoreComboUI();
        SetupBossUI();
        SetupPauseBlurBackdrop();
        SetupPauseScreenButton();
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.F11) || ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.Return))){
            ToggleDisplayMode();
        }

        if (tutorialTimer > 0){
            tutorialTimer -= Time.deltaTime;
            if (tutorialCanvasGroup != null){
                if (tutorialTimer <= FADE_DURATION){
                    tutorialCanvasGroup.alpha = Mathf.Clamp01(tutorialTimer / FADE_DURATION);
                } else {
                    tutorialCanvasGroup.alpha = 1f;
                }
            }
            if (tutorialTimer <= 0){
                if (objectivesPanel != null) objectivesPanel.SetActive(false);
                if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
                if (tutorialCanvasGroup != null) tutorialCanvasGroup.gameObject.SetActive(false);
            }
        }
    }

    private void SetupSliderLabels(){
        // Configure inside energy text for crisp white readability
        if (energyText != null){
            energyText.fontSize = 24f;
            energyText.color = Color.white;
            energyText.alignment = TextAlignmentOptions.Center;
            RectTransform rt = energyText.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(380f, 50f);
            if (energySlider != null){
                energyText.text = string.Format("<b>ENERGY: {0} / {1}</b>", energySlider.value, energySlider.maxValue);
            }
        }

        // Configure inside health text for crisp white readability
        if (healthText != null){
            healthText.fontSize = 24f;
            healthText.color = Color.white;
            healthText.alignment = TextAlignmentOptions.Center;
            RectTransform rt = healthText.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(380f, 50f);
            if (healthSlider != null){
                healthText.text = string.Format("<b>HEALTH: {0} / {1}</b>", healthSlider.value, healthSlider.maxValue);
            }
        }
    }

    private void SetupTutorialUI(){
        TMP_FontAsset font = energyText != null ? energyText.font : null;

        GameObject tutorialRoot = new GameObject("TutorialGroup");
        tutorialRoot.transform.SetParent(transform, false);
        RectTransform rootRect = tutorialRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        tutorialCanvasGroup = tutorialRoot.AddComponent<CanvasGroup>();

        // 1. Objectives Panel at Top-Left (Below Energy Slider, Y=-115, non-overlapping)
        objectivesPanel = new GameObject("ObjectivesPanel");
        objectivesPanel.transform.SetParent(tutorialRoot.transform, false);
        RectTransform objRect = objectivesPanel.AddComponent<RectTransform>();
        objRect.anchorMin = new Vector2(0f, 1f);
        objRect.anchorMax = new Vector2(0f, 1f);
        objRect.pivot = new Vector2(0f, 1f);
        objRect.anchoredPosition = new Vector2(40f, -115f);
        objRect.sizeDelta = new Vector2(500f, 140f);

        Image objBg = objectivesPanel.AddComponent<Image>();
        objBg.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
        objBg.raycastTarget = false;

        GameObject objTextGo = new GameObject("ObjectivesText");
        objTextGo.transform.SetParent(objectivesPanel.transform, false);
        RectTransform objTextRect = objTextGo.AddComponent<RectTransform>();
        objTextRect.anchorMin = Vector2.zero;
        objTextRect.anchorMax = Vector2.one;
        objTextRect.offsetMin = new Vector2(16f, 10f);
        objTextRect.offsetMax = new Vector2(-16f, -10f);

        TextMeshProUGUI objTmp = objTextGo.AddComponent<TextMeshProUGUI>();
        if (font != null) objTmp.font = font;
        objTmp.fontSize = 22f;
        objTmp.lineSpacing = 14f;
        objTmp.color = Color.white;
        objTmp.raycastTarget = false;
        objTmp.text = "<color=#FDE047><b>MISSION OBJECTIVES</b></color> <color=#94A3B8>[ENDLESS SURVIVAL]</color>\n" +
                      "• <color=#FFFFFF>SURVIVE THE INFINITE VOID MARATHON</color>\n" +
                      "• <color=#FFFFFF>DESTROY RECURRING ESCALATING BOSSES</color>\n" +
                      "• <color=#FFFFFF>CONTINUE UNTIL YOUR SHIP IS DESTROYED</color>";

        // 2. How to Play Panel at Bottom-Left (X=40, Y=35, non-overlapping)
        howToPlayPanel = new GameObject("HowToPlayPanel");
        howToPlayPanel.transform.SetParent(tutorialRoot.transform, false);
        RectTransform htpRect = howToPlayPanel.AddComponent<RectTransform>();
        htpRect.anchorMin = new Vector2(0f, 0f);
        htpRect.anchorMax = new Vector2(0f, 0f);
        htpRect.pivot = new Vector2(0f, 0f);
        htpRect.anchoredPosition = new Vector2(40f, 35f);
        htpRect.sizeDelta = new Vector2(480f, 165f);

        Image htpBg = howToPlayPanel.AddComponent<Image>();
        htpBg.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
        htpBg.raycastTarget = false;

        GameObject htpTextGo = new GameObject("HowToPlayText");
        htpTextGo.transform.SetParent(howToPlayPanel.transform, false);
        RectTransform htpTextRect = htpTextGo.AddComponent<RectTransform>();
        htpTextRect.anchorMin = Vector2.zero;
        htpTextRect.anchorMax = Vector2.one;
        htpTextRect.offsetMin = new Vector2(16f, 10f);
        htpTextRect.offsetMax = new Vector2(-16f, -10f);

        TextMeshProUGUI htpTmp = htpTextGo.AddComponent<TextMeshProUGUI>();
        if (font != null) htpTmp.font = font;
        htpTmp.fontSize = 21f;
        htpTmp.lineSpacing = 11f;
        htpTmp.color = Color.white;
        htpTmp.raycastTarget = false;
        htpTmp.text = "<color=#FDE047><b>TACTICAL CONTROLS</b></color>\n" +
                      "<color=#FFFFFF>SHOOT : LEFT MOUSE / ALT</color>\n" +
                      "<color=#FFFFFF>BOOST : RIGHT MOUSE / SPACE</color>\n" +
                      "<color=#FFFFFF>MOVE  : WSAD / ARROW KEYS</color>\n" +
                      "<color=#38BDF8><b>EMP   : [E] KEY (CLEAR BULLETS)</b></color>\n" +
                      "<color=#FDE047><b>SCREEN: [F11] (FULL / WINDOWED)</b></color>";
    }

    private void SetupTimerUI(){
        if (timerText != null) return;
        TMP_FontAsset font = energyText != null ? energyText.font : null;

        GameObject timerGo = new GameObject("SurvivalTimer");
        timerGo.transform.SetParent(transform, false);
        RectTransform timerRect = timerGo.AddComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.pivot = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0f, -20f);
        timerRect.sizeDelta = new Vector2(460f, 48f);

        Image timerBg = timerGo.AddComponent<Image>();
        timerBg.color = new Color(0.04f, 0.08f, 0.16f, 0.9f);
        timerBg.raycastTarget = false;

        GameObject timerTextGo = new GameObject("TimerText");
        timerTextGo.transform.SetParent(timerGo.transform, false);
        RectTransform tTextRect = timerTextGo.AddComponent<RectTransform>();
        tTextRect.anchorMin = Vector2.zero;
        tTextRect.anchorMax = Vector2.one;
        tTextRect.offsetMin = Vector2.zero;
        tTextRect.offsetMax = Vector2.zero;

        timerText = timerTextGo.AddComponent<TextMeshProUGUI>();
        if (font != null) timerText.font = font;
        timerText.fontSize = 26f;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = Color.white;
        timerText.raycastTarget = false;
        timerText.text = "<b><color=#94A3B8>TIME</color> <color=#FFFFFF>00:00</color>   <color=#94A3B8>DIST</color> <color=#38BDF8>0 KM</color></b>";
    }

    private void SetupScoreComboUI(){
        if (scoreComboText != null) return;
        TMP_FontAsset font = energyText != null ? energyText.font : null;

        // Positioned at BOTTOM-RIGHT: Zero overlap with Top-Right Health Slider!
        GameObject scGo = new GameObject("ScoreComboHUD");
        scGo.transform.SetParent(transform, false);
        RectTransform scRect = scGo.AddComponent<RectTransform>();
        scRect.anchorMin = new Vector2(1f, 0f);
        scRect.anchorMax = new Vector2(1f, 0f);
        scRect.pivot = new Vector2(1f, 0f);
        scRect.anchoredPosition = new Vector2(-40f, 35f);
        scRect.sizeDelta = new Vector2(320f, 75f);

        Image scBg = scGo.AddComponent<Image>();
        scBg.color = new Color(0.04f, 0.08f, 0.16f, 0.92f);
        scBg.raycastTarget = false;

        GameObject textGo = new GameObject("ScoreComboText");
        textGo.transform.SetParent(scGo.transform, false);
        RectTransform tRect = textGo.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = new Vector2(14f, 6f);
        tRect.offsetMax = new Vector2(-14f, -6f);

        scoreComboText = textGo.AddComponent<TextMeshProUGUI>();
        if (font != null) scoreComboText.font = font;
        scoreComboText.fontSize = 22f;
        scoreComboText.alignment = TextAlignmentOptions.Right;
        scoreComboText.color = Color.white;
        scoreComboText.raycastTarget = false;
        scoreComboText.text = "<color=#94A3B8>SCORE</color>  <b><color=#FFFFFF>0</color></b>\n<color=#FDE047><b>RANK D</b></color>";
    }

    private void SetupBossUI(){
        if (healthSlider == null) return;

        bossContainer = Instantiate(healthSlider.gameObject, transform);
        bossContainer.name = "BossHealthContainer";
        RectTransform bRect = bossContainer.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 1f);
        bRect.anchorMax = new Vector2(0.5f, 1f);
        bRect.pivot = new Vector2(0.5f, 1f);
        bRect.anchoredPosition = new Vector2(0f, -75f);
        bRect.sizeDelta = new Vector2(700f, 46f);

        bossSlider = bossContainer.GetComponent<Slider>();
        if (bossSlider != null){
            bossSlider.interactable = false;
            Navigation nav = bossSlider.navigation;
            nav.mode = Navigation.Mode.None;
            bossSlider.navigation = nav;
        }

        bossText = bossContainer.GetComponentInChildren<TMP_Text>();

        Image[] images = bossContainer.GetComponentsInChildren<Image>();
        foreach (Image img in images){
            if (img.gameObject.name.Contains("Fill")){
                img.color = new Color(0.95f, 0.2f, 0.3f, 1f);
            }
        }

        if (bossText != null){
            bossText.text = "<b><color=#FDE047>PROTOCOL GUARDIAN</color></b>";
            bossText.alignment = TextAlignmentOptions.Center;
            bossText.color = Color.white;
            bossText.enableWordWrapping = false;
            bossText.textWrappingMode = TextWrappingModes.NoWrap;
            bossText.enableAutoSizing = true;
            bossText.fontSizeMin = 10f;
            bossText.fontSizeMax = 18f;
            bossText.overflowMode = TextOverflowModes.Ellipsis;
            RectTransform tRt = bossText.GetComponent<RectTransform>();
            if (tRt != null){
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = new Vector2(6f, 0f);
                tRt.offsetMax = new Vector2(-6f, 0f);
            }
        }

        bossContainer.SetActive(false);
    }

    private void SetupPauseBlurBackdrop(){
        if (pausePanel == null) return;

        blurBackdrop = new GameObject("PauseFrostedBlurBackdrop");
        blurBackdrop.transform.SetParent(pausePanel.transform, false);
        blurBackdrop.transform.SetAsFirstSibling();

        RectTransform rt = blurBackdrop.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-1500f, -1500f);
        rt.offsetMax = new Vector2(1500f, 1500f);

        blurRawImage = blurBackdrop.AddComponent<RawImage>();
        blurRawImage.color = new Color(0.04f, 0.08f, 0.18f, 0.90f);
        blurRawImage.raycastTarget = true;
    }

    private void SetupPauseScreenButton(){
        if (pausePanel == null || screenModeButton != null) return;
        TMP_FontAsset font = timerText != null ? timerText.font : (energyText != null ? energyText.font : null);

        GameObject btnGo = new GameObject("ScreenModeButton");
        btnGo.transform.SetParent(pausePanel.transform, false);
        RectTransform rt = btnGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -210f);
        rt.sizeDelta = new Vector2(380f, 44f);

        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.1f, 0.22f, 0.45f, 0.95f);

        screenModeButton = btnGo.AddComponent<Button>();
        ColorBlock cb = screenModeButton.colors;
        cb.highlightedColor = new Color(0.2f, 0.45f, 0.85f, 1f);
        cb.pressedColor = new Color(0.08f, 0.16f, 0.35f, 1f);
        screenModeButton.colors = cb;
        screenModeButton.onClick.AddListener(ToggleDisplayMode);

        GameObject txtGo = new GameObject("Text");
        txtGo.transform.SetParent(btnGo.transform, false);
        RectTransform txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        screenModeButtonText = txtGo.AddComponent<TextMeshProUGUI>();
        if (font != null) screenModeButtonText.font = font;
        screenModeButtonText.fontSize = 20f;
        screenModeButtonText.alignment = TextAlignmentOptions.Center;
        screenModeButtonText.color = Color.white;
        UpdateScreenButtonLabel();
    }

    public void UpdateScreenButtonLabel(){
        if (screenModeButtonText != null){
            screenModeButtonText.text = Screen.fullScreen 
                ? "<b>[F11] FULLSCREEN <color=#38BDF8>(CLICK: 720P)</color></b>" 
                : "<b>[F11] WINDOWED <color=#38BDF8>(CLICK: FULLSCREEN)</color></b>";
        }
    }

    public static void ToggleDisplayMode(){
        if (Screen.fullScreen){
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        } else {
            Resolution maxRes = Screen.currentResolution;
            Screen.SetResolution(maxRes.width, maxRes.height, FullScreenMode.FullScreenWindow);
        }
        if (Instance != null){
            Instance.Invoke("UpdateScreenButtonLabelDelayed", 0.15f);
        }
    }

    private void UpdateScreenButtonLabelDelayed(){
        UpdateScreenButtonLabel();
    }

    public void OnPauseStateChanged(bool isPaused){
        if (blurBackdrop == null) SetupPauseBlurBackdrop();
        if (screenModeButton == null) SetupPauseScreenButton();

        if (isPaused){
            UpdateScreenButtonLabel();
            if (blurRawImage != null){
                // Capture downscaled bilinear texture for authentic frosted glass blur
                // EXCLUDE Layer 5 (UI) so UI elements/PAUSED text are NOT rendered into the blur!
                if (Camera.main != null){
                    int w = Screen.width / 4;
                    int h = Screen.height / 4;
                    if (blurTexture != null) blurTexture.Release();
                    blurTexture = new RenderTexture(w, h, 0);
                    blurTexture.filterMode = FilterMode.Bilinear;
                    
                    int originalMask = Camera.main.cullingMask;
                    Camera.main.cullingMask = originalMask & ~(1 << 5); // Exclude UI layer (5)
                    RenderTexture prev = Camera.main.targetTexture;
                    Camera.main.targetTexture = blurTexture;
                    Camera.main.Render();
                    Camera.main.targetTexture = prev;
                    Camera.main.cullingMask = originalMask;

                    blurRawImage.texture = blurTexture;
                    blurRawImage.color = new Color(0.12f, 0.18f, 0.32f, 0.94f);
                }
            }
            if (blurBackdrop != null) blurBackdrop.SetActive(true);
        } else {
            if (blurBackdrop != null) blurBackdrop.SetActive(false);
            if (blurTexture != null){
                blurTexture.Release();
                blurTexture = null;
            }
        }
    }

    public void ShowBossBar(string bossName, float maxHealth){
        currentBossName = bossName;
        if (bossContainer != null){
            bossContainer.SetActive(true);
            if (bossSlider != null){
                bossSlider.maxValue = maxHealth;
                bossSlider.value = maxHealth;
            }
            if (bossText != null){
                bossText.enableWordWrapping = false;
                bossText.textWrappingMode = TextWrappingModes.NoWrap;
                bossText.enableAutoSizing = true;
                bossText.fontSizeMin = 10f;
                bossText.fontSizeMax = 18f;
                bossText.overflowMode = TextOverflowModes.Ellipsis;
                RectTransform tRt = bossText.GetComponent<RectTransform>();
                if (tRt != null){
                    tRt.anchorMin = Vector2.zero;
                    tRt.anchorMax = Vector2.one;
                    tRt.offsetMin = new Vector2(6f, 0f);
                    tRt.offsetMax = new Vector2(-6f, 0f);
                }
                bossText.text = string.Format("<b><color=#FDE047>{0}</color>  <color=#FFFFFF>{1} / {1}</color></b>", bossName, Mathf.RoundToInt(maxHealth));
            }
        }
    }

    public void UpdateBossHealth(float current, float max){
        if (bossSlider != null){
            bossSlider.maxValue = max;
            bossSlider.value = Mathf.RoundToInt(current);
            if (bossText != null){
                bossText.enableWordWrapping = false;
                bossText.textWrappingMode = TextWrappingModes.NoWrap;
                bossText.enableAutoSizing = true;
                bossText.fontSizeMin = 10f;
                bossText.fontSizeMax = 18f;
                bossText.overflowMode = TextOverflowModes.Ellipsis;
                bossText.text = string.Format("<b><color=#FDE047>{0}</color>  <color=#FFFFFF>{1} / {2}</color></b>", currentBossName, bossSlider.value, bossSlider.maxValue);
            }
        }
    }

    public void HideBossBar(){
        if (bossContainer != null){
            bossContainer.SetActive(false);
        }
    }

    public void UpdateSurvivalTime(float timeInSeconds, float distKm = 0f){
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        if (timerText != null){
            timerText.text = string.Format("<b><color=#94A3B8>TIME</color> <color=#FFFFFF>{0:00}:{1:00}</color>   <color=#94A3B8>DIST</color> <color=#38BDF8>{2:N0} KM</color></b>", minutes, seconds, distKm);
        }
    }

    public void UpdateScoreAndCombo(int score, int combo){
        if (scoreComboText == null) return;
        string rank;
        string rankColor;
        if (score >= 45000){ rank = "SSS"; rankColor = "#F59E0B"; }
        else if (score >= 25000){ rank = "S"; rankColor = "#FBBF24"; }
        else if (score >= 12000){ rank = "A"; rankColor = "#34D399"; }
        else if (score >= 5000){ rank = "B"; rankColor = "#60A5FA"; }
        else if (score >= 1500){ rank = "C"; rankColor = "#A78BFA"; }
        else { rank = "D"; rankColor = "#94A3B8"; }

        string comboStr = combo > 1 ? string.Format("  <color=#F59E0B><b>x{0} STREAK</b></color>", combo) : "";
        scoreComboText.text = string.Format("<color=#94A3B8>SCORE</color>  <b><color=#FFFFFF>{0:N0}</color></b>{1}\n<color={2}><b>RANK {3}</b></color>", score, comboStr, rankColor, rank);
    }

    public void UpdateEnergySlider(float current, float max){
        energySlider.maxValue = max;
        energySlider.value = Mathf.RoundToInt(current);
        energyText.text = string.Format("<b>ENERGY: {0} / {1}</b>", energySlider.value, energySlider.maxValue);
    }

    public void UpdateHealthSlider(float current, float max){
        healthSlider.maxValue = max;
        healthSlider.value = Mathf.RoundToInt(current);
        healthText.text = string.Format("<b>HEALTH: {0} / {1}</b>", healthSlider.value, healthSlider.maxValue);
    }
}
