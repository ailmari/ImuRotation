# ImuRotation
Device tracking script for Unity AR projects. Uses IMU (gyroscope, accelerometer and magnetometer) to determine the pose of the device.

Navigator.cs is an abstraction for Unity's Location Service, which is required to read magnetometer values. It's a singleton, so Singleton.cs is also included here.
