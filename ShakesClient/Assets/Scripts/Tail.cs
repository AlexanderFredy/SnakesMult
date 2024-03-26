using System;
using System.Collections.Generic;
using UnityEngine;

public class Tail : MonoBehaviour
{
    [SerializeField] private Transform _detailPrefab;
    [SerializeField] private float _detailDistance = 1f;

    private Transform _head;  
    private float _snakeSpeed = 2f;
    private List<Vector3> _positionHistory = new();
    private List<Transform> _details = new();

    public void Init(Transform head, float speed, int detailCount)
    {
        _head = head;
        _snakeSpeed = speed;

        _details.Add(transform);
        _positionHistory.Add(_head.position);
        _positionHistory.Add(transform.position);

        SetDetailCount(detailCount);
    }

    private void SetDetailCount(int detailCount)
    {
        if (detailCount == _details.Count + 1) return;

        int diff = (_details.Count - 1) - detailCount;

        if (diff < 1)
        {
            for (int i = 0; i < -diff; i++)
                AddDetail();
        }
        else
        {
            for (int i = 0; i < diff; i++)
                RemoveDetail();
        }
    }

    private void AddDetail()
    {
        Vector3 position = _details[_details.Count - 1].position;
        Transform detail = Instantiate(_detailPrefab, position, Quaternion.identity);
        _details.Insert(0, detail);
        _positionHistory.Add(position);
    }

    private void RemoveDetail()
    {
        if (_details.Count <= 1)
        {
            Debug.LogError("Try delete detail, which is no");
            return;
        }

        Transform detail = _details[0];
        _details.Remove(detail);
        Destroy(detail.gameObject);
        _positionHistory.RemoveAt(_positionHistory.Count - 1);
    }

    private void Update()
    {
        float distance = (_head.position - _positionHistory[0]).magnitude;

        while (distance > _detailDistance)
        {
            Vector3 direction = (_head.position - _positionHistory[0]).normalized;
            _positionHistory.Insert(0, _positionHistory[0] + direction*_detailDistance);
            _positionHistory.RemoveAt(_positionHistory.Count - 1);

            distance -= _detailDistance;
        }
        
        //теперь плавно двигаем все детали
        for (int i = 0; i < _details.Count; i++)
        {
            _details[i].position = Vector3.Lerp(_positionHistory[i + 1], _positionHistory[i], distance/ _detailDistance);
        }
    }

    internal void Destroy()
    {
        for (int i = 0; i < _details.Count; i++)
        {
            Destroy(_details[i].gameObject);
        }
    }
}
