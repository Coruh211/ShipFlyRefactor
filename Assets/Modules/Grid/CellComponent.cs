using UnityEngine;

public class CellComponent : MonoBehaviour
{
	protected Vector3 _position;
	protected BoxCollider _boxCollider;

	private void OnEnable() => Init();

	protected virtual void Init()
	{
		_boxCollider = GetComponent<BoxCollider>();
		if (!_boxCollider)
			_boxCollider = gameObject.AddComponent<BoxCollider>();
	}

	public void SetPosition(Vector3 pos) => _position = pos;
}
