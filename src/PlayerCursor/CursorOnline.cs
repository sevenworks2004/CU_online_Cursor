
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
            if (newScene.name == "SampleScene" && KrokoshaScavMultiplayer.network_system_is_running)
            {
                gameObjectProcessor = new GameObject("CursorUiRuniner");
                gameObjectProcessor.AddComponent<OnlineCursorProcess>();
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
        foreach (var plr in NetBody.all_instances)
        {
            if (!plr.body.TryGetComponent<OnlineCursorUi>(out OnlineCursorUi _))
            {
                plr.body.gameObject.AddComponent<OnlineCursorUi>();
            }
        }
    }

}

public class OnlineCursorUi : MonoBehaviour
{
    private Body body;
    private GameObject gameObjectCursor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        gameObjectCursor = new GameObject("Cursor");
        gameObjectCursor.transform.localPosition = Vector3.zero;
        gameObjectCursor.transform.localScale = new Vector3(10, 10);

        Sprite sprite = AssetLoader.LoadEmbeddedSprite("assets.cursor.normal.png",96);

        spriteRenderer = gameObjectCursor.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        // spriteRenderer.color = Color.blue;
        // spriteRenderer.sortingOrder = 6000;
        body = gameObject.GetComponent<Body>();
        if (body == null)
        {
            Console.WriteLine("Error Body");
            Destroy(gameObject);
            return;
        }
        gameObjectCursor.transform.SetParent(body.transform, false);
    }

    private bool isRun = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            isRun = !isRun;
        }
    }
    void LateUpdate()
    {

        if (body == null)
        {
            Destroy(gameObject);
            return;
        }
        if (!isRun)
        {
            gameObjectCursor.transform.localPosition = Vector3.zero;
            return;
        };

        Vector3 ls = gameObjectCursor.transform.localScale;
        float sign = Mathf.Sign(body.transform.lossyScale.x);
        gameObjectCursor.transform.localScale = new Vector3(
            Mathf.Abs(ls.x) * sign,
            ls.y,
            ls.z
        );
        var targetLookPos = body.targetLookPos;
        var bodyPos = body.transform.position;
        var offset = targetLookPos - bodyPos;
        gameObjectCursor.transform.localPosition = new Vector2(offset.x,offset.y);
        Console.WriteLine($"Cursor: {offset} -- BodyPos: {bodyPos} -- targetLookPos : {targetLookPos}");
    }
}