using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Mouse Controller class handles setting updates to the mouse object's position.
/// </summary>
/// <remarks>
/// Sets "elevation" angle to be relative to the horizon and swaps convention for
/// y and z position from unity's to make setZ set the altitude dimension and setY
/// set the distance along the track.
/// </remarks>
public class MouseController : MonoBehaviour
{
    /// <remarks>
    /// Depreciated. This field is currently unused, but would be necessary
    /// for the determineY.
    /// </remarks>
	public float yOffset = 1.5f;

    /// <remarks>
    /// will store info of successful ray cast to ensure that the mouse is
    /// position at fixed distance above track. Currently this method isn't
    /// used and the z-position is fixed to that if the mouse model in the
    /// Unity UI
    /// </remarks>
    /// <returns>
    /// The altitude of the path/mesh collider located beneath the mouse
    /// with the y-offset added to it
    /// </returns>
    private float DetermineY()
    {
		RaycastHit hitInfo;
		//terrain should have mesh collider and be on custom terrain
		//layer so we don't hit other objects with our raycast

		//cast ray
		if (Physics.Raycast (transform.position, Vector3.down, out hitInfo))
        {

			//get where on the z axis our raycast hit the ground
            Debug.Log(hitInfo.point.y);
			return hitInfo.point.y + yOffset;
		}
        else if (Physics.Raycast(transform.position,
                                 Vector3.up, out hitInfo))
        {

            Debug.Log(hitInfo.point.y);
			return hitInfo.point.y + yOffset;
		}

        return yOffset;
    }

    /// <summary>
    /// Sets the x location of the mouse.
    /// </summary>
    /// <remarks>
    /// The x axis the short axis of the environment. i.e. for a linear track
    /// this is generally fixed to 0
    /// </remarks>
    /// <param name="x">A float defining the new x position of the mouse.</param>
    public void SetX(float x)
    {
        Vector3 update = transform.position;
        update.x = x;
        transform.position = update;
    }

    /// <summary>
    /// Sets the y location of the mouse.
    /// </summary>
    /// <remarks>
    /// The y axis the long axis of the environment. i.e. for a linear track
    /// this is analogous to the "treadmill position". In Unity dimensions this
    /// is the z-axis.
    /// </remarks>
    /// <param name="y">A float defining the new y position of the mouse.</param>
    public void SetY(float y)
    {
        Vector3 update = transform.position;
        update.z = y;
        transform.position = update;
    }

    /// <summary>
    /// Sets the z location of the mouse.
    /// </summary>
    /// <remarks>
    /// The z axis represents the altitude of the mouse. In Unity dimensions
    /// this is the y-axis.
    /// </remarks>
    /// <param name="z">A float defining the new z position of the mouse.</param>
    public void SetZ(float z)
    {
        Vector3 update = transform.position;
        update.y = z;
        transform.position = update;
    }

    /// <summary>
    /// Sets the rotation of the mouse.
    /// </summary>
    /// <remarks>
    /// Sets the rotation of the mouse object/main view camera. Rotations are
    /// clockwise as viewed from overhead of the mouse 
    /// </remarks>
    /// <param name="new_rotation">A float defining the new rotation of the mouse
    /// (in degrees).</param>
    public void SetRotation(float new_rotation)
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = new_rotation;
        transform.rotation = Quaternion.Euler(rotation);
    }


    /// <summary>
    /// Sets the rotation of the main camera.
    /// </summary>
    /// <remarks>
    /// Sets the rotation of the main view camera relative to the mouse. Rotations are
    /// clockwise as viewed from overhead of the mouse 
    /// </remarks>
    /// <param name="new_rotation">A float defining the new rotation of the main camera
    /// (in degrees).</param>
    public void SetViewRotation(float new_rotation)
    {
        Transform camera_transform = transform.Find("Main Camera");
        Vector3 rotation = camera_transform.rotation.eulerAngles;
        rotation.y = new_rotation;
        camera_transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Sets the elevation angle of the mouse object.
    /// </summary>
    /// <remarks>
    /// Sets the rotation of the mouse object/main view camera. Rotations are
    /// relative to the horizon.
    /// </remarks>
    /// <param name="elevation">A float defining the elevation angle of the
    /// mouse object.</param>
    public void SetElevation(float elevation)
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.x = 90.0f - elevation;
        transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Sets the elevation angle of the main camera object.
    /// </summary>
    /// <remarks>
    /// Sets the rotation of the main view camera. Rotations are
    /// relative to the horizon.
    /// </remarks>
    /// <param name="elevation">A float defining the elevation angle of the
    /// main camera.</param>
    public void SetViewElevation(float elevation)
    {
        Transform camera_transform = transform.Find("Main Camera");
        Vector3 rotation = camera_transform.rotation.eulerAngles;
        rotation.x = 90.0f - elevation;
        camera_transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Sets the screen orientation for the mouse object.
    /// </summary>
    /// <remarks>
    /// Sets the screen orientation angle for the mouse camera object.
    /// </remarks>
    /// <param name="orientation">A float defining the orientation angle of the
    /// mouse object..</param>
    public void SetViewOrientation(float orientation)
    {
        Transform camera_transform = transform.Find("Main Camera");
        Vector3 rotation = camera_transform.rotation.eulerAngles;
        rotation.z = orientation;
        camera_transform.rotation = Quaternion.Euler(rotation);
    }
}
