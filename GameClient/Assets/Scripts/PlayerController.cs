using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement options")]
    public bool serverAuthoritativeMovement = true;
    public bool allowJumping = true;
    public bool allowSprinting = true;
    
    [Space]

    [Header("Keybinds")] 
    public KeyCode forward = KeyCode.W;
    public KeyCode backward = KeyCode.S;
    public KeyCode right = KeyCode.D;
    public KeyCode left = KeyCode.A;
    public KeyCode jump = KeyCode.Space;
    public KeyCode sprint = KeyCode.LeftShift;
    
    
    #region Sending Input
    private void FixedUpdate()
    {
        SendInputToServer();

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SendInputToServer()
    {
        if (!serverAuthoritativeMovement) return;
        
        bool isJumping = Input.GetKey(jump) && allowJumping;
        bool isSprinting = Input.GetKey(sprint) && allowSprinting;
        
        bool[] _inputs = new bool[]
        {
            Input.GetKey(forward),
            Input.GetKey(backward),
            Input.GetKey(left),
            Input.GetKey(right),
            isJumping,
            isSprinting,
        };

        if (Client.Instance.playerMovement) ClientSend.PlayerMovement(_inputs);
    }
    #endregion
}
