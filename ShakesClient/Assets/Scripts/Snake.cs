using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float Speed { get { return _speed; } }

    [SerializeField] private Transform _head;
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private float _speed = 2f;

    private Tail _tail;

    private Vector3 _targetDirection = Vector3.forward;

    public void Init(int detailCount, int skinIndex)
    {
        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount, skinIndex);

        GetComponent<SetSkins>().Set(MultiplayerManager.Instance.playerSkins[skinIndex]);        
    }

    public void SetDetailCount(int detailCount)
    { 
        _tail.SetDetailCount(detailCount);
    }

    public void Destroy()
    {
        _tail.Destroy();
        Destroy(gameObject);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += _head.forward * _speed * Time.deltaTime;
    }

    public void SetRotation(Vector3 pointToLook)
    { 
        _head.LookAt(pointToLook);
    }

}
