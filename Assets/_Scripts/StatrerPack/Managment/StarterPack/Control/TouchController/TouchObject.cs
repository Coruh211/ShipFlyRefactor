using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchObject : MonoBehaviour
{
    [Header("Выберите вариант расположения значений нажатия")]
    [SerializeField] ChoosenVector _chooseVector = ChoosenVector.XYZ;

    [Header("Вводить \"1\" в направление которое хотите двигать")]
    [SerializeField] private Vector3 _movedVector;

    [SerializeField] private float _speedMoved = 1f;

    private Vector3 PointVector;
    public void MouseDown()
    {

    }

    public void MouseUp()
    {

    }

    public void MouseDrag()
    {
        PointVector = TouchController.Instance.GetTouchPosition();
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            GetMovedPosition(PointVector),
            _speedMoved * Time.deltaTime);
    }

    private Vector3 GetMovedPosition(Vector3 point)
	{
        Vector3 nextVector;
		switch (_chooseVector)
		{
            case ChoosenVector.XYZ:
                nextVector = new Vector3(point.x, point.y, point.z);
                break;
            case ChoosenVector.XZY:
                nextVector = new Vector3(point.x, point.z, point.y);
                break;
            case ChoosenVector.YXZ:
                nextVector = new Vector3(point.y, point.x, point.z);
                break;
            case ChoosenVector.YZX:
                nextVector = new Vector3(point.y, point.z, point.x);
                break;
            case ChoosenVector.ZXY:
                nextVector = new Vector3(point.z, point.x, point.y);
                break;
            case ChoosenVector.ZYX:
                nextVector = new Vector3(point.z, point.y, point.x);
                break;
            default:
                goto case ChoosenVector.XYZ;
        }

        var vecX = _movedVector.x == 1 ? nextVector.x : transform.localPosition.x;
        var vecY = _movedVector.y == 1 ? nextVector.y : transform.localPosition.y;
        var vecZ = _movedVector.z == 1 ? nextVector.z : transform.localPosition.z;

        return new Vector3(vecX,vecY,vecZ);
	}

    private enum ChoosenVector
	{
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZYX,
        ZXY,
	}
}
