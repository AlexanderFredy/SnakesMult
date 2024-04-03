using TMPro;
using UnityEngine;

public class NameLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _tmp;
    [SerializeField] private float upOffset = 50f;
    [SerializeField] private RectTransform rectTrans;
    private Transform _owner;

    public void Init(Transform owner, string name)
    {
        _owner = owner;
        _tmp.text = name;
    }

    void Update()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(_owner.position) + new Vector3(0f, upOffset);
        rectTrans.position = pos;
    }
}
