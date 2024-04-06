using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public float Speed { get { return _speed; } }

    [SerializeField] private int _playerLayer = 6;
    [SerializeField] private Tail _tailPrefab;
    [field: SerializeField] public Transform Head { get; private set; }
    [SerializeField] private float _speed = 2f;

    public Tail Tail {get; private set; }

    private Vector3 _targetDirection = Vector3.forward;

    public void Init(int detailCount, int skinIndex, bool isPlayer = false)
    {
        if (isPlayer)
        {
            gameObject.layer = _playerLayer;
            var childrens = GetComponentsInChildren<Transform>();
            for ( int i = 0; i< childrens.Length; i++)
            {
                childrens[i].gameObject.layer = _playerLayer;
            }
        }
               
        Tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        Tail.Init(Head, _speed, detailCount, skinIndex, _playerLayer, isPlayer);

        GetComponent<SetSkins>().Set(MultiplayerManager.Instance.playerSkins[skinIndex]);        
    }

    public void SetDetailCount(int detailCount)
    { 
        Tail.SetDetailCount(detailCount);
    }

    public void Destroy()
    {
        Tail.Destroy();
        Destroy(gameObject);
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        transform.position += Head.forward * _speed * Time.deltaTime;
    }

    public void SetRotation(Vector3 pointToLook)
    { 
        Head.LookAt(pointToLook);
    }

}
