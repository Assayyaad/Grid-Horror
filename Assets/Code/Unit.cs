using UnityEngine;

public abstract class Unit<T> : Singleton<T>
    where T : Unit<T>
{
    [SerializeField]
    protected float speed = 5f;

    public Room currentRoom { get; protected set; } = null;
    protected Room targetRoom = null;

    public virtual RoomTileType type { get; }

    protected virtual void Start()
    {
        this.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.Tiles[(int)this.type];
    }

    protected virtual void Update()
    {
        if (this.targetRoom != null)
        {
            //this.currentRoom.Exit();
            this.MoveTowardsRoom();

            return;
        }

        this.targetRoom = this.ChooseTargetRoom();
    }

    private void MoveTowardsRoom()
    {
        Vector2Int targetPos = this.targetRoom.position;
        this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(targetPos.x + 0.5f, targetPos.y + 0.5f, 0), Time.deltaTime * this.speed);

        // Check if the unit has reached the target room
        if (Vector3.Distance(this.transform.position, new Vector3(targetPos.x + 0.5f, targetPos.y + 0.5f, 0)) < 0.1f)
        { this.EnterRoom(); }
    }

    private void EnterRoom()
    {
        this.currentRoom = this.targetRoom;
        this.targetRoom = null;

        this.currentRoom.Enter();
    }

    protected abstract Room ChooseTargetRoom();
}
