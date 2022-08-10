using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyArrow : MonoBehaviour
{
    [SerializeField] private Arrow arrowPrefab;

    [SerializeField] private float borderX = 80f;
    [SerializeField] private float borderY = 80f;

    [SerializeField] private float borderShowX = -10f;
    [SerializeField] private float borderShowY = -10f;

    private List<Arrow> arrows = new List<Arrow>();

    private void Update()
    {
        foreach (Arrow arrow in arrows)
        {
            if (!arrow.Target || !arrow.Target.gameObject.activeSelf)
            {
                arrow.SetActiveArrow(false);
                continue;
            }

            Vector3 screenPosition = arrow.ScreenPosition();
            if ((screenPosition.x > Screen.width - borderShowX || screenPosition.x < borderShowX) ||
                (screenPosition.y > Screen.height - borderShowY || screenPosition.y < borderShowY))
            {
                arrow.SetActiveArrow(true);
                arrow.Rotation();
                arrow.transform.position = new Vector3(Mathf.Clamp(screenPosition.x, borderX, Screen.width - borderX), Mathf.Clamp(screenPosition.y, borderY, Screen.height - borderY));
            }
            else
            {
                arrow.SetActiveArrow(false);
            }
        }
    }

    public void AddArrow(Transform target, Transform player, Color color)
    {
        Arrow arrowGo = Instantiate(arrowPrefab, transform);
        arrowGo.Init(target, player, color);
        arrows.Add(arrowGo);
    }
}
