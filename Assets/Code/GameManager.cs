using TMPro;

using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Sprite[] Tiles => this.themes[this.theme].tiles;
    public int MaxShards => MapGenerator.Instance.shards;
    public bool AllShardsCollected => this.shardsCollected >= this.MaxShards;

    [SerializeField]
    private TextMeshProUGUI shardsText;

    [SerializeField]
    private Theme[] themes = new Theme[6];

    private byte theme = 0;
    private int shardsCollected;

    protected override void Awake()
    {
        base.Awake();

        this.theme = (byte)Random.Range(0, this.themes.Length);
        this.theme = 2;

        //PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    private void Start()
    {
        Room.ShardCollected += this.OnShardsCollected;
        Room.GameWon += this.OnGameWon;
        Player.PlayerDied += this.OnPlayerDied;

        this.UpdateHUD();

        //GameManager.Instance.;
        //Player.Instance.;
        //Monster.Instance.;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Room.ShardCollected -= this.OnShardsCollected;
        Room.GameWon -= this.OnGameWon;
        Player.PlayerDied -= this.OnPlayerDied;
    }

    private void UpdateHUD()
    {
        this.shardsText.text = $"Shards: {this.shardsCollected}/{this.MaxShards}";
    }

    private void OnShardsCollected()
    {
        this.shardsCollected++;

        if (this.AllShardsCollected)
        {
            MapGenerator.Instance.OpenExit();
        }

        this.UpdateHUD();
    }

    private void OnGameWon()
    {
        Debug.Log("You Won");
        Player.Instance.gameObject.SetActive(false);
    }

    private void OnPlayerDied()
    {
        Debug.Log("You Died");
        Player.Instance.gameObject.SetActive(false);
    }
}