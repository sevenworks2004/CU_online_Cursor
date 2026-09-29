using System;
using CUCoreLib.Helpers;
using KrokoshaCasualtiesMP;
using KrokoshaCasualtiesUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OnlineCursorProcess : MonoBehaviour
{
    private static GameObject gameObjectProcessor;
    public static bool isIgnorePVPInvsableCursor = false; // :)
    public bool keyVisable = true;
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
        if (Input.GetKeyDown(KeyCode.F2)) keyVisable = !keyVisable;
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
            }
#if DEBUG
            if (isIgnorePVPInvsableCursor && keyVisable)
            {
                if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = true;
                continue;
            }
            if (!plrBody.body.alive)
            {
                if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = false;
            }
#else
            if (plrBody.player.is_alttab || !plrBody.body.alive)
            {
                if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = false;
            }
#endif
            else
            {
                if (!keyVisable)
                {
                    onlineCursorUi.isVisiableCurssor = false;
                }
                if (KrokoshaScavMultiplayer.rules.PVP && KrokoshaScavMultiplayer.rules.Teams)
                {
                    if (plrBody.player.playerColor == NetPlayer.LOCAL_PLAYER.playerColor)
                    {
                        if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = true;
                    }
                }
                else if (KrokoshaScavMultiplayer.rules.PVP && !KrokoshaScavMultiplayer.rules.Teams)
                {
                    if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = false;
                }
                else
                {
                    if (onlineCursorUi != null) onlineCursorUi.isVisiableCurssor = true;
                }

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
    private Body body;
    private NetPlayer netPlayer;
    private GameObject mainGameObject;
    private GameObject cursorObject;
    private SpriteRenderer cursorRender;
    private Color24 colorPlayer;
    public bool isVisiableCurssor = true;

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
        // mainGameObject.transform.localPosition = Vector3.zero;

        cursorObject = new GameObject("Curssor");
        cursorObject.transform.localScale = new Vector3(20f, 20f);
        cursorObject.transform.SetParent(mainGameObject.transform);

        cursorRender = cursorObject.AddComponent<SpriteRenderer>();
        cursorRender.sprite = cursorNormalSprite;
        // spriteRenderer.transform.localPosition = new Vector2(0.5f,-0.5f);
        cursorRender.transform.localScale = new Vector2(0.6f, 0.6f);
        cursorRender.sortingOrder = 32767;
        cursorRender.sortingLayerName = "Overlay";


    }

    void Update()
    {
        if (body == null)
        {
            Destroy(gameObject);
            return;
        }
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
        }
        foreach (var col in cols)
        {
            if (col.TryGetComponent<BuildingEntity>(out BuildingEntity building))
            {
                if (building.TryGetComponent<UsableObject>(out UsableObject usableObject))
                {
                    var posBody = body.transform.position;
                    var usablePos = usableObject.transform.position;
                    var limitDist = 10f * usableObject.rangeMultiplier;
                    // if (objDescript == null) objDescript = Descriptions(cursorObject.transform, building.fullNameDisplay, building.description);

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
                    cursorRender.sprite = cursorLinkSprite;
                }
            }
            else if (col.TryGetComponent<Body>(out Body _))
            {
                cursorRender.sprite = cursorLinkSprite;
            }
            else if (col.TryGetComponent<Item>(out Item item))
            {
                cursorRender.sprite = cursorLink2OrAlternative3Sprite;
            }
            else
            {
                cursorRender.sprite = cursorNormalSprite;
            }
        }

#if DEBUG
        if (!KrokoshaScavMultiplayer.network_system_is_running) return;
#endif

        if (netPlayer == null) return;

    }
}