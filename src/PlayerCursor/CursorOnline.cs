
using System;
using System.Collections.Generic;
using System.Threading;
using CUCoreLib.Helpers;
using KrokoshaCasualtiesMP;
using KrokoshaCasualtiesUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class OnlineCursorProcess : MonoBehaviour
{
    private List<OnlineCursorUi> listPlayer = [];
    private static GameObject gameObjectProcessor;
    public static void init()
    {

        SceneManager.activeSceneChanged += (Scene oldScene, Scene newScene) =>
        {
            if (newScene.name == "SampleScene")
            {
                if (KrokoshaScavMultiplayer.network_system_is_running)
                {
                    gameObjectProcessor = new GameObject("CursorUiRuniner");
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

            if (!plrBody.body.TryGetComponent<OnlineCursorUi>(out OnlineCursorUi onlineCursorUi))
            {
                plrBody.body.gameObject.AddComponent<OnlineCursorUi>();
            }

            if (plrBody.player.is_alttab)
            {
                if (onlineCursorUi != null) onlineCursorUi.enabled = false;
            }
            else
            {
                if (onlineCursorUi != null) onlineCursorUi.enabled = true;
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
    private GameObject cursor;
    private SpriteRenderer cursorRender;
    private Color24 colorPlayer;


    void Start()
    {
        body = gameObject.GetComponent<Body>();
        if (KrokoshaScavMultiplayer.network_system_is_running)
        {
            netPlayer = NetPlayer.GetNetPlayerFromBody(body);
            if (netPlayer == null)
            {
                Console.WriteLine($"Error Net Player Not found name body: {body.name}");
            }
            colorPlayer = netPlayer.playerColor;
        }


        mainGameObject = new GameObject("target_Body");
        mainGameObject.layer = 0;
        // mainGameObject.transform.localPosition = Vector3.zero;

        cursor = new GameObject("Curssor");
        cursor.transform.localScale = new Vector3(20f, 20f);
        cursor.transform.SetParent(mainGameObject.transform);

        cursorRender = cursor.AddComponent<SpriteRenderer>();
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

        mainGameObject.transform.position = body.transform.position;
        var targetLookPos = body.targetLookPos;
        var bodyPos = gameObject.transform.position;
        var offset = targetLookPos - bodyPos;
        cursor.transform.localPosition = new Vector3(offset.x + 1.2f, offset.y - 1.2f);

        Collider2D[] cols = Physics2D.OverlapPointAll(targetLookPos);
        if (cols.Length == 0)
        {
            cursorRender.sprite = cursorNormalSprite;
        }
        foreach (var col in cols)
        {
            // Console.WriteLine(col.name);
            if (col.TryGetComponent<BuildingEntity>(out BuildingEntity building))
            {
                if (building.TryGetComponent<UsableObject>(out UsableObject usableObject))
                {
                    var posBody = body.transform.position;
                    var usablePos = usableObject.transform.position;
                    var limitDist = 10 * usableObject.rangeMultiplier;
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
                Console.WriteLine(col.name);
                cursorRender.sprite = cursorNormalSprite;
            }
        }


        if (!KrokoshaScavMultiplayer.network_system_is_running) return;

        if (netPlayer == null) return;
        
        if (colorPlayer != netPlayer.playerColor)
        {
            colorPlayer = netPlayer.playerColor;
            setColorCurssor();
        }

    }
    void setColorCurssor()
    {
        Texture2D texture = cursorRender.sprite.texture;

        Texture2D newTexture = new Texture2D(
            texture.width,
            texture.height,
            TextureFormat.RGBA32,
            false
        );

        Color[] pixels = texture.GetPixels();
        Color targetColor = new Color(colorPlayer.r / 255f, colorPlayer.g / 255f, colorPlayer.b / 255f);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            // Если пиксель не белый (или близок к белому) и достаточно темный (черный / контур)
            if (!(pixel.r > 0.9f && pixel.g > 0.9f && pixel.b > 0.9f) &&
                pixel.r < 0.2f &&
                pixel.g < 0.2f &&
                pixel.b < 0.2f)
            {
                pixels[i] = new Color(targetColor.r, targetColor.g, targetColor.b, pixel.a);
            }
        }

        newTexture.SetPixels(pixels);
        newTexture.Apply();

        Sprite oldSprite = cursorRender.sprite;

        Sprite newSprite = Sprite.Create(
            newTexture,
            oldSprite.rect,
            oldSprite.pivot / oldSprite.rect.size,
            oldSprite.pixelsPerUnit
        );

        cursorRender.sprite = newSprite;
    }
}