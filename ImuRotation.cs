using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImuRotation : MonoBehaviour
{
	// Service to query compass heading from
	Navigator navigator;

	// Samples from imu sensors
	Vector3 gyroSample;
	Vector3 aclrSample;
	float compassSample;

	// For setting orientation
	Quaternion oldRotation;
	Quaternion newRotation;
	float yaw;
	float pitch;
	float roll;

	// For compass reliability
	bool compassReliable = true;
	int reliableCompassSamplesCounter = 0;

	// Low pass filter alpha value for smoothing
	const float smooth = 0.5f;
	// Accelerometer low pass filter values
	float fX_aclr;
	float fY_aclr;
	float fZ_aclr;
	// Compass low pass filter values
	float lastSin;
	float lastCos;

	void Start()
	{
		// Get instance of Navigator
		navigator = Navigator.Instance;

		// Gyroscope is disabled on Android by default
		Input.gyro.enabled = true;
	}

#if !UNITY_EDITOR
	// FixedUpdate guaranteed to run once every frame
	void FixedUpdate()
	{
		// Save previous rotation
		oldRotation = transform.rotation;

		// Sample gyro and accelerometer
		gyroSample = Input.gyro.rotationRateUnbiased;
		aclrSample = Input.acceleration;
		compassSample = navigator.heading * Mathf.PI / 180.0f;

		// Lowpass filter compass
		lastSin = smooth * lastSin + (1.0f - smooth) * Mathf.Sin(compassSample);
		lastCos = smooth * lastCos + (1.0f - smooth) * Mathf.Cos(compassSample);
		// Calculate yaw by compass
		yaw = Mathf.Atan2(lastSin, lastCos) * 57.3f;

		// Low pass filter accelerometer sample
		fX_aclr = aclrSample.x * smooth + (fX_aclr * (1.0f - smooth));
		fY_aclr = aclrSample.y * smooth + (fY_aclr * (1.0f - smooth));
		fZ_aclr = aclrSample.z * smooth + (fZ_aclr * (1.0f - smooth));
		// Calculate pitch and roll by accelerometer
		pitch = Mathf.Atan2((fZ_aclr), Mathf.Sqrt(Mathf.Pow(fX_aclr, 2) + Mathf.Pow(-fY_aclr, 2))) * -57.3f;
		roll = Mathf.Atan2(fX_aclr, -fY_aclr) * -57.3f;

		// If compass is reliable (device pointing down)
		compassReliable = (pitch >= 15.0f && pitch <= 75.0f);
		if(compassReliable)
		{
			reliableCompassSamplesCounter++;

			// If reliable for long enough
			if(reliableCompassSamplesCounter >= 120)
			{
				// Rotate according to accelerometer and compass
				newRotation = Quaternion.Euler(pitch, yaw, roll);
				transform.rotation = Quaternion.Slerp(oldRotation, newRotation, 0.05f);
			}
		}
		else
		{
			// Unreliable sample, start counting again
			reliableCompassSamplesCounter = 0;

			// Rotate according to accelerometer
			newRotation = Quaternion.Euler(pitch, oldRotation.eulerAngles.y, roll);
			transform.rotation = Quaternion.Slerp(oldRotation, newRotation, 0.1f);
		}

		// Rotate according to gyroscope
		transform.Rotate(-gyroSample.x, -gyroSample.y, gyroSample.z);
	}
#endif
}
