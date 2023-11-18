using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotate : MonoBehaviour
{
	public Vector3 AngularVelocity { set { angularVelocity = value; } get { return angularVelocity; } }
	[SerializeField] private Vector3 angularVelocity = Vector3.up;

	/// <summary>The rotation space.</summary>
	public Space RelativeTo { set { relativeTo = value; } get { return relativeTo; } }
	[SerializeField] private Space relativeTo;

	protected virtual void Update()
	{
		transform.Rotate(angularVelocity * Time.deltaTime, relativeTo);
	}
}
