using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Colyseus;
using System;
using Unity.VisualScripting;
using System.Linq;
using TMPro;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    #region Server
    [Header("Visual elements")]
    [SerializeField] public Material[] playerSkins;
    [SerializeField] private TextMeshProUGUI _counterDown;
    [SerializeField] private Indicator indicatorPrefab;
    [SerializeField] private RectTransform _indicatorContainer;

    private const string GameRoomName = "state_handler";
    private ColyseusRoom<State> _room;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        InitializeClient();
        Connection();
    }

    private async void Connection()
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "login", PlayerSettings.Instance.Login },
            { "skinsCount", playerSkins.Length }
        };

        _room = await client.JoinOrCreate<State>(GameRoomName,data);
        _room.OnStateChange += OnChange;

        _room.OnMessage<string>("time", ChangeTime);
    }

    private void ChangeTime(string seconds)
    {
        if (int.TryParse(seconds, out int s))
        {
            TimeSpan time = TimeSpan.FromSeconds(s);
            _counterDown.text = time.ToString(@"mm\:ss");
        }
        else
            _counterDown.text = "00:00";
    }

    private void OnChange(State state, bool isFirstState)
    {
        if (isFirstState == false) return;
        _room.OnStateChange -= OnChange;

        CreatePlayer(state.players[_room.SessionId]);

        state.players.ForEach((key, player) =>
        {
            if (key != _room.SessionId) CreateEnemy(key, player);//CreatePlayer(player);
            //else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;

        _room.State.apples.ForEach(CreateApple);
        _room.State.apples.OnAdd += (key,apple) => CreateApple(apple);
        _room.State.apples.OnRemove += RemoveApple;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        LeveRoom();
    }

    public void LeveRoom()
    {
        _room?.Leave();
    }

    public void SendMessage(string key, Dictionary<string,object> data)
    {
        _room.Send(key, data);
    }
    #endregion

    #region Player
    [Header("Logic")]
    [SerializeField] private PlayerAim _palyerAim;
    [SerializeField] private Controller _controllerPrefab;
    [SerializeField] private Snake _snakePrefab;
    private Snake playerSnake;

    private void CreatePlayer(Player player)
    {
        Vector3 position = new Vector3(player.x,0,player.z);
        Quaternion quaternion = Quaternion.identity;

        playerSnake = Instantiate(_snakePrefab,position, quaternion);
        playerSnake.Init(player.d, player.c, true);

        PlayerAim aim = Instantiate(_palyerAim, position, quaternion);
        aim.Init(playerSnake.Head,playerSnake.Speed);

        Controller controller = Instantiate(_controllerPrefab);
        controller.Init(_room.SessionId,aim, player,playerSnake);

        AddLeader(_room.SessionId, player);

        //Instantiate(indicatorPrefab).Init(snake.transform, GameObject.Find("TestTarget").transform, _indicatorContainer, player);
    }

    #endregion

    #region Enemy
    [SerializeField] private NameLabel _nameLabelPrefab;
    [SerializeField] private Canvas _canvas;
    private Dictionary<string, EnemyController> _enemies = new();

    private void CreateEnemy(string key, Player player)
    {
        Vector3 position = new Vector3(player.x, 0, player.z);

        Snake snake = Instantiate(_snakePrefab, position, Quaternion.identity);
        snake.Init(player.d, player.c);

        NameLabel nl = Instantiate(_nameLabelPrefab, position, Quaternion.identity, _canvas.transform);
        nl.Init(snake.transform, player.login);

        Instantiate(indicatorPrefab).Init(playerSnake.transform, snake.transform, _indicatorContainer, player);

        EnemyController enemy = snake.AddComponent<EnemyController>();
        enemy.Init(key,player, snake);

        _enemies.Add(key, enemy);

        AddLeader(key, player);
    }

    private void RemoveEnemy(string key, Player value)
    {
        RemoveLeader(key);
        
        if (_enemies.ContainsKey(key) == false)
        {
            Debug.LogError("This enemy is not exist");
            return;
        }

        EnemyController enemy = _enemies[key];
        _enemies.Remove(key);
        enemy.Destroy();
    }

    #endregion

    #region Apple
    [SerializeField] private Apple _applePrefab;
    private Dictionary<Vector2Float, Apple> _apples = new Dictionary<Vector2Float, Apple>();

    private void CreateApple(Vector2Float vector2Float)
    {
        Vector3 position = new Vector3(vector2Float.x,0,vector2Float.z);
        Apple apple = Instantiate(_applePrefab, position, Quaternion.identity);
        apple.Init(vector2Float);
        _apples.Add(vector2Float, apple);
    }
    
    private void RemoveApple(int key, Vector2Float vector2Float)
    {
        if (_apples.ContainsKey(vector2Float) == false) return;

        _apples.Remove(vector2Float);
        _apples[vector2Float].Destroy();
    }

    #endregion

    #region LeaderBoard
    [Header("Leader board")]
    [SerializeField] private Text _text;

    private class LoginScorePair
    {
        public string login;
        public float score;
    }

    Dictionary<string, LoginScorePair> _leaders = new Dictionary<string, LoginScorePair>();

    private void AddLeader(string sessionID, Player player)
    {
        if (_leaders.ContainsKey(sessionID)) return;

        _leaders.Add(sessionID, new LoginScorePair
        {
            login = player.login,
            score = player.score
        });

        UpdateBoard();
    }

    private void RemoveLeader(string sessionID)
    {
        if (_leaders.ContainsKey(sessionID) == false) return;
        _leaders.Remove(sessionID);

        UpdateBoard();
    }

    public void UpdateScore(string sessionID, int score)
    {
        if (_leaders.ContainsKey(sessionID) == false) return;

        _leaders[sessionID].score = score;
        UpdateBoard();
    }

    private void UpdateBoard()
    {
        int topCount = Mathf.Clamp(_leaders.Count,0,8);
        var top8 = _leaders.OrderByDescending(pair => pair.Value.score).Take(topCount);

        string text = "";
        int i = 1;
        foreach (var item in top8)
        {
            text += $"{i}. {item.Value.login}: {item.Value.score}\n";
            i++;
        }

        _text.text = text;
    }
    #endregion

}
