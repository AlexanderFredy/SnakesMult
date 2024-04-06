using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    [SerializeField] private RectTransform pointer;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private TextMeshProUGUI lable;

    private Transform _target;
    private Transform _playerTransform;
    private Camera _camera;

    public void Init(Transform playerTrans, Transform target, Transform container, Player player)
    {
        _playerTransform = playerTrans;
        _target = target;
        transform.SetParent(container);
        lable.text = player.login;
        lable.color = MultiplayerManager.Instance.playerSkins[player.c].color;
        _camera = Camera.main;
    }

    private void Update()
    {
        if (!_target)
        {
            Destroy(gameObject);
            return;
        }
       
        Vector3 toPosition = _target.position;
        toPosition.y = 0;
        Vector3 fromPosition = _playerTransform.position;
        fromPosition.y = 0;
        Vector3 dir = (toPosition - fromPosition).normalized;
        float angle = Vector3.Angle(toPosition - fromPosition, transform.right);
        if (toPosition.z - fromPosition.z > 0)
            arrow.transform.eulerAngles = new Vector3(0, 0, 90 + angle);
        else
            arrow.transform.eulerAngles = new Vector3(0, 0, 90 - angle);

        //print(angle + "   " + dir);

        Vector3 targetPositionScreenPoint = _camera.WorldToScreenPoint(_target.position);
        bool isOffScreen = targetPositionScreenPoint.x <= 0 || targetPositionScreenPoint.x >= Screen.width
                            || targetPositionScreenPoint.y <= 0 || targetPositionScreenPoint.y >= Screen.height;

        arrow.GetComponent<Image>().enabled = isOffScreen;
        lable.enabled = isOffScreen;

        if (isOffScreen)
        {           
            Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
            cappedTargetScreenPosition.x = Screen.width / 2 * (1 + dir.x);
            if (cappedTargetScreenPosition.x <= Screen.width * 0.02f)
                cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x,Screen.width*0.02f, Screen.width * 0.05f);
            else if (cappedTargetScreenPosition.x >= Screen.width * 0.98f)
                cappedTargetScreenPosition.x = Mathf.Clamp(cappedTargetScreenPosition.x, Screen.width * 0.95f, Screen.width * 0.98f);
            cappedTargetScreenPosition.y = Screen.height / 2 * (1 + dir.z);
            if (cappedTargetScreenPosition.y <= Screen.height * 0.02f)
                cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, Screen.height * 0.02f, Screen.height * 0.05f);
            else if (cappedTargetScreenPosition.x >= Screen.height * 0.98f)
                cappedTargetScreenPosition.y = Mathf.Clamp(cappedTargetScreenPosition.y, Screen.height * 0.95f, Screen.height * 0.98f);

            pointer.position = cappedTargetScreenPosition;
        }        
    }  
}
