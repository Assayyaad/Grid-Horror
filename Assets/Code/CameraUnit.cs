using UnityEngine;

public abstract class CameraUnit<T> : Unit<T>
    where T : CameraUnit<T>
{
    private new Camera camera;

    protected override void Awake()
    {
        base.Awake();
        this.camera = Camera.main;
    }

    private void LateUpdate()
    {
        Vector3 temp = this.transform.position;
        temp.z = -10;
        this.camera.transform.position = temp;
    }
}
