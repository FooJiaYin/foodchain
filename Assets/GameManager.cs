using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform _boardRoot;

    // [SerializeField] private Tile _tilePrefab;
    public Tile[] _tilePrefabs = new Tile[5];

    [SerializeField] private GameObject StartButton;
    [SerializeField] private GameObject TutorialButton;
    [SerializeField] private GameObject Background;
    [SerializeField] private Text HelpText;
    private TutorialController Tutorial;
    private GameObject TitleImage;
    private GameObject GameOverDisplay;
    private GameObject FinalScoreDisplay;

    private AudioSource BGMAudio;

    [SerializeField] private Text _elapsedTimeFromStartText;
    [SerializeField] private int eatScore;
    [SerializeField] private int mergeScore;
    private Text _ScoreText;
    private int score = 0;

    private int state = 0;
    private int lastState = 0;
    private const int Width = 6;
    private const int Height = 10;

    private const int MiddleX = Width / 2;
    private const int Top = Height - 1;

    private Tile currentTile;
    private Tile[,] tilesMap = new Tile[Width, Height];
    private List<Tile> tiles = new List<Tile>();

    [SerializeField] private float MaxElapsedTimePerStep = 0.18f;
    [SerializeField] private float MaxElapsedTimeFromStart = 5f;

    private float _elapsedTimeFromStart;
    private float elapsedTime;

    private float ElapsedTimeFromStart
    {
        get => _elapsedTimeFromStart;

        set
        {
            _elapsedTimeFromStart = value;
            _elapsedTimeFromStartText.text = (MaxElapsedTimeFromStart - _elapsedTimeFromStart).ToString("F0");
        }
    }

    void Awake() {
        Tutorial = Object.FindObjectOfType<TutorialController>();
        BGMAudio = gameObject.GetComponent<AudioSource>();
        TitleImage = GameObject.Find("TitleImage");
        GameOverDisplay = GameObject.Find("GameOver");
        FinalScoreDisplay = GameObject.Find("FinalScore");
		StartButton.GetComponent<Button>().onClick.AddListener(onStartClick);
		TutorialButton.GetComponent<Button>().onClick.AddListener(displayTutorial);
    }

	private void onStartClick()
    {
        this.state = 1;
        this.score = 0;
        BGMAudio.Stop();
        StartButton.SetActive(false);
        TutorialButton.SetActive(false);
        TitleImage.SetActive(false);
        Background.SetActive(true);
        this._ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        this._ScoreText.text = this.score.ToString();
        this.HelpText.text = "← : <b>MOVE LEFT</b>  → : <b>MOVE RIGHT</b>  SPACE: <b>TUTORIAL</b>";
        this.currentTile = CreateNewTile();

        ElapsedTimeFromStart = 0f;
        this.elapsedTime = 0f;

        // StartCoroutine(StartGame());
	}
	private void displayTutorial()
    {
        Debug.Log("displayTutorial");
        this.lastState = this.state;
        this.state = 2;
        this.HelpText.gameObject.SetActive(true);
        HelpText.text = "← : <b>LAST PAGE</b>  → : <b>NEXT PAGE</b>  SPACE : <b>EXIT</b>";
        Tutorial.gameObject.SetActive(true);
        if(this.lastState == 1) {
            foreach (var t in this.tilesMap)
            {
                if(t != null) t.gameObject.SetActive(false);
            }        
        }
        // StartCoroutine(StartGame());
	}

    // private IEnumerator StartGame() {
    //     while(true) {
    //         yield return PlayGame();
    //     }
    // }

    private IEnumerator Start()
    {
        Transition videoTransition = GameObject.Find("Video").GetComponent<Transition>();
        goToMenu();
        StartButton.SetActive(false);
        TutorialButton.SetActive(false);
        GameObject.Find("Video").SetActive(true);
        yield return new WaitForSeconds(30f);
        videoTransition.Exit();
        StartButton.SetActive(true);
        TutorialButton.SetActive(true);
    }

    private void goToMenu() 
    {
        this.state = 0;
        Background.SetActive(false);
        Tutorial.gameObject.SetActive(false);
        GameOverDisplay.SetActive(false);
        FinalScoreDisplay.SetActive(false);
        TitleImage.SetActive(true);
        StartButton.SetActive(true);
        TutorialButton.SetActive(true);
        this.BGMAudio.Play();
    }

    void Update() 
    {
        if(this.state == 1) {
            if(ElapsedTimeFromStart <= MaxElapsedTimeFromStart)
            {
                ElapsedTimeFromStart += Time.deltaTime;
                this.elapsedTime += Time.deltaTime;

                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveTileLeft(this.currentTile);
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveTileRight(this.currentTile);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    displayTutorial();
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    if (!MoveTileDown(this.currentTile))
                    {
                        this.currentTile = CreateNewTile();

                        if (this.currentTile == null) gameOver(false);
                    }
                }
                
                if (this.elapsedTime >= MaxElapsedTimePerStep)
                {
                    if (!MoveTileDown(this.currentTile))
                    {
                        this.currentTile = CreateNewTile();

                        if (this.currentTile == null) gameOver(false);
                    }

                    this.elapsedTime = 0;

                    // printMap();
                }
            } else {
                gameOver(true);
            }
        }
        else if (this.state == 2) 
        {
            // Tutorial.gameObject.SetActive(false);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.state = this.lastState;
                if(this.lastState == 1) {
                    this.HelpText.text = "← : <b>MOVE LEFT</b>  → : <b>MOVE RIGHT</b>  SPACE: <b>TUTORIAL</b>";
                    foreach (var t in this.tilesMap)
                    {
                        if(t != null) t.gameObject.SetActive(true);
                    }
                }
                else {
                    this.HelpText.gameObject.SetActive(false);
                }
            }
        }
        else if(this.state == 3) {
            if (Input.anyKeyDown)
            {
                goToMenu();
            }
        }
    }

    // private IEnumerator PlayGame()
    // {
    
    //     // GameOver

        
    // }

    private void updateScore(int change) {
        this.score += change;
        this._ScoreText.text = this.score.ToString();
        if(this.score <= 0) gameOver(false);
    }

    private void printMap() {
        string tilesMapStr = "";
        for(int i = Height - 1; i >= 0; i--) {
            for (int j = 0; j < Width; j++)
            {
                if(this.tilesMap[j, i] == null) tilesMapStr += '-';
                else tilesMapStr += this.tilesMap[j, i].type.ToString();
            }
            tilesMapStr += ' ';
        }
        tilesMapStr += '\n';
        // foreach(Tile tile in this.tiles) {
        //     tilesMapStr += tile.type.ToString() + '(' + (Height - tile.Y).ToString() + ',' + tile.X.ToString() + ") ";
        // }
        Debug.Log(tilesMapStr);
    }
    private Tile CreateNewTile()
    {
        if (this.tilesMap[MiddleX, Top] != null) return null; 
        
        int randomId = Random.Range(0, 5);
        return createTile(randomId, MiddleX, Top);
    }

    private Tile createTile(int type, int x, int y)
    {
        var tile = Instantiate(_tilePrefabs[type], _boardRoot);
        tile.X = x;
        tile.Y = y;
        return tile;
    }

    private bool CanTileMoveTo(int x, int y, int width)
    {
        if (x < 0 || x >= width || y < 0)
            return false;

        return this.tilesMap[x, y] == null;
    }

    private void MoveTileLeft(Tile tile)
    {
        if (!CanTileMoveTo(tile.X - 1, tile.Y, Width)) return;
        tile.X--;
    }
    private void MoveTileRight(Tile tile)
    {
        if (!CanTileMoveTo(tile.X + 1, tile.Y, Width)) return;
        tile.X++;
    }

    private bool MoveTileDown(Tile tile)
    {
        if (CanTileMoveTo(tile.X, tile.Y - 1, Width))
        {
            tile.Y--;
            return true;
        }
        onHit(tile);
        return false;
    }

    private void onHit(Tile tile) {
        Debug.Log("Hited");
        printMap();
        tile.Hit();
        List<Tile> sameTileList = getSameAdjacent(tile, new List<Tile>());
        // Debug.Log(sameTileList.Count);
        if(sameTileList == null) {
            Debug.Log("Eated");
        }
        else if(sameTileList.Count() > 1) {
            updateScore(sameTileList.Count());
            Destroy(tile.gameObject);
            foreach(Tile sameTile in sameTileList) {
                Debug.Log(sameTile.X);
                destroyTile(sameTile);
            }
            Tile newTile = createTile((tile.type+1)%5, tile.X, tile.Y);
            addTile(newTile);
            Debug.Log("Merged");
        }
        else {
            addTile(tile);
            Debug.Log("Landed");
        }
        printMap();
        updateTiles();
        Debug.Log("Update Tiles");
        printMap();
    }

    private void updateTiles() {
        foreach(Tile tile in this.tilesMap) {
            if(tile != null && CanTileMoveTo(tile.X, tile.Y - 1, Width)) {
                var cont = true;
                while(cont) {
                    this.tilesMap[tile.X, tile.Y] = tile;
                    this.tilesMap[tile.X, tile.Y+1] = null;
                    cont = MoveTileDown(tile);
                }
                // return;
                List<Tile> sameTileList = getSameAdjacent(tile, new List<Tile>());
            // Debug.Log(sameTileList.Count);
                if(sameTileList == null) {
                    Debug.Log("Eated");
                }
                else if(sameTileList.Count() > 1) {
                    Destroy(tile.gameObject);
                    foreach(Tile sameTile in sameTileList) {
                        Debug.Log(sameTile.X);
                        destroyTile(sameTile);
                    }
                    Tile newTile = createTile((tile.type+1)%5, tile.X, tile.Y);
                    addTile(newTile);
                    Debug.Log("Merged");
                }
            }
        }
    }

    private void addTile(Tile tile) {
        this.tilesMap[tile.X, tile.Y] = tile;
        // this.tiles.Add(tile);
    }

    private void destroyTile(Tile tile) {
        // this.tiles.Remove(tile);
        Destroy(tile.gameObject);
        this.tilesMap[tile.X, tile.Y] = null;
    }

    private bool eat(Tile tile)
    {
        int x = tile.X;
        int y = tile.Y;
        List<Tile> adjacentTiles = new List<Tile>();

        if(x > 0) adjacentTiles.Add(this.tilesMap[x-1, y]);
        if(x < Width-1) adjacentTiles.Add(this.tilesMap[x+1, y]);
        if(y > 0) adjacentTiles.Add(this.tilesMap[x, y-1]);
        
        foreach(Tile adjacentTile in adjacentTiles) {
            if(adjacentTile == null) continue;
            if (adjacentTile.type == (tile.type-1)%5) {
                destroyTile(adjacentTile);
                Debug.Log("Eated");
                printMap();
            } else if (adjacentTile.type == (tile.type+1)%5) {
                destroyTile(tile);
                return false;
            }
        }
        return true;
    }

    private List<Tile> getSameAdjacent(Tile tile, List<Tile> sameTileList)
    {
        int x = tile.X;
        int y = tile.Y;
        List<Tile> sameTileList_ = new List<Tile>();
        List<Tile> adjacentTiles = new List<Tile>();

        if(x > 0) adjacentTiles.Add(this.tilesMap[x-1, y]);
        if(x < Width-1) adjacentTiles.Add(this.tilesMap[x+1, y]);
        if(y > 0) adjacentTiles.Add(this.tilesMap[x, y-1]);
        
        foreach(Tile adjacentTile in adjacentTiles) {
            if(adjacentTile == null) continue;
            if(adjacentTile.type == tile.type) {
                if(!sameTileList.Any(other => other.X == adjacentTile.X && other.Y == adjacentTile.Y)) {
                    sameTileList_.Add(adjacentTile);
                    sameTileList.Add(adjacentTile);
                }
            } else if (tile.type == (adjacentTile.type+1)%5) {
                destroyTile(adjacentTile);
                updateScore(eatScore);
            } else if (adjacentTile.type == (tile.type+1)%5) {
                updateScore(eatScore);
                Destroy(tile.gameObject);
                return null;
            }
        }
        foreach(Tile sameTile in sameTileList_) {
            sameTileList = getSameAdjacent(sameTile, sameTileList);
        }
        // Debug.Log(sameTileList.Count);
        return sameTileList;
    }

    private void gameOver(bool success) {
        this.state = 3;
        this.HelpText.text = "<b>Press Any Key to Continue</b>";
        ElapsedTimeFromStart = Mathf.Min(ElapsedTimeFromStart, MaxElapsedTimeFromStart);

        // yield return new WaitForSeconds(1.0f);

        Destroy(this.currentTile?.gameObject);

        foreach (var t in this.tilesMap)
        {
            if(t != null) destroyTile(t);
        }
        if (!success) GameOverDisplay.SetActive(true);
        else {
            FinalScoreDisplay.SetActive(true);
            GameObject.Find("ScoreTextFinal").GetComponent<Text>().text = this.score.ToString();
        }
    }

    public void Fill()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var tile = Instantiate(_tilePrefabs[0], _boardRoot);
                tile.X = x;
                tile.Y = y;
            }
        }
    }
}
