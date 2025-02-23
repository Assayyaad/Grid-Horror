using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Sprite[] Tiles => this.themes[this.theme].tiles;

    [SerializeField]
    private Theme[] themes = new Theme[6];

    private byte theme = 0;
    private int shardsCollected;
    private int shardsCount;

    protected override void Awake()
    {
        base.Awake();

        this.theme = (byte)Random.Range(0, this.themes.Length);
        //PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    public void CollectShard()
    {
        this.shardsCollected++;

        if (this.shardsCollected >= MapGenerator.Instance.shards)
        {
            MapGenerator.Instance.OpenExit();
        }
    }
}