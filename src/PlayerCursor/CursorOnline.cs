using System;
using System.Collections.Generic;
using CasualtiesTogetherUtils;
using CUCoreLib.Helpers;
using ModOnlineCursor;
using TMPro;
using Together;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OnlineCursorProcess : MonoBehaviour
{
    private static GameObject gameObjectProcessor;
    public static bool isIgnorePVPInvsableCursor = false; // :)
    public static bool visable = true;
    public static void init()
    {
        if (isIgnorePVPInvsableCursor)
        {
            LoggerCursor.Logger.LogInfo("Ignore PVP Cursor Enabel");
        }
        SceneManager.activeSceneChanged += (Scene oldScene, Scene newScene) =>
        {
            if (newScene.name == "SampleScene")
            {
                if (Net.IsRunning && gameObjectProcessor == null)
                {
                    initalizerCursorSprite();
                    gameObjectProcessor = new GameObject("CurssorUi_Processor");
                    gameObjectProcessor.AddComponent<OnlineCursorProcess>();

                }
#if DEBUG
                else
                {
                    initalizerCursorSprite();
                    PlayerCamera.main.body.gameObject.AddComponent<OnlineCursorUi>().body = PlayerCamera.main.body;
                }
#endif
            }
            else
            {
                if (gameObjectProcessor != null) Destroy(gameObjectProcessor);
            }
        };
    }

    public static void initalizerCursorSprite()
    {
        var cursors = PlayerCamera.main.cursors;
        var textureNormalCursor = cursors[0];
        var normalCursor = Sprite.Create(
        textureNormalCursor,
        new Rect(0, 0, textureNormalCursor.width, textureNormalCursor.height),
        new Vector2(0.50f, 0.50f),
        15f
        );
        OnlineCursorUi.cursorNormalSprite = normalCursor;

        var texture_link2_or_alternative3 = cursors[1];
        var cursor_link2_or_alternative3 = Sprite.Create(
            texture_link2_or_alternative3,
            new Rect(0, 0, texture_link2_or_alternative3.width, texture_link2_or_alternative3.height),
            new Vector2(0.50f, 0.50f),
            15f
        );
        OnlineCursorUi.cursorLink2OrAlternative3Sprite = cursor_link2_or_alternative3;


        var texture_alternative = cursors[2];
        var cursor_alternative = Sprite.Create(
            texture_alternative,
            new Rect(0, 0, texture_alternative.width, texture_alternative.height),
            new Vector2(0.50f, 0.50f),
            15f
        );
        OnlineCursorUi.cursorAlternativeSprite = cursor_alternative;


        var texture_text_also_can_be_alternative = cursors[3];
        var cursor_text_also_can_be_alternative = Sprite.Create(
            texture_text_also_can_be_alternative,
            new Rect(0, 0, texture_text_also_can_be_alternative.width, texture_text_also_can_be_alternative.height),
            new Vector2(0.50f, 0.50f),
            15f
        );
        OnlineCursorUi.cursorTextAlsoCanBeAlternativeSprite = cursor_text_also_can_be_alternative;

        var texture_link = cursors[4];
        var cursor_link = Sprite.Create(
            texture_link,
            new Rect(0, 0, texture_link.width, texture_link.height),
            new Vector2(0.50f, 0.50f),
            15f
        );
        OnlineCursorUi.cursorLinkSprite = cursor_link;
        LoggerCursor.Logger.LogInfo("Games system cursor initialized");
    }

    void LateUpdate()
    {
        if (!Util.IsWorldGenerated()) return;
        if (Input.GetKeyDown(KeyCode.F2))
        {
            visable = !visable;
        }
        foreach (var netBody in NetBody.all_instances)
        {
#if DEBUG
            // 
#else
            if (netBody.plr == ScavPlayer.LocalPlayer) continue;
#endif
            if (!netBody.body.TryGetComponent<OnlineCursorUi>(out OnlineCursorUi onlineCursorUi))
            {
                var uiCursor = netBody.body.gameObject.AddComponent<OnlineCursorUi>();
                uiCursor.body = netBody.body;
                continue;
            }
        }

    }
}

public class OnlineCursorUi : MonoBehaviour
{
    internal static Sprite cursorNormalSprite; // 0
    internal static Sprite cursorLink2OrAlternative3Sprite; // 1
    internal static Sprite cursorAlternativeSprite; // 2
    internal static Sprite cursorTextAlsoCanBeAlternativeSprite; // 3
    internal static Sprite cursorLinkSprite; // 4
    public Body body;
    public ScavPlayer scavPlayer;
    private GameObject mainGameObject;
    private GameObject cursorPositions;
    private Color24 plrColor;
    private Action updateUsernameAndColor;
    private StatusCursor statusCursor;
    private Action updateCursorStatus;

    void Awake()
    {
        mainGameObject = new GameObject("target_Body");
        mainGameObject.layer = 0;

        cursorPositions = new GameObject("CurssorPositions");
        cursorPositions.transform.SetParent(mainGameObject.transform);

        var cursorSprite = new GameObject("Cursor", [typeof(SpriteRenderer), typeof(RectTransform)]);
        cursorSprite.transform.SetParent(cursorPositions.transform);

        // var texture = PlayerCamera.main.cursors;
        // Sprite.Create()
        var cursorRender = cursorSprite.GetComponent<SpriteRenderer>();
        cursorRender.sprite = cursorNormalSprite;
        cursorRender.transform.localScale = new Vector2(0.6f, 0.6f);
        cursorRender.sortingOrder = 32767;
        cursorRender.sortingLayerName = "Overlay";
        updateCursorStatus = () =>
        {
            if (statusCursor == StatusCursor.cursorNormal)
            {
                cursorRender.sprite = cursorNormalSprite;
            }
            else if (statusCursor == StatusCursor.cursorAlternative)
            {
                cursorRender.sprite = cursorAlternativeSprite;
            }
            else if (statusCursor == StatusCursor.cursorLink)
            {
                cursorRender.sprite = cursorLinkSprite;
            }
            else if (statusCursor == StatusCursor.cursorLink2OrAlternative3)
            {
                cursorRender.sprite = cursorLink2OrAlternative3Sprite;
            }
            else if (statusCursor == StatusCursor.hide)
            {
                cursorRender.sprite = null;
            }
        };

        var rectSprite = cursorSprite.GetComponent<RectTransform>();
        rectSprite.localPosition = new Vector2(1.2f, -1.2f);

        // userName
        var mainCanvast = new GameObject(
            "main_Username",
            [
                typeof(RectTransform),
                typeof(Canvas),
                typeof(ContentSizeFitter)
            ]
        );
        mainCanvast.transform.SetParent(cursorPositions.transform, false);

        var canvas = mainCanvast.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32767;

        var rectDescriptions = mainCanvast.GetComponent<RectTransform>();
        rectDescriptions.sizeDelta = new Vector2(5, 5);

        GameObject background = new GameObject("background", [typeof(Image), typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup)]);
        background.transform.SetParent(mainCanvast.transform);

        var imgBackground = background.GetComponent<Image>();
        imgBackground.color = new Color(0, 0, 0);

        var conSizeFi = background.GetComponent<ContentSizeFitter>();
        conSizeFi.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        conSizeFi.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = background.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleLeft;
        // layout.padding = new RectOffset(0.1f, 0.1f, 1, 1);
        layout.childControlWidth = true;
        layout.childScaleWidth = true;
        layout.childForceExpandWidth = true;

        var rectBackground = background.GetComponent<RectTransform>();
        rectBackground.localPosition = new Vector2(3.5f, -1.6f);
        rectBackground.sizeDelta = new Vector2(2, 2);

        var userNameObj = new GameObject("username", [typeof(TextMeshProUGUI)]);
        userNameObj.transform.SetParent(background.transform);

        var textUsername = userNameObj.GetComponent<TextMeshProUGUI>();
        textUsername.fontSize = 0.8f;

        if (scavPlayer == null && !Net.IsRunning)
        {
            textUsername.text = "Local Player";
        }
        updateUsernameAndColor = () =>
        {
            textUsername.text = $"<color={plrColor.ToHex()}>{scavPlayer.playerName}</color>";
        };
    }
    void Start()
    {
#if DEBUG
        if (Net.IsRunning)
        {
            if (body == null)
            {
                Destroy(this);
            }
            scavPlayer = ScavPlayer.GetPlayerFromBody(body);
            if (scavPlayer == null)
            {
                Destroy(this);
            }
            plrColor = scavPlayer.playerColor;
            updateUsernameAndColor();

        }
#else
        if (body == null)
        {
            Destroy(this);
        }
        scavPlayer = ScavPlayer.GetPlayerFromBody(body);
        if (scavPlayer == null)
        {
            Destroy(this);
        }
        plrColor = scavPlayer.playerColor;
        updateUsernameAndColor();
#endif


    }

    void Update()
    {
#if DEBUG
        if (Net.IsRunning)
        {
            var plr = ScavPlayer.GetPlayerFromBody(body);
            if (plr == null)
            {
                Destroy(this);
                return;
            }
            scavPlayer = plr;
            body = plr.body;
        }
#else
        var plr = ScavPlayer.GetPlayerFromBody(body);
        if (plr == null)
        {
            Destroy(this);
            return;
        }
        scavPlayer = plr;
        body = plr.body;
#endif
    }
    void LateUpdate()
    {
        statusCursor = StatusCursor.cursorNormal;
#if DEBUG
        if (Net.IsRunning)
        {
            if (scavPlayer == null)
            {
                Destroy(this);
                return;
            }

            if (!OnlineCursorProcess.visable)
            {
                if (mainGameObject.activeSelf)
                {
                    mainGameObject.SetActive(false);
                }
                return;
            }
            else
            {
                if (!mainGameObject.activeSelf)
                {
                    mainGameObject.SetActive(true);
                }
            }
            if (OnlineCursorProcess.isIgnorePVPInvsableCursor) { }
            else if (Multiplayer.rules.PVP && Multiplayer.rules.Teams)
            {
                if (scavPlayer.playerColor != ScavPlayer.LocalPlayer.playerColor)
                {
                    statusCursor = StatusCursor.hide;
                    updateCursorStatus();
                    return;
                }
            }
            else if (Multiplayer.rules.PVP && !Multiplayer.rules.Teams)
            {
                statusCursor = StatusCursor.hide;
                updateCursorStatus();
                return;
            }
            else if (scavPlayer.IsAltTab || scavPlayer.IsDead())
            {
                statusCursor = StatusCursor.hide;
                updateCursorStatus();
                return;
            }
        }
#else
        if (scavPlayer == null)
        {
            Destroy(this);
            return;
        }

        if (!OnlineCursorProcess.visable)
        {
            if (mainGameObject.activeSelf)
            {
                mainGameObject.SetActive(false);
            }
            return;
        }
        else
        {
            if (!mainGameObject.activeSelf)
            {
                mainGameObject.SetActive(true);
            }
        }
        if (OnlineCursorProcess.isIgnorePVPInvsableCursor) { }
        else if (Multiplayer.rules.PVP && Multiplayer.rules.Teams)
        {
            if (scavPlayer.playerColor != ScavPlayer.LocalPlayer.playerColor)
            {
                statusCursor = StatusCursor.hide;
                updateCursorStatus();
                return;
            }
        }
        else if (Multiplayer.rules.PVP && !Multiplayer.rules.Teams)
        {
            statusCursor = StatusCursor.hide;
            updateCursorStatus();
            return;
        }
        else if (scavPlayer.IsAltTab || scavPlayer.IsDead())
        {
            statusCursor = StatusCursor.hide;
            updateCursorStatus();
            return;
        }
#endif

        mainGameObject.transform.position = body.transform.position;
        var targetLookPos = body.targetLookPos;
        var bodyPos = gameObject.transform.position;
        var offset = targetLookPos - bodyPos;
        cursorPositions.transform.localPosition = new Vector3(offset.x, offset.y);
        Collider2D collider = Physics2D.OverlapPoint(targetLookPos, LayerMask.GetMask(["Item", "Ground", "Descriptor", "Limb", "Body"]));
        // if (!collider)
        // {
        //     collider = Physics2D.OverlapPoint(targetLookPos, LayerMask.GetMask("Body"));
        // }
        if ((bool)collider)
        {
            if (collider.TryGetComponent<Item>(out Item item))
            {
                statusCursor = StatusCursor.cursorLink2OrAlternative3;
#if DEBUG
                LoggerCursor.Logger.LogDebug($"Hover item : {item.name}");
#endif
            }
            else if (collider.TryGetComponent<BuildingEntity>(out BuildingEntity building))
            {
#if DEBUG
                LoggerCursor.Logger.LogDebug($"Hover BuildingEntity : {building.name}");
#endif
                if (building.TryGetComponent<UsableObject>(out UsableObject usableObject))
                {
                    var posBody = body.transform.position;
                    var usablePos = usableObject.transform.position;
                    var limitDist = 10f * usableObject.rangeMultiplier;
                    var dist = Vector2.Distance(posBody, usablePos);
                    if (dist < limitDist)
                    {
                        // cursorRender.sprite = cursorLinkSprite;
                        statusCursor = StatusCursor.cursorLink;
                    }
                    else
                    {
                        // cursorRender.sprite = cursorAlternativeSprite;
                        statusCursor = StatusCursor.cursorAlternative;
                    }
                }
                else
                {
                    // cursorRender.sprite = cursorAlternativeSprite;
                    statusCursor = StatusCursor.cursorAlternative;
                }
            }
            else if (collider.TryGetComponent<Body>(out Body body))
            {
                statusCursor = StatusCursor.cursorLink;
#if DEBUG
                LoggerCursor.Logger.LogDebug($"Hover Body : {body.name}");
#endif

            }
            else if (collider.TryGetComponent<ChunkScript>(out ChunkScript chunk))
            {
                var posBlock = WorldGeneration.world.WorldToBlockPos(targetLookPos);
                BlockInfo blockInfo = WorldGeneration.world.GetBlockInfo(WorldGeneration.world.worldBlocks[posBlock.x, posBlock.y]);
                Descriptions(blockInfo.name);
                statusCursor = StatusCursor.cursorAlternative;
#if DEBUG
                LoggerCursor.Logger.LogDebug($"Hover block : {blockInfo.name}");
#endif
            }
            else
            {
#if DEBUG
                LoggerCursor.Logger.LogDebug(collider.GetType().FullName);
#endif
                statusCursor = StatusCursor.cursorNormal;
            }
        }
        updateCursorStatus();
#if DEBUG
        if (!Net.IsRunning) return;
        if (scavPlayer == null) return;
#endif

        if (scavPlayer.playerColor != plrColor)
        {
            plrColor = scavPlayer.playerColor;
            updateUsernameAndColor();
        }
    }
    void Descriptions(string title, string descriptions = null)
    {
        // hoverTextTitle.text = title;
    }
    void OnDestroy()
    {
        Destroy(mainGameObject);
        Destroy(cursorPositions);
    }
}

public enum StatusCursor
{
    cursorNormal = 0,
    cursorLink2OrAlternative3 = 1,
    cursorAlternative = 2,
    cursorTextAlsoCanBeAlternativeSprite = 3,
    cursorLink = 4,

    hide = 999
}