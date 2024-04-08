using Colyseus.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private string _clientID;
    private Player _player;
    private Snake _snake;
    public bool IsAlive { get; private set; } = true;
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

    public void ShowDeath()
    {
        IsAlive = false;

        _snake.Tail.SetDetailCount(0);

        _snake.GetComponent<DeathParticle>().ShowDestroy();

        SetVisibility(transform, false);
        SetVisibility(_snake.transform, false);
        SetVisibility(_snake.Tail.transform, false);
    }

    public async void ShowLife()
    {
        await Task.Delay(1000);

        SetVisibility(transform, true);
        SetVisibility(_snake.transform, true);
        SetVisibility(_snake.Tail.transform, true);

        IsAlive = true;
    }

    private void SetVisibility(Transform part, bool isVision)
    {
        MeshRenderer[] renderers = part.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = isVision;
        }
    }

    public void Destroy()
    {
        _player.OnChange -= OnChange;
        _snake?.Destroy();
    }
}
