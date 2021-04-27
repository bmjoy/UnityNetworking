using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTransform
{

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public NetworkTransform(Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        position = _position;
        rotation = _rotation;
        scale = _scale;
    }
}
