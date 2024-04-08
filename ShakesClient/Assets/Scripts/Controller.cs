using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _cameraOffsetY = 15f;
    [SerializeField] private Transform _cursor;
    private MultiplayerManager _multiplayerManager;
    private PlayerAim _playerAim;
    private string _clientID;
    private Player _player;
    private Camera _camera;
    private Snake _snake;
    private Plane _plane;

    public void Init(string clientID, PlayerAim aim, Player player, Snake snake)
    {
        _multiplayerManager = MultiplayerManager.Instance;

        _playerAim = aim;
        _clientID = clientID;
        _player = player;
        _snake = snake;
        _camera = Camera.main;
        _plane = new Plane(Vector3.up,Vector3.zero);

        _camera.transform.parent = _snake.transform;
        _camera.transform.localPosition = Vector3.up * _cameraOffsetY;

        _player.OnChange += OnChange;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount > 0)
            {
                MoveCursor(Input.GetTouch(0).position);
                _playerAim.SetTargetDirection(_cursor.position);
            }            
        } else if (Input.GetMouseButton(0))
        {
            MoveCursor(Input.mousePosition);
            _playerAim.SetTargetDirection(_cursor.position);
        }

        SendMove();
    }

    private void SendMove()
    {
        _playerAim.GetMoveInfo(out Vector3 position);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x",position.x},
            {"z",position.z}
        };

        _multiplayerManager.SendMessage("move", data);
    }

    private void MoveCursor(Vector3 pointerPosition)
    {
        Ray ray = _camera.ScreenPointToRay(pointerPosition);
        _plane.Raycast(ray, out float distance);
        Vector3 point = ray.GetPoint(distance);

        _cursor.position = point;
    }

    private void OnChange(List<DataChange> changes)
    {
        if (!_snake) return;

        Vector3 position = _snake.transform.position;
        for (int i = 0; i < changes.Count; i++)
        {
            switch (changes[i].Field)
            {
                case "x":
                    position.x = (float)changes[i].Value;
                    break;
                case "z":
                    position.z = (float)changes[i].Value;
                    break;
                case "d":
                    _snake.SetDetailCount((byte)changes[i].Value);
                    break;
                case "score":
                    _multiplayerManager.UpdateScore(_clientID, (ushort)changes[i].Value);
                    break;
                default:
                    Debug.LogWarning("Field " + changes[i].Field + " doesn't have a processor");
                    break;
            }

            _snake.SetRotation(position);
        }
    }

    public void Destroy()
    {
        _camera.transform.parent = null;

        _player.OnChange -= OnChange;
        _snake.Destroy();
        Destroy(gameObject);
    }

}
