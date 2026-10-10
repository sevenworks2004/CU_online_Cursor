using System;
using System.Collections.Generic;
using CasualtiesTogetherUtils;
using CUCoreLib.Helpers;
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
        SceneManager.activeSceneChanged += (Scene oldScene, Scene newScene) =>
        {
            if (newScene.name == "SampleScene")
            {
                if (Net.IsRunning && gameObjectProcessor == null)
                {
                    gameObjectProcessor = new GameObject("CurssorUi_Processor");
                    gameObjectProcessor.AddComponent<OnlineCursorProcess>();

                }
#if DEBUG
                else
                {
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
    private static Sprite cursorNormalSprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.normal.png");
    private static Sprite cursorLinkSprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.link.png");
    private static Sprite cursorAlternativeSprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.alternative.png");
    private static Sprite cursorLink2OrAlternative3Sprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.link2_or_alternative3.png");
    private static Sprite cursorTextAlsoCanBeAlternativeSprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.text_also_can_be_alternative.png");
    public Body body;
    public ScavPlayer scavPlayer;
    private GameObject mainGameObject;
    private GameObject cursorPositions;
    private SpriteRenderer cursorRender;
    private Color24 plrColor;
    private Action updateUsernameAndColor;
    void Awake()
    {
        mainGameObject = new GameObject("target_Body");
        mainGameObject.layer = 0;

        cursorPositions = new GameObject("CurssorPositions");
        cursorPositions.transform.SetParent(mainGameObject.transform);

        var cursorSprite = new GameObject("Cursor", [typeof(SpriteRenderer), typeof(RectTransform)]);
        cursorSprite.transform.SetParent(cursorPositions.transform);

        cursorRender = cursorSprite.GetComponent<SpriteRenderer>();
        cursorRender.sprite = cursorNormalSprite;
        cursorRender.transform.localScale = new Vector2(0.6f, 0.6f);
        cursorRender.sortingOrder = 32767;
        cursorRender.sortingLayerName = "Overlay";

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
                Console.WriteLine("===============================\nError body null\n========================");
                Destroy(this);
            }
            scavPlayer = ScavPlayer.GetPlayerFromBody(body);
            if (scavPlayer == null)
            {
                Console.WriteLine("===============================\nError body null\n========================");
                Destroy(this);
            }
            plrColor = scavPlayer.playerColor;
            updateUsernameAndColor();

        }
#else
        if (body == null)
        {
            Console.WriteLine("===============================\nError body null\n========================");
            Destroy(this);
        }
        scavPlayer = ScavPlayer.GetPlayerFromBody(body);
        if (scavPlayer == null)
        {
            Console.WriteLine("===============================\nError scavBody null\n========================");
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
        if (scavPlayer == null)
        {
            Destroy(this);
            return;
        }
        if (OnlineCursorProcess.isIgnorePVPInvsableCursor) { }
        else if (Multiplayer.rules.PVP && Multiplayer.rules.Teams)
        {
            if (scavPlayer.playerColor != ScavPlayer.LocalPlayer.playerColor)
            {
                cursorRender.sprite = null;
                return;
            }
        }
        else if (Multiplayer.rules.PVP && !Multiplayer.rules.Teams)
        {
            cursorRender.sprite = null;
            return;
        }
        else if (scavPlayer.IsAltTab || scavPlayer.IsDead())
        {
            cursorRender.sprite = null;
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

        mainGameObject.transform.position = body.transform.position;
        var targetLookPos = body.targetLookPos;
        var bodyPos = gameObject.transform.position;
        var offset = targetLookPos - bodyPos;
        cursorPositions.transform.localPosition = new Vector3(offset.x, offset.y);

        Collider2D[] cols = Physics2D.OverlapPointAll(targetLookPos);
        if (cols.Length == 0)
        {
            cursorRender.sprite = cursorNormalSprite;
            // hoverTextTitle.text = null;
        }
        foreach (var col in cols)
        {
            if (col.TryGetComponent<BuildingEntity>(out BuildingEntity building))
            {
                Descriptions(building.fullNameDisplay);
                if (building.TryGetComponent<UsableObject>(out UsableObject usableObject))
                {
                    var posBody = body.transform.position;
                    var usablePos = usableObject.transform.position;
                    var limitDist = 10f * usableObject.rangeMultiplier;
                    var dist = Vector2.Distance(posBody, usablePos);
                    if (dist < limitDist)
                    {
                        cursorRender.sprite = cursorLinkSprite;
                    }
                    else
                    {
                        cursorRender.sprite = cursorAlternativeSprite;
                    }
                }
                else
                {
                    cursorRender.sprite = cursorAlternativeSprite;
                }
            }
            else if (col.TryGetComponent<Body>(out Body b))
            {
                cursorRender.sprite = cursorLinkSprite;
                var p = ScavPlayer.GetPlayerFromBody(b);
                if (p == null) continue;
                Descriptions(p.playerName);
            }
            else if (col.TryGetComponent<Item>(out Item item))
            {
                cursorRender.sprite = cursorLink2OrAlternative3Sprite;
                Descriptions(item.fullName);
            }
            else if (col.TryGetComponent<ChunkScript>(out ChunkScript chunk))
            {
                var posBlock = WorldGeneration.world.WorldToBlockPos(targetLookPos);
                BlockInfo blockInfo = WorldGeneration.world.GetBlockInfo(WorldGeneration.world.worldBlocks[posBlock.x, posBlock.y]);
                Descriptions(blockInfo.name);
                cursorRender.sprite = cursorAlternativeSprite;
            }
            else
            {
#if DEBUG
                Console.WriteLine(col.GetType().FullName);
#endif
                cursorRender.sprite = cursorNormalSprite;
            }
        }
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
        Destroy(cursorRender);
    }
}