using MelonLoader;
using UnityEngine;

namespace GregModNoEOL;

internal static class NoEolOverlay
{
    private static bool _isVisible;
    private static Rect _windowRect;
    private static bool _firstOpen = true;

    private static Texture2D _texBackdrop;
    private static Texture2D _texSidebar;
    private static Texture2D _texCard;
    private static Texture2D _texPrimaryBtn;
    private static Texture2D _texPrimaryBtnHover;
    private static Texture2D _texMutedBtn;
    private static Texture2D _texMutedBtnHover;
    private static Texture2D _texToggleOn;
    private static Texture2D _texToggleOff;
    private static Texture2D _texBorder;
    private static Texture2D _texModalDim;
    private static Texture2D _texWhite;
    private static bool _texturesReady;
    private static bool _stylesReady;

    private static GUIStyle _stWindowTitle;
    private static GUIStyle _stSectionTitle;
    private static GUIStyle _stFormLabel;
    private static GUIStyle _stMuted;
    private static GUIStyle _stHint;
    private static GUIStyle _stPrimaryBtn;
    private static GUIStyle _stMutedBtn;
    private static GUIStyle _stToggleOn;
    private static GUIStyle _stToggleOff;
    private static GUIStyle _stCard;
    private static GUIStyle _stBackdrop;
    private static GUIStyle _stModalBlocker;

    private static MelonPreferences_Entry<bool> _prefDisableSwitchEol;
    private static MelonPreferences_Entry<bool> _prefDisableServerEol;
    private static MelonPreferences_Entry<bool> _prefAutoRepairSwitches;
    private static MelonPreferences_Entry<bool> _prefAutoRepairServers;

    private static readonly Color ColBackdrop = new(10f / 255f, 12f / 255f, 16f / 255f, 1f);
    private static readonly Color ColSidebar = new(24f / 255f, 30f / 255f, 40f / 255f, 1f);
    private static readonly Color ColCard = new(30f / 255f, 36f / 255f, 46f / 255f, 1f);
    private static readonly Color ColBorder = new(40f / 255f, 48f / 255f, 60f / 255f, 1f);
    private static readonly Color ColPrimaryBtn = new(0f / 255f, 133f / 255f, 120f / 255f, 1f);
    private static readonly Color ColPrimaryBtnHover = new(0f / 255f, 152f / 255f, 136f / 255f, 1f);
    private static readonly Color ColMutedBtn = new(48f / 255f, 55f / 255f, 68f / 255f, 1f);
    private static readonly Color ColMutedBtnHover = new(58f / 255f, 66f / 255f, 82f / 255f, 1f);
    private static readonly Color ColToggleOn = new(0f / 255f, 180f / 255f, 160f / 255f, 1f);
    private static readonly Color ColToggleOff = new(120f / 255f, 50f / 255f, 50f / 255f, 1f);
    private static readonly Color ColTitle = new(248f / 255f, 250f / 255f, 252f / 255f, 1f);
    private static readonly Color ColSection = new(226f / 255f, 232f / 255f, 240f / 255f, 1f);
    private static readonly Color ColFormLabel = new(200f / 255f, 208f / 255f, 218f / 255f, 1f);
    private static readonly Color ColMuted = new(154f / 255f, 164f / 255f, 178f / 255f, 1f);
    private static readonly Color ColHint = new(130f / 255f, 170f / 255f, 255f / 255f, 1f);
    private static readonly Color ColTealAccent = new(80f / 255f, 220f / 255f, 210f / 255f, 1f);

    private const float WindowW = 420f;
    private const float WindowH = 380f;

    public static bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;

            if (_isVisible)
            {
                if (_firstOpen)
                {
                    _windowRect = new Rect(
                        (Screen.width - WindowW) * 0.5f,
                        (Screen.height - WindowH) * 0.5f,
                        WindowW, WindowH);
                    _firstOpen = false;
                }
            }
        }
    }

    public static void Init(
        MelonPreferences_Entry<bool> disableSwitchEol,
        MelonPreferences_Entry<bool> disableServerEol,
        MelonPreferences_Entry<bool> autoRepairSwitches,
        MelonPreferences_Entry<bool> autoRepairServers)
    {
        _prefDisableSwitchEol = disableSwitchEol;
        _prefDisableServerEol = disableServerEol;
        _prefAutoRepairSwitches = autoRepairSwitches;
        _prefAutoRepairServers = autoRepairServers;
    }

    public static void Draw()
    {
        if (!_isVisible) return;

        EnsureTextures();
        EnsureStyles();

        GUI.depth = 32000;

        var fullScreen = new Rect(0, 0, Screen.width, Screen.height);
        GUI.Box(fullScreen, "", _stModalBlocker);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _windowRect = GUI.Window(9002, _windowRect, (GUI.WindowFunction)DrawWindow, "", _stBackdrop);
    }

    private static void DrawWindow(int id)
    {
        var r = new Rect(0, 0, _windowRect.width, _windowRect.height);

        GUI.DrawTexture(r, _texSidebar);

        var titleBarH = 52f;
        GUI.DrawTexture(new Rect(0, 0, r.width, titleBarH), _texBackdrop);

        GUI.Label(new Rect(16, 12, r.width - 80, 28), "NoEOL . Data Center", _stWindowTitle);

        var closeBtnRect = new Rect(r.width - 40, 10, 28, 28);
        if (GUI.Button(closeBtnRect, "✕", _stMutedBtn))
            _isVisible = false;

        DrawBorder(new Rect(0, 0, r.width, r.height), _texBorder);

        var contentY = titleBarH + 8f;
        var pad = 20f;
        var contentW = r.width - pad * 2;

        GUI.Label(new Rect(pad, contentY, contentW, 22), "Configuration", _stSectionTitle);
        contentY += 30f;

        GUI.Label(new Rect(pad, contentY, contentW, 18), "Toggle features on/off. Changes apply immediately.", _stMuted);
        contentY += 28f;

        contentY = DrawToggleCard(pad, contentY, contentW,
            "Disable Switches EOL",
            "Prevents switches from reaching end-of-life. Restores default value every frame.",
            _prefDisableSwitchEol);

        contentY = DrawToggleCard(pad, contentY, contentW,
            "Disable Servers EOL",
            "Prevents servers from reaching end-of-life. Restores default value every frame.",
            _prefDisableServerEol);

        contentY = DrawToggleCard(pad, contentY, contentW,
            "Auto Repair Broken Switches",
            "Automatically repairs all broken switches every frame.",
            _prefAutoRepairSwitches);

        contentY = DrawToggleCard(pad, contentY, contentW,
            "Auto Repair Broken Servers",
            "Automatically repairs all broken servers every frame.",
            _prefAutoRepairServers);

        contentY += 8f;

        var btnW = 120f;
        var btnX = r.width - pad - btnW;
        if (GUI.Button(new Rect(btnX, contentY, btnW, 32), "Close", _stMutedBtn))
            _isVisible = false;

        GUI.DragWindow(new Rect(0, 0, r.width, titleBarH));
    }

    private static float DrawToggleCard(float x, float y, float w, string title, string description, MelonPreferences_Entry<bool> pref)
    {
        var cardH = 62f;
        var cardRect = new Rect(x, y, w, cardH);
        GUI.DrawTexture(cardRect, _texCard);
        DrawBorder(cardRect, _texBorder);

        var isOn = pref.Value;
        var toggleStyle = isOn ? _stToggleOn : _stToggleOff;
        var toggleText = isOn ? "ON" : "OFF";
        var toggleW = 52f;
        var toggleH = 26f;
        var toggleRect = new Rect(x + w - toggleW - 12, y + (cardH - toggleH) * 0.5f, toggleW, toggleH);

        if (GUI.Button(toggleRect, toggleText, toggleStyle))
        {
            pref.Value = !pref.Value;
            MelonPreferences.Save();
            ModReleaseLog.ConfigEvent($"{pref.DisplayName} = {pref.Value}");
        }

        var textX = x + 14;
        var textW = w - toggleW - 40;
        GUI.Label(new Rect(textX, y + 10, textW, 20), title, _stFormLabel);
        GUI.Label(new Rect(textX, y + 30, textW, 28), description, _stMuted);

        return y + cardH + 8f;
    }

    private static void DrawBorder(Rect r, Texture2D tex)
    {
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, 1), tex);
        GUI.DrawTexture(new Rect(r.x, r.yMax - 1, r.width, 1), tex);
        GUI.DrawTexture(new Rect(r.x, r.y, 1, r.height), tex);
        GUI.DrawTexture(new Rect(r.xMax - 1, r.y, 1, r.height), tex);
    }

    private static void EnsureTextures()
    {
        if (_texturesReady) return;
        _texturesReady = true;

        _texBackdrop = MakeTex(ColBackdrop);
        _texSidebar = MakeTex(ColSidebar);
        _texCard = MakeTex(ColCard);
        _texPrimaryBtn = MakeTex(ColPrimaryBtn);
        _texPrimaryBtnHover = MakeTex(ColPrimaryBtnHover);
        _texMutedBtn = MakeTex(ColMutedBtn);
        _texMutedBtnHover = MakeTex(ColMutedBtnHover);
        _texToggleOn = MakeTex(ColToggleOn);
        _texToggleOff = MakeTex(ColToggleOff);
        _texBorder = MakeTex(ColBorder);
        _texModalDim = MakeTex(new Color(0, 0, 0, 140f / 255f));
        _texWhite = MakeTex(Color.white);
    }

    private static Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        UnityEngine.Object.DontDestroyOnLoad(t);
        return t;
    }

    private static void EnsureStyles()
    {
        if (_stylesReady) return;
        _stylesReady = true;

        _stBackdrop = new GUIStyle { normal = { background = _texBackdrop } };

        _stModalBlocker = new GUIStyle { normal = { background = _texModalDim } };

        _stWindowTitle = new GUIStyle
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = ColTitle }
        };

        _stSectionTitle = new GUIStyle
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.LowerLeft,
            normal = { textColor = ColSection }
        };

        _stFormLabel = new GUIStyle
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = ColFormLabel }
        };

        _stMuted = new GUIStyle
        {
            fontSize = 11,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft,
            normal = { textColor = ColMuted }
        };

        _stHint = new GUIStyle
        {
            fontSize = 12,
            normal = { textColor = ColHint }
        };

        _stPrimaryBtn = new GUIStyle
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white, background = _texPrimaryBtn },
            hover = { background = _texPrimaryBtnHover }
        };

        _stMutedBtn = new GUIStyle
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(230f / 255f, 234f / 255f, 240f / 255f, 1f), background = _texMutedBtn },
            hover = { background = _texMutedBtnHover }
        };

        _stToggleOn = new GUIStyle
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white, background = _texToggleOn }
        };

        _stToggleOff = new GUIStyle
        {
            fontSize = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(220f / 255f, 180f / 255f, 180f / 255f, 1f), background = _texToggleOff }
        };

        _stCard = new GUIStyle { normal = { background = _texCard } };
    }
}
