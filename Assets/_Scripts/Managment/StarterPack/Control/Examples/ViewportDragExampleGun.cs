using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ViewportDragExampleGun : MonoBehaviour, IInputHandler
{
    [SerializeField] private GameObject gun;
    [SerializeField] private float rotationSpeed = 7;
    
    [Tooltip("Чувствительность поворота для каждой оси")]
    [SerializeField] private Vector2 sensetivity = new Vector2(70, 70);
    
    [Tooltip("Большие максимальные углы поворота. Не зависят от поворота парента и начального угла поворота оружия")]
    [SerializeField] private Vector2 positiveMaxAngles = new Vector2(45, 45);
    
    [Tooltip("Меньшие максимальные углы поворота. Не зависят от поворота парента и начального угла поворота оружия")]
    [SerializeField] private Vector2 negativeMaxAngles = new Vector2(-45, -45);
    
    [Tooltip("Если true, то абсолютная sensetivity по высоте будет такой же, как и по ширине, какой бы высота не была")]
    [SerializeField] private bool screenRatioFreedom = true;

    private Vector2 positiveMaxAnglesCurrent;
    private Vector2 negativeMaxAnglesCurrent;
    private Vector2 startLocalEuler;
    private Vector3 currentAngle;

    private void Start()
    {
        CalculateCurrentMaxAngles();
    }

    [InputCallback(EventTriggerType.Drag)]
    public void Drag(PointerEventData data)
    {
        float deltaX = -data.delta.x * sensetivity.x;
        float deltaY = data.delta.y * sensetivity.y * (screenRatioFreedom ? (float)Screen.height / Screen.width : 1);

        if (deltaX > 0 && currentAngle.y - deltaX > negativeMaxAnglesCurrent.y ||
            deltaX < 0 && currentAngle.y - deltaX < positiveMaxAnglesCurrent.y)
            currentAngle.y -= deltaX;

        if (deltaY > 0 && currentAngle.x - deltaY > negativeMaxAnglesCurrent.x ||
            deltaY < 0 && currentAngle.x - deltaY < positiveMaxAnglesCurrent.x)
            currentAngle.x -= deltaY;
    }
    
    private void Update()
    {
        gun.transform.localRotation = Quaternion.Slerp(gun.transform.localRotation, Quaternion.Euler(currentAngle), Time.deltaTime * rotationSpeed);
    }

    private void CalculateCurrentMaxAngles() // При изменении максимальных углов требуется вызвать этот метод для их перерасчёта
    {
        startLocalEuler = gun.transform.localEulerAngles.ToVector2(false);
        positiveMaxAnglesCurrent = positiveMaxAngles + startLocalEuler;
        negativeMaxAnglesCurrent = negativeMaxAngles - startLocalEuler;
        currentAngle = gun.transform.localEulerAngles;
    }
}