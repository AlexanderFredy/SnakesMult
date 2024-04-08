using TMPro;
using UnityEngine;

public class NameLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmp;
    [SerializeField] private float upOffset = 50f;
    [SerializeField] private RectTransform rectTrans;
    private Transform _owner;
    private EnemyController _enemy;

    public void Init(Transform owner, string name)
    {
        _owner = owner;
        _tmp.text = name;

        _enemy = _owner.GetComponent<EnemyController>();
    }

    void Update()
    {
        if (_enemy != null && _enemy.IsAlive)
        {
            _tmp.enabled = true;
            Vector3 pos = Camera.main.WorldToScreenPoint(_owner.position) + new Vector3(0f, upOffset);
            rectTrans.position = pos;
        }
        else
            _tmp.enabled = false;
    }
}
