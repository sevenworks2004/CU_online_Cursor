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
            if (isIgnorePVPInvsableCursor)
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
    // private GameObject objDescript;

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
            // if (objDescript != null) Destroy(objDescript);

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
            // if (objDescript != null) Destroy(objDescript);
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

        // if (colorPlayer != netPlayer.playerColor)
        // {
        //     colorPlayer = netPlayer.playerColor;
        //     setColorCurssor();
        // }

    }
    // public static GameObject Descriptions(Transform transform,string title, string descriptions)
    // {
    //     var mainDescriptions = new GameObject("mainDescriptions", [typeof(RectTransform)]);
    //     var rectMainDescriptions = mainDescriptions.GetComponent<RectTransform>();
    //     rectMainDescriptions.sizeDelta = new Vector2(300f, 300f);

    //     var bgObj = new GameObject("background", [typeof(RectTransform)]);
    //     bgObj.transform.SetParent(mainDescriptions.transform);

    //     var rectBackgound = bgObj.GetComponent<RectTransform>();
    //     rectBackgound.anchorMin = Vector2.zero;
    //     rectBackgound.anchorMax = Vector2.one;
    //     rectBackgound.offsetMin = Vector2.zero;
    //     rectBackgound.offsetMax = Vector2.zero;

    //     var bgImage = bgObj.AddComponent<Image>();
    //     bgImage.color = new Color(1f, 0f, 0f, 0.0f);



    //     return mainDescriptions;
    // }
    // void setColorCurssor()
    // {
    //     Texture2D texture = cursorRender.sprite.texture;

    //     Texture2D newTexture = new Texture2D(
    //         texture.width,
    //         texture.height,
    //         TextureFormat.RGBA32,
    //         false
    //     );

    //     Color[] pixels = texture.GetPixels();
    //     Color targetColor = new Color(colorPlayer.r / 255f, colorPlayer.g / 255f, colorPlayer.b / 255f);

    //     for (int i = 0; i < pixels.Length; i++)
    //     {
    //         Color pixel = pixels[i];

    //         // Если пиксель не белый (или близок к белому) и достаточно темный (черный / контур)
    //         if (!(pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f) &&
    //             pixel.r < 0.2f &&
    //             pixel.g < 0.2f &&
    //             pixel.b < 0.2f)
    //         {
    //             pixels[i] = new Color(targetColor.r, targetColor.g, targetColor.b, pixel.a);
    //         }
    //     }

    //     newTexture.SetPixels(pixels);
    //     newTexture.Apply();

    //     Sprite oldSprite = cursorRender.sprite;

    //     Sprite newSprite = Sprite.Create(
    //         newTexture,
    //         oldSprite.rect,
    //         oldSprite.pivot / oldSprite.rect.size,
    //         oldSprite.pixelsPerUnit
    //     );

    //     cursorRender.sprite = newSprite;
    // }
}
// public static class RemoteCursorDescription
// {
//     public static GameObject Create(Transform parent, string title, string description)
//     {
//         // --- Корневой объект ---
//         var root = new GameObject("remoteCursorDescription");

//         // RectTransform вместо обычного Transform
//         var rootRect = root.AddComponent<RectTransform>();
//         rootRect.sizeDelta = new Vector2(240f, 100f); // ширина x высота
//         rootRect.pivot = new Vector2(0.5f, 1f);       // точка привязки сверху

//         if (parent != null)
//         {
//             root.transform.SetParent(parent, false);
//             rootRect.localPosition = Vector3.zero;
//         }

//         // --- Фон (чёрный полупрозрачный) ---
//         var bg = new GameObject("Background");
//         bg.transform.SetParent(root.transform, false);

//         var bgRect = bg.AddComponent<RectTransform>();
//         bgRect.anchorMin = Vector2.zero;
//         bgRect.anchorMax = Vector2.one;
//         bgRect.offsetMin = Vector2.zero;
//         bgRect.offsetMax = Vector2.zero;

//         var bgImage = bg.AddComponent<Image>();
//         bgImage.color = new Color(1f, 0f, 0f, 0.6f); // чёрный, 60% непрозрачности

//         // --- Title ---
//         var titleGO = CreateText(root.transform, "Title", title, 16, FontStyle.Bold);
//         var titleRect = titleGO.GetComponent<RectTransform>();
//         titleRect.anchorMin = new Vector2(0f, 1f);
//         titleRect.anchorMax = new Vector2(1f, 1f);
//         titleRect.pivot = new Vector2(0.5f, 1f);
//         titleRect.offsetMin = new Vector2(10f, -30f); // отступ слева
//         titleRect.offsetMax = new Vector2(-10f, -8f); // отступ справа и сверху

//         // --- Description ---
//         var descGO = CreateText(root.transform, "Description", description, 13, FontStyle.Normal);
//         var descRect = descGO.GetComponent<RectTransform>();
//         descRect.anchorMin = new Vector2(0f, 0f);
//         descRect.anchorMax = new Vector2(1f, 1f);
//         descRect.pivot = new Vector2(0.5f, 1f);
//         descRect.offsetMin = new Vector2(10f, 8f);
//         descRect.offsetMax = new Vector2(-10f, -35f);

//         return root;
//     }

//     private static GameObject CreateText(Transform parent, string name, string content, int fontSize, FontStyle style)
//     {
//         var go = new GameObject(name);
//         go.transform.SetParent(parent, false);

//         go.AddComponent<RectTransform>();

//         var text = go.AddComponent<Text>();
//         text.text = content;
//         // text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
//         text.fontSize = fontSize;
//         text.fontStyle = style;
//         text.color = Color.white;
//         text.alignment = TextAnchor.UpperLeft;
//         text.horizontalOverflow = HorizontalWrapMode.Wrap;
//         text.verticalOverflow = VerticalWrapMode.Overflow;
//         text.raycastTarget = false;

//         return go;
//     }
// }