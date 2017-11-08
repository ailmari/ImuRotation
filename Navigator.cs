using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigator : Singleton<Navigator>
{
	// GNSS coordinates and compass heading
	public float latitude;
	public float longitude;
	public float heading;

	[SerializeField]
	private float desiredAccuracy = 5.0f;
	[SerializeField]
	private float updateDistance = 5.0f;

	public NavigatorStatus status = NavigatorStatus.INITIALIZING;
	public enum NavigatorStatus
	{
		READY,
		INITIALIZING,
		STOPPED,
	}

	void Start()
	{
		// Start service
		#if UNITY_ANDROID
			StartCoroutine(StartNavigator(desiredAccuracy, updateDistance));
		#endif

		// Mock location on editor
		#if UNITY_EDITOR
			latitude = 65.019665f; // Kasarmintie
			longitude = 25.491095f;
			//latitude = 65.058162f; // Tietotalo
			//longitude = 25.469159f;
			//latitude = 65.011707f; // Rotuaari
			//longitude = 25.470510f;
			heading = 90.0f;
			status = NavigatorStatus.READY;
		#endif
	}

	void Update()
	{
		#if !UNITY_EDITOR
			if(status == NavigatorStatus.READY) {
				latitude = Input.location.lastData.latitude;
				longitude = Input.location.lastData.longitude;
				heading = Input.compass.trueHeading;
			}
		#endif
	}

	IEnumerator StartNavigator(float desiredAccuracy, float updateDistance)
	{
		// Check if location service enabled
		if(!Input.location.isEnabledByUser)
		{
			status = NavigatorStatus.STOPPED;
			yield break;
		}

		// Start service
		Input.location.Start(desiredAccuracy, updateDistance);

		// Enable Compass
		Input.compass.enabled = true;

		// Wait until service initialized
		int maxWait = 5;
		while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
		{
			status = NavigatorStatus.INITIALIZING;
			yield return new WaitForSeconds(1);
			maxWait--;
		}

		// Service didn't initialize
		if(maxWait < 1)
		{
			status = NavigatorStatus.STOPPED;
			yield break;
		}

		// Connection has failed..
		if(Input.location.status == LocationServiceStatus.Failed)
		{
			status = NavigatorStatus.STOPPED;
			yield break;
		}
		// ..or connection established
		else
		{
			latitude = Input.location.lastData.latitude;
			longitude = Input.location.lastData.longitude;
			heading = Input.compass.trueHeading;
			status = NavigatorStatus.READY;
		}
	}
}
