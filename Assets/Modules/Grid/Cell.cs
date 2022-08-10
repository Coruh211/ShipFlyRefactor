using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool IsFree = false;
	public void SetSize(Vector3 newSize) => transform.localScale = newSize;
}