using System;
using System.Collections.Generic;
using CUCoreLib.Helpers;
using KrokoshaCasualtiesMP;
using KrokoshaCasualtiesUtils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OnlineCursorProcess : MonoBehaviour
{
    private static GameObject gameObjectProcessor;
    public static bool isIgnorePVPInvsableCursor = false; // :)
    public bool visable = true;
    public static void init()
    {
        SceneManager.activeSceneChanged += (Scene oldScene, Scene newScene) =>
        {
            if (newScene.name == "SampleScene")
            {
                if (KrokoshaScavMultiplayer.network_system_is_running)
                {
                    gameObjectProcessor = new GameObject("CurssorUi_Processor");
                    gameObjectProcessor.AddComponent<OnlineCursorProcess>();

                }
#if DEBUG
                else
                {
                    PlayerCamera.main.body.gameObject.AddComponent<OnlineCursorUi>();
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
        foreach (var plrBody in NetBody.all_instances)
        {
#if DEBUG
            // 
#else
            if (plrBody.player == NetPlayer.LOCAL_PLAYER) continue;
#endif

            if (!plrBody.body.TryGetComponent<OnlineCursorUi>(out OnlineCursorUi onlineCursorUi))
            {
                plrBody.body.gameObject.AddComponent<OnlineCursorUi>();
                continue;
            }
            if (isIgnorePVPInvsableCursor && visable)
            {
                onlineCursorUi.isVisiableCurssor = false;
            }
            else if (plrBody.player.is_alttab || !plrBody.body.alive)
            {
                onlineCursorUi.isVisiableCurssor = false;
            }
            else
            {
                if (KrokoshaScavMultiplayer.rules.PVP && KrokoshaScavMultiplayer.rules.Teams)
                {
                    if (plrBody.player.playerColor == NetPlayer.LOCAL_PLAYER.playerColor)
                    {
                        onlineCursorUi.isVisiableCurssor = true;
                    }
                }
                else if (KrokoshaScavMultiplayer.rules.PVP && !KrokoshaScavMultiplayer.rules.Teams)
                {
                    onlineCursorUi.isVisiableCurssor = false;
                }
                else
                {
                    onlineCursorUi.isVisiableCurssor = true;
                }
            }
            if (!visable)
            {
                onlineCursorUi.isVisiableCurssor = false;
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
    public NetPlayer netPlayer;
    private GameObject mainGameObject;
    private GameObject cursorObject;
    private SpriteRenderer cursorRender;
    private Color24 colorPlayer;
    public bool isVisiableCurssor = true;
    private GameObject mainDescriptionsCanvast;
    private TextMeshProUGUI hoverTextTitle;

    void Start()
    {
        body = gameObject.GetComponent<Body>();
#if DEBUG
        if (KrokoshaScavMultiplayer.network_system_is_running)
        {
            netPlayer = NetPlayer.GetNetPlayerFromBody(body);
            if (netPlayer != null)
            {
                colorPlayer = netPlayer.playerColor;
            }
            else
            {
                Console.WriteLine($"Error Net Player Not found name body: {body.name}");
            }
        }
#else
        netPlayer = NetPlayer.GetNetPlayerFromBody(body);
        if (netPlayer != null)
        {
            colorPlayer = netPlayer.playerColor;
        }
        else
        {
            Console.WriteLine($"Error Net Player Not found name body: {body.name}");
        }
#endif
        mainGameObject = new GameObject("target_Body");
        mainGameObject.layer = 0;

        cursorObject = new GameObject("Curssor");
        // cursorObject.transform.localScale = new Vector3(20f, 20f);
        cursorObject.transform.SetParent(mainGameObject.transform);

        cursorRender = cursorObject.AddComponent<SpriteRenderer>();
        cursorRender.sprite = cursorNormalSprite;
        cursorRender.transform.localScale = new Vector2(0.6f, 0.6f);
        cursorRender.sortingOrder = 32767;
        cursorRender.sortingLayerName = "Overlay";

        // Descriptions
        mainDescriptionsCanvast = new GameObject(
            "main_Descriptions",
            [
                typeof(RectTransform),
                typeof(Canvas),
                typeof(ContentSizeFitter)
            ]
        );
        mainDescriptionsCanvast.transform.SetParent(cursorObject.transform, false);
        mainDescriptionsCanvast.transform.localPosition = Vector2.zero;

        var canvas = mainDescriptionsCanvast.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32767;


        var rectDescriptions = mainDescriptionsCanvast.GetComponent<RectTransform>();
        rectDescriptions.sizeDelta = new Vector2(5, 5);

        GameObject background = new GameObject("background", [typeof(Image), typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup)]);
        background.transform.SetParent(mainDescriptionsCanvast.transform);

        var img = background.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f);

        var rec = background.GetComponent<RectTransform>();
        rec.sizeDelta = new Vector2(5, 5);

        var layout = background.GetComponent<VerticalLayoutGroup>();

        var conSizeFi = background.GetComponent<ContentSizeFitter>();
        conSizeFi.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        conSizeFi.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textObject = new GameObject("text", [typeof(TextMeshProUGUI)]);
        textObject.transform.SetParent(background.transform);

        hoverTextTitle = textObject.GetComponent<TextMeshProUGUI>();
        // hoverTextTitle.text = "Cashout Table Info";
        hoverTextTitle.fontSize = 0.7f;

        var rectBackground = background.GetComponent<RectTransform>();
        rectBackground.anchorMax = new Vector2(0.5f, 1f);
        rectBackground.anchorMin = new Vector2(0.5f, 1f);
        rectBackground.pivot = new Vector2(0.5f, 1f);

        background.transform.localPosition = new Vector3(5f, -1.0f);
    }
    void Update()
    {

#if DEBUG
        if (KrokoshaScavMultiplayer.network_system_is_running)
        {
            var plr = NetPlayer.GetNetPlayerFromBody(body);
            if (plr == null)
            {
                Destroy(gameObject);
                return;
            }
            netPlayer = plr;
            body = plr.body;
        }
#else
            var plr = NetPlayer.GetNetPlayerFromBody(body);
            if (plr == null)
            {
                Destroy(gameObject);
                return;
            }
            netPlayer = plr;
            body = plr.body;
#endif

    }
    void LateUpdate()
    {
        if (!isVisiableCurssor)
        {
            cursorRender.sprite = null;
            return;
        }

        mainGameObject.transform.position = body.transform.position;
        var targetLookPos = body.targetLookPos;
        var bodyPos = gameObject.transform.position;
        var offset = targetLookPos - bodyPos;
        cursorObject.transform.localPosition = new Vector3(offset.x + 1.2f, offset.y - 1.2f);

        Collider2D[] cols = Physics2D.OverlapPointAll(targetLookPos);
        if (cols.Length == 0)
        {
            cursorRender.sprite = cursorNormalSprite;
            hoverTextTitle.text = null;
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
                var p = NetPlayer.GetNetPlayerFromBody(b);
                if (p == null) continue;
                Descriptions(p.playername);
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
        if (!KrokoshaScavMultiplayer.network_system_is_running) return;
#endif
        if (netPlayer == null) return;
    }
    void Descriptions(string title, string descriptions = null)
    {
        hoverTextTitle.text = title;
    }
    void OnDestroy()
    {
        Destroy(mainGameObject);
        Destroy(cursorObject);
        Destroy(cursorRender);
    }
}