using UnityEngine;

public class ArcadeCollider : CellComponent
{
	[SerializeField] private bool _isTrigger = false;
	[SerializeField] private Vector3 _size;

	public bool IsTrigger { set { _isTrigger = value; } }
	public Vector3 Center { get { return _position; } set { _position = value; } }
	public Vector3 Size { get { return _size; } set { _size = value; } }

	protected override void Init()
	{
		base.Init();
	}

	public void SetSize() => transform.localScale = _size;
	public void SetTriggerCollider() => _boxCollider.isTrigger = _isTrigger;
}
