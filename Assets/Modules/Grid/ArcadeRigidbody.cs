using System;
using UnityEngine;

public class ArcadeRigidbody : CellComponent
{
	[SerializeField] private float _mass;
	[SerializeField] private bool _useGravity = false;

	private Rigidbody rigidbody;

	public Action<ArcadeCollider> CollisionEnter;
	public Action<ArcadeCollider> CollisionExit;

	public float Mass { get { return _mass; } set { _mass = value; } }
	public bool UseGravity { get { return _useGravity; } set { _useGravity = value; } }

	protected override void Init()
	{
		base.Init();

		rigidbody = GetComponent<Rigidbody>();
		if (!rigidbody)
			rigidbody = gameObject.AddComponent<Rigidbody>();

		rigidbody.useGravity = _useGravity;
	}

	public void Move(Vector3 velocity)
	{

	}
}
