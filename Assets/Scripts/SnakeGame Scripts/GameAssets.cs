using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets Instance;

    [Header("Snake Sprites")]
    public Sprite snakeHeadSprite;
    public Sprite snakeBodySprite;
    public Sprite snakeBodyCornerSprite;
    //public Sprite snakeTailSprite;

    [Header("Apple Sprites")]
    public Sprite redAppleSprite;
    public Sprite goldenAppleSprite;
    public Sprite diamondAppleSprite;
    public Sprite metalAppleSprite;
    public Sprite blueAppleSprite;

    [Header("Apple Timer UI Sprites")]
    public Sprite circleSprite;

    [Header("Wall Prefab")]
    public GameObject wallTilePrefab;

    private void Awake()
    {
        Instance = this;
    }
}