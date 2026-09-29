
using UnityEngine;

public class BodyOverlay : MonoBehaviour
{
    public Body body;

    public float height = 2.0f;
    public float squareSize = 0.25f;

    private void Awake()
    {
        CreateSquare();
    }

    private void LateUpdate()
    {
        FollowBody();
    }

    private void CreateSquare()
    {
        GameObject square = new GameObject("Square");

        square.transform.SetParent(transform, false);

        square.transform.localPosition = Vector3.zero;

        square.transform.localScale = new Vector3(
            squareSize,
            squareSize,
            1f
        );

        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();

        renderer.sprite = CreateSquareSprite();
        renderer.color = Color.black;

        renderer.sortingOrder = 6000;
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);

        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        texture.filterMode = FilterMode.Point;

        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }

    private void FollowBody()
    {
        if (body == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 position = body.transform.position;

        transform.position = new Vector3(
            position.x,
            position.y + height,
            position.z
        );

        transform.rotation = Quaternion.identity;
    }
}
