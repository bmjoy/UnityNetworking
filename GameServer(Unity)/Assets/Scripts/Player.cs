using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    
    [Header("Movement Settings")]
    public CharacterController cc;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float sprintSpeed = 10;
    public float jumpHeight = 5f;
    
    private bool[] inputs;
    private float yVelocity = 0;

    void Start()
    {
        gravity *= Mathf.Pow(Time.fixedDeltaTime, 2);
        moveSpeed *= Time.fixedDeltaTime;
        sprintSpeed *= Time.fixedDeltaTime;
        jumpHeight *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;

        inputs = new bool[6];
    }
    
    #region Player Movement
    public void FixedUpdate()
    {
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }
    private void Move(Vector2 _inputDirection)
    {
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= inputs[5] ? sprintSpeed : moveSpeed;

        if (cc.isGrounded)
        {
            yVelocity = 0;
            if (inputs[4]) { yVelocity = jumpHeight; }
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        cc.Move(_moveDirection);
        
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
    #endregion
}
