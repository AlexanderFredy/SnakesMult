using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private string _clientID;
    private Player _player;
    private Snake _snake;
    public void Init(string clientID, Player player, Snake snake)
    {
        _clientID = clientID;
        _player = player;
        _snake = snake;
        player.OnChange += OnChange;
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
                    MultiplayerManager.Instance.UpdateScore(_clientID, (ushort)changes[i].Value);
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
        _player.OnChange -= OnChange;
        _snake?.Destroy();
    }
}
