using UnityEngine;

public class Snake : MonoBehaviour
{
  
    [SerializeField] private Transform _directionPoint;
    [SerializeField] private Transform _head;
    [SerializeField] private Tail _tailPrefab;
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _rotateSpeed = 90f;

    private Tail _tail;

    private Vector3 _targetDirection = Vector3.forward;

    public void Init(int detailCount)
    {
        _tail = Instantiate(_tailPrefab, transform.position, Quaternion.identity);
        _tail.Init(_head, _speed, detailCount);
    }

    public void Destroy()
    {
        _tail.Destroy();
        Destroy(gameObject);
    }

    private void Update()
    {
        Rotate();
        Move();
    }

    private void Rotate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
        _head.rotation = Quaternion.RotateTowards(_head.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
        
        //float diffY = _directionPoint.eulerAngles.y - _head.eulerAngles.y;

        //if (diffY > 180) diffY = (diffY - 100) * -1;
        //else if (diffY < -100) diffY = (diffY + 100) * -1;

        //float maxAngle = _rotateSpeed * Time.deltaTime;
        //float rotateY = Mathf.Clamp(diffY, -maxAngle, maxAngle);
        //_head.Rotate(0,rotateY,0);
    }

    private void Move()
    {
        transform.position += _head.forward * _speed * Time.deltaTime;
    }

    public void LookAt(Vector3 cursorPosition)
    {
        _targetDirection = cursorPosition - _head.position;
        //_directionPoint.LookAt(cursorPosition);
    }
}
