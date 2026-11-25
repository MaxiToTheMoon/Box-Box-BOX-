using System;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.XR;
using System.Collections;


public class CarHandler : MonoBehaviour
{
  [SerializeField] Rigidbody rb;
	[SerializeField] Transform gameModel;

	[SerializeField] ExplodeHandler explodeHandler;
    bool isExploded = false;
    float carStartPositionX;
    float carStartPositionY;

    [SerializeField] float accelerationMultiplier = 3f;
	[SerializeField] float decelerationMultiplier = 15f;
	[SerializeField] float steerMultiplier = 5f;
	[SerializeField] float maxSteerVelocity = 2f;

	[Header("Impostazioni Marce")]
	public int currentGear = 0;
  public int maxGear = 5;
	[SerializeField] float[] maxSpeedPerGear = new float[] { 10f, 20f, 30f, 40f, 50f };
	[SerializeField] float idleStartBoost = 10f;

	//Input
	Vector2 input = Vector2.zero;

	//Stats
	float carStartPositionZ;
	float distanceTravelled = 0;
	public float DistanceTravelled => distanceTravelled;
	public float CurrentSpeed => rb.linearVelocity.z;


    //Respawn
    [SerializeField] float respawnDelay = 5f;

    void Start()
  {
    carStartPositionZ = transform.position.z;
	carStartPositionX = transform.position.x;
	carStartPositionY = transform.position.y;
  }

  void Update()
	{
		if (isExploded)
			return;
		//rotate car model when turning
		gameModel.transform.rotation = Quaternion.Euler(0, rb.linearVelocity.x * 5, 0);

		//Update distance travelled
		distanceTravelled = transform.position.z - carStartPositionZ;
	}

  void FixedUpdate()
  {
	if (isExploded)
	{
			//rb.linearDamping = 0;
			rb.linearDamping = rb.linearVelocity.z * 0.02f;
			//rb.linearVelocity = new Vector3(0, 0, 0);
			rb.linearDamping = Mathf.Clamp(rb.linearDamping, 1.5f, 10);

			rb.MovePosition(Vector3.Lerp(transform.position, new Vector3(carStartPositionX, carStartPositionY, transform.position.z), Time.deltaTime * 0.9f));
			return;
	}

		// Frenata
    if (input.y < -0.01f)
      Break();
    else if (currentGear > 0)
      Accelerate();
    else
      rb.linearDamping = 0.5f;

		Steer();

		AutoDownshift();

		if (rb.linearVelocity.z <= 0)
      rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0);
  }

	private void Accelerate()
	{
		rb.linearDamping = 0f;
		float currentMaxVelocity = maxSpeedPerGear[Mathf.Clamp(currentGear - 1, 0, maxSpeedPerGear.Length - 1)];

		if(rb.linearVelocity.z >= currentMaxVelocity) return;

		float currentAcceleration = accelerationMultiplier;

		if (currentGear == 1 && rb.linearVelocity.z < 2f)
    	currentAcceleration += idleStartBoost;

		rb.AddForce(currentAcceleration * 1f * rb.transform.forward, ForceMode.Acceleration);
	}

	private void Break()
	{
		if (rb.linearVelocity.z <= 0) return;

		rb.linearDamping = 0.2f;
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

	public void ShiftUp()
  {
    if (currentGear < maxGear)
      currentGear++;
  }

	private void AutoDownshift()
  {
    if (currentGear <= 0) return;

    if (currentGear > 1)
    {
      float previousGearMaxSpeed = maxSpeedPerGear[currentGear - 2];
            
      if (rb.linearVelocity.z < previousGearMaxSpeed - 1f)
        currentGear--;
    }
    else if (currentGear == 1 && rb.linearVelocity.z < 1f && input.y < -0.01f)
    	currentGear = 0;
	}


	float EXPLOSION_SPEED_THRESHOLD = 15f;
    private void OnCollisionEnter(Collision collision)
    {
		if (isExploded)
			return;

        Vector3 spawnPosition = new Vector3(
			carStartPositionX,
			carStartPositionY,
			transform.position.z
		);

        Vector3 velocity = rb.linearVelocity;
        float impactSpeed = rb.linearVelocity.magnitude;
        explodeHandler.Explode(velocity, impactSpeed >= EXPLOSION_SPEED_THRESHOLD);
        isExploded = true;

        StartCoroutine(RespawnCar(spawnPosition));

    }

    private IEnumerator RespawnCar(Vector3 spawnPosition)
    {
        yield return new WaitForSecondsRealtime(respawnDelay);

        explodeHandler.ResetParts(spawnPosition);

        rb.position = spawnPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = false;

        currentGear = 0;
        isExploded = false;
    }
}
