using System;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class CarHandler : MonoBehaviour
{
  [SerializeField] Rigidbody rb;
	[SerializeField] Transform gameModel;

	[SerializeField] float accelerationMultiplier = 3f;
	[SerializeField] float decelerationMultiplier = 15f;
	[SerializeField] float steerMultiplier = 5f;

	[SerializeField] float maxSteerVelocity = 2f;
	[SerializeField] float maxForwardVelocity = 30f;

	//Input
	Vector2 input = Vector2.zero;

	void Update()
	{
		//rotate car model when turning
		gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 5, 0);
	}

  void FixedUpdate()
  {
		if (input.y > 0.01f)
			Accelerate();
		else
			rb.linearDamping = 0.2f; //if the player is not pressing the button the car will slow down a little bit

		if(input.y < -0.01f)
			Break();

		Steer();

		if(rb.linearVelocity.z <= 0)
			rb.linearVelocity = Vector3.zero;
  }

	private void Accelerate()
	{
		rb.linearDamping = 0f;

		if(rb.linearVelocity.z >= maxForwardVelocity) return;

		rb.AddForce(accelerationMultiplier * input.y * rb.transform.forward, ForceMode.Acceleration);
	}

	private void Break()
	{
		//unless we are going forward
		if (rb.linearVelocity.z <= 0) return;

		rb.AddForce(decelerationMultiplier * input.y * rb.transform.forward, ForceMode.Acceleration);
	}

	private void Steer()
	{
		if (Math.Abs(input.x) > 0.01f)
		{
			float speedBaseSteerLimit = rb.linearVelocity.z / 5f;
			speedBaseSteerLimit = Mathf.Clamp01(speedBaseSteerLimit);

			rb.AddForce(steerMultiplier * speedBaseSteerLimit * input.x * rb.transform.right, ForceMode.Acceleration);

			float normalizedX = rb.linearVelocity.x / maxSteerVelocity;
			normalizedX = Mathf.Clamp(normalizedX, -1.0f, 1.0f);

			rb.linearVelocity = new Vector3(normalizedX * maxSteerVelocity, 0, rb.linearVelocity.z);
		}
		else
		{
			//auto center car
			rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, 0, rb.linearVelocity.z), Time.fixedDeltaTime * 3);
		}
	}

	public void SetInput(Vector2 inputVector)
	{
		input = Vector2.ClampMagnitude(inputVector, 1f);
	}
}
