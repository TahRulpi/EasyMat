using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelGrid 
{
    // Normal apple (Red apple)
    private Vector2Int redApplePosition;
    private GameObject redAppleObject;

    private int redAppleEatCount = 0;

    // Golden apple
    private Vector2Int goldenApplePosition;
    private GameObject goldenAppleObject;
    private float goldenAppleTimer;
    private float goldenAppleDuration = 5f; // 5 seconds
    private bool goldenAppleActive = false;

    private RectTransform goldenAppleCircleRect; // store rect for easy positioning

    private Image goldenAppleCircleUI;
    private Image metalAppleCircleUI;

    // Diamond apple
    private Vector2Int diamondApplePosition;
    private GameObject diamondAppleObject;
    private bool diamondAppleActive = false;

    // Metal apple
    private Vector2Int metalApplePosition;
    private GameObject metalAppleObject;
    private bool metalAppleActive = false;
    private float metalAppleDuration = 10f;
    private float metalAppleTimer = 0f;
    private bool metalAppleWaiting = false; // triggered every 15 apples

    // added new for testing ==================================
    // Blue apple
    private Vector2Int blueApplePosition;
    private GameObject blueAppleObject;
    private bool blueAppleActive = false;

    // added new for testing ==================================
    private List<GameObject> wallObjects = new List<GameObject>();

    private int width;
    private int height;
    private Snake snake;

    public void Update()
    {
        if (goldenAppleActive)
        {
            goldenAppleTimer -= Time.deltaTime;

            if (goldenAppleCircleUI != null)
                goldenAppleCircleUI.fillAmount = goldenAppleTimer / goldenAppleDuration;

            if (goldenAppleTimer <= 0f)
            {
                Object.Destroy(goldenAppleObject);
                goldenAppleActive = false;

                if (goldenAppleCircleUI != null)
                    GameObject.Destroy(goldenAppleCircleUI.gameObject);
            }
        }

        // METAL APPLE countdown
        if (metalAppleActive)
        {
            metalAppleTimer -= Time.deltaTime;

            GameHandler.ShowMetalWarning("Eat the metal apple! Time left: " + metalAppleTimer.ToString("F1"));

            // The timer
            if (metalAppleCircleUI != null)
                metalAppleCircleUI.fillAmount = metalAppleTimer / metalAppleDuration;

            if (metalAppleTimer <= 0f)
            {
                metalAppleActive = false;
                Object.Destroy(metalAppleObject);

                // The timer
                if (metalAppleCircleUI != null) 
                    GameObject.Destroy(metalAppleCircleUI.gameObject);

                GameHandler.HideMetalWarning();

                // === SNAKE GAME OVER (same as collision) ===
                snake.SendMessage("ForceGameOver");
            }
        }
    }

    public LevelGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public void Setup(Snake snake)
    {
        this.snake = snake;

        SpawnRedApple();

        CreateWalls();  // added new for testing ==================================
    }

    private void SpawnRedApple()
    {
        do
        {
            redApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(redApplePosition));

        redAppleObject = new GameObject("RedApple", typeof(SpriteRenderer));
        redAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.redAppleSprite;

        redAppleObject.transform.position = new Vector3(redApplePosition.x, redApplePosition.y, 0);
    }

    private void SpawnGoldenApple()
    {
        do
        {
            goldenApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(goldenApplePosition));

        goldenAppleObject = new GameObject("GoldenApple", typeof(SpriteRenderer));
        goldenAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.goldenAppleSprite;

        goldenAppleObject.transform.position = new Vector3(goldenApplePosition.x, goldenApplePosition.y, 0);

        goldenAppleActive = true;
        goldenAppleTimer = goldenAppleDuration;

        //goldenAppleCircleUI = CreateGoldenAppleCircleUI();
        //goldenAppleCircleUI.fillAmount = 1f;

        goldenAppleCircleUI = CreateWorldTimer(GameAssets.Instance.circleSprite, new Color(0f, 1f, 0f, 0.05f), 3f, goldenAppleObject.transform);
    }

    private void SpawnDiamondApple()
    {
        do
        {
            diamondApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(diamondApplePosition));

        diamondAppleObject = new GameObject("DiamondApple", typeof(SpriteRenderer));
        diamondAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.diamondAppleSprite;

        diamondAppleObject.transform.position = new Vector3(diamondApplePosition.x, diamondApplePosition.y, 0);

        diamondAppleActive = true;
    }

    private void SpawnMetalApple()
    {
        do
        {
            metalApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(metalApplePosition));

        metalAppleObject = new GameObject("MetalApple", typeof(SpriteRenderer));
        metalAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.metalAppleSprite;

        metalAppleObject.transform.position = new Vector3(metalApplePosition.x, metalApplePosition.y, 0);

        metalAppleActive = true;
        metalAppleTimer = metalAppleDuration;

        GameHandler.ShowMetalWarning("Eat the Metal Apple in 7 seconds!");

        metalAppleCircleUI = CreateWorldTimer(GameAssets.Instance.circleSprite, new Color(0f, 0f, 1f, 0.05f), 3f, metalAppleObject.transform);
    }

    // added new for testing ==================================
    private void SpawnBlueApple()
    {
        do
        {
            blueApplePosition = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (snake.GetFullSnakeGridPositionList().Contains(blueApplePosition));

        blueAppleObject = new GameObject("BlueApple", typeof(SpriteRenderer));
        blueAppleObject.GetComponent<SpriteRenderer>().sprite = GameAssets.Instance.blueAppleSprite;

        blueAppleObject.transform.position = new Vector3(blueApplePosition.x, blueApplePosition.y, 0);

        blueAppleActive = true;
    }

    public bool TrySnakeEatFood(Vector2Int snakeGridPosition)
    {
        // RED APPLE = Grow + 10 points
        if (snakeGridPosition == redApplePosition)
        {
            Object.Destroy(redAppleObject);
            SpawnRedApple();
            GameHandler.AddScore(10);

            redAppleEatCount++;

            // Every 5 red apples → Golden apple appears
            if (redAppleEatCount % 5 == 0 && !goldenAppleActive)
            {
                SpawnGoldenApple();
            }

            // Every 10 red apples → Diamond apple 50% chance
            if (redAppleEatCount % 10 == 0 && !diamondAppleActive)
            {
                float chance = Random.value; // 0.0 to 1.0
                if (chance <= 0.50f) // 50% chance
                {
                    SpawnDiamondApple();
                }
            }

            // Every 15 red apples → Metal apple MUST appear
            if (redAppleEatCount % 12 == 0 && !metalAppleActive)
            {
                SpawnMetalApple();
            }

            // added new for testing ==================================
            // Every 8 red apples → Blue apple MUST appear
            if (redAppleEatCount % 2 == 0 && !blueAppleActive)
            {
                SpawnBlueApple();
            }

            return true; // grow
        }

        if (goldenAppleActive && snakeGridPosition == goldenApplePosition)
        {
            Object.Destroy(goldenAppleObject);
            goldenAppleActive = false;

            if (goldenAppleCircleUI != null)
            {
                GameObject.Destroy(goldenAppleCircleUI.gameObject);
                goldenAppleCircleUI = null;
                goldenAppleCircleRect = null;
            }

            GameHandler.AddScore(50);
            return false; // no grow
        }


        // DIAMOND APPLE = 500 points, NO grow
        if (diamondAppleActive && snakeGridPosition == diamondApplePosition)
        {
            Object.Destroy(diamondAppleObject);
            diamondAppleActive = false;

            GameHandler.AddScore(500);

            return false; // no grow
        }

        if (metalAppleActive && snakeGridPosition == metalApplePosition)
        {
            Object.Destroy(metalAppleObject);
            metalAppleActive = false;

            GameHandler.HideMetalWarning();

            // Metal apple gives no score, no growth
            return false;
        }

        // added new for testing ==================================
        // BLUE APPLE = Pass walls for 5 sec
        if (blueAppleActive && snakeGridPosition == blueApplePosition)
        {
            Object.Destroy(blueAppleObject);
            blueAppleActive = false;

            snake.canPassWalls = true;
            snake.StartCoroutine(snake.DisableWallPassAfter(5f));

            // added new for testing ==================================
            snake.canPassWalls = true;
            snake.StartCoroutine(snake.DisableWallPassAfter(5f));
            // Remove visual walls
            ClearWalls();

            return false; // no grow
        }

        return false;
    }

    private Image CreateWorldTimer(Sprite circleSprite, Color color, float size, Transform followTarget)
    {
        // Create parent canvas (World Space)
        GameObject root = new GameObject("WorldTimerUI");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = -1; // BEHIND THE APPLE

        // Create image
        GameObject imgObj = new GameObject("TimerCircle");
        imgObj.transform.SetParent(root.transform, false);

        Image img = imgObj.AddComponent<Image>();
        img.sprite = circleSprite;
        img.color = color;

        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillClockwise = false;
        img.fillAmount = 1f;

        // Size the circle (world units)
        RectTransform rt = img.rectTransform;
        rt.sizeDelta = new Vector2(size, size);

        // Parent to the apple so it follows automatically
        root.transform.SetParent(followTarget, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localScale = Vector3.one;

        return img;
    }

    // added new for testing ==================================
    public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
    {
        // If Blue Apple power is active → wrap around
        if (snake.canPassWalls)
        {
            if (gridPosition.x < 0) gridPosition.x = width - 1;
            if (gridPosition.x > width - 1) gridPosition.x = 0;
            if (gridPosition.y < 0) gridPosition.y = height - 1;
            if (gridPosition.y > height - 1) gridPosition.y = 0;

            return gridPosition;
        }

        // If Blue Apple NOT active → walls cause game over
        if (gridPosition.x < 0 || gridPosition.x > width - 1 ||
            gridPosition.y < 0 || gridPosition.y > height - 1)
        {
            snake.SendMessage("ForceGameOver"); // you already use this
        }

        return gridPosition;
    }
    //public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
    //{
    //    if (gridPosition.x < 0)
    //    {
    //        gridPosition.x = width - 1;
    //    }
    //    if (gridPosition.x > width - 1)
    //    {
    //        gridPosition.x = 0;
    //    }
    //    if (gridPosition.y < 0)
    //    {
    //        gridPosition.y = height - 1;
    //    }
    //    if (gridPosition.y > height - 1)
    //    {
    //        gridPosition.y = 0;
    //    }
    //    return gridPosition;
    //}

    // added new for testing ==================================
    public void CreateWalls()
    {
        ClearWalls();

        // Top and bottom walls
        for (int x = 0; x < width; x++)
        {
            CreateWallAt(new Vector2Int(x, -1));       // bottom row (outside grid)
            CreateWallAt(new Vector2Int(x, height));   // top row (outside grid)
        }

        // Left and right walls
        for (int y = 0; y < height; y++)
        {
            CreateWallAt(new Vector2Int(-1, y));       // left border
            CreateWallAt(new Vector2Int(width, y));    // right border
        }
    }

    private void CreateWallAt(Vector2Int pos)
    {
        GameObject wall = Object.Instantiate(GameAssets.Instance.wallTilePrefab);
        wall.transform.position = new Vector3(pos.x, pos.y, 0);
        wallObjects.Add(wall);
    }

    public void ClearWalls()
    {
        foreach (var w in wallObjects)
            Object.Destroy(w);

        wallObjects.Clear();
    }
}