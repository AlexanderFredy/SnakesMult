using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private LayerMask _collisionLayer;
    [SerializeField] private float _overlapRadius = .5f;
    [SerializeField] private float _rotateSpeed = 90f;
    private Transform _snakeHead;
    private Vector3 _targetDirection = Vector3.forward;
    private float _speed;
    private bool isAlive = true;
    private string _clientID;

    public void Init(string sessionID,Transform snakeHead,float speed)
    {
        _clientID = sessionID;
        _snakeHead = snakeHead;
        _speed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) return;

        Rotate();
        Move();
        CheckExit();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        CheckCollision();
    }

    private void CheckCollision()
    {
        Collider[] colliders = Physics.OverlapSphere(_snakeHead.position, _overlapRadius, ~_collisionLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].TryGetComponent(out Apple apple))
            {
                apple.Collect();
            } else
            {
                if (colliders[i].GetComponentInParent<Snake>())
                {
                    Transform enemy = colliders[i].transform;
                    float playerAngle = Vector3.Angle(enemy.position - _snakeHead.position, transform.forward);
                    float enemyAngle = Vector3.Angle(_snakeHead.position - enemy.position, enemy.forward);
                    if (playerAngle < enemyAngle + 5)
                    {
                        Respawn();
                    }
                } else
                    Respawn();
            }
        }
    }

    private async void Respawn()
    {
        //FindAnyObjectByType<Controller>().Destroy();
        //Destroy(gameObject);

        isAlive = false;

        Snake snake = _snakeHead.GetComponentInParent<Snake>();
        Tail tail = snake.Tail;
        tail.SetDetailCount(0);

        snake.GetComponent<DeathParticle>().ShowDestroy();

        SetVisibility(transform,false);
        SetVisibility(_snakeHead, false);
        SetVisibility(tail.transform, false);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"id",_clientID}
        };
        MultiplayerManager.Instance.SendMessage("respawn", data);

        await Task.Delay(2000);

        var newPosition = new Vector3(UnityEngine.Random.Range(-64, 64), 0, UnityEngine.Random.Range(-64, 64));
        transform.position = newPosition;
        snake.transform.position = newPosition;
        tail.transform.position = newPosition;

        SetVisibility(transform, true);
        SetVisibility(_snakeHead, true);
        SetVisibility(tail.transform, true);

        MultiplayerManager.Instance.SendMessage("enemyAlive", data);

        isAlive = true;
    }

    private void SetVisibility(Transform part, bool isVision)
    {
        MeshRenderer[] renderers = part.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = isVision;
        }
    }

    private void Rotate()
    {
        Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * _rotateSpeed);
    }

    private void Move()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void CheckExit()
    {
        if (Math.Abs(_snakeHead.position.x) > 128 || Math.Abs(_snakeHead.position.z) > 128)
            Respawn();
    }


    public void SetTargetDirection(Vector3 pointToLook)
    {
        _targetDirection = pointToLook - transform.position;
    }

    public void GetMoveInfo(out Vector3 position)
    {
        position = transform.position;
    }
}
