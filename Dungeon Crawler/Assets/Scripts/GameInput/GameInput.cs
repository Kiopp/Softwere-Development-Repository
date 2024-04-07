using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    /*
        GameInput handles player inputs using the new unity input actions
        
        Code written by Jesper Wentzell
    */
    [SerializeField] private float moveDistance = 6.0f;
    [SerializeField] private float moveDelay = 0.5f;  // Delay between movements
    [SerializeField] private float rotateDelay = 0.5f; // Delay between rotations
    [SerializeField] private LayerMask wallLayer;
    private PlayerInputActions playerInputActions;

    // Movement/rotation
    private float nextMoveTime = 0.0f;
    private float nextRotateTime = 0.0f;
    private bool isMoving = false;
    private bool isRotating = false;
    private GameObject playerObject = null;
    public GameObject PlayerObject
    {
        get { return playerObject; }
        set { playerObject = value; }
    }

    // Run once when first initialized, similar to a constructor
    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // Subscribe to started events, started is called on initial keypress
        playerInputActions.Player.Move.started += ctx => OnMoveStart();
        playerInputActions.Player.Rotate.started += ctx => OnRotateStart();

        // Sybscribe to canceled events, canceled is called when key is released
        playerInputActions.Player.Move.canceled += ctx => OnMoveRelease();
        playerInputActions.Player.Rotate.canceled += ctx => OnRotateRelease();
    }

    #region EventMethods
    public void OnMoveStart()
    {
        isMoving = true;
        MovePlayer();
    }
    public void OnMoveRelease()
    {
        isMoving = false;
    }

    public void OnRotateStart()
    {
        isRotating = true;
        RotatePlayer();
    }
    public void OnRotateRelease()
    {
        isRotating = false;
    }
    #endregion

    #region PublicGetMethods
    public float GetNextMoveTime()
    {
        return nextMoveTime;
    }

    public float GetNextRotateTime()
    {
        return nextRotateTime;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    public bool IsRotating()
    {
        return isRotating;
    }
    #endregion

    public void MovePlayer()
    {
        // Get player movement input
        bool moving = playerInputActions.Player.Move.IsPressed();

        // Update timer
        nextMoveTime = Time.time + moveDelay;

        if (moving)
        {
            // Get the current forward direction
            Vector3 forwardDirection = playerObject.transform.forward.normalized;

            // Multiple raycasts for 3D collision detection
            if (CanMove(forwardDirection))
            {
                //Debug.Log("Moved");
                playerObject.transform.position += forwardDirection * moveDistance; // Move
            }
        }
        else
        {
            //Debug.Log("Did not move");
        }
    }

    public void RotatePlayer()
    {
        // Get player rotation input
        float rotationValue = playerInputActions.Player.Rotate.ReadValue<float>();

        // Update timer
        nextRotateTime = Time.time + rotateDelay;

        if (Mathf.Abs(rotationValue) > 0f) // Check for any rotation value
        {
            //Debug.Log("Rotated");
            if (rotationValue > 0) // Clockwise rotation
            {
                // Rotate the object 90 degrees on the Y-axis
                playerObject.transform.Rotate(0f, -90f, 0f);
            }
            else if (rotationValue < 0) // Counter clockwise rotation
            {
                // Rotate the object 90 degrees on the Y-axis
                playerObject.transform.Rotate(0f, 90f, 0f);
            }
        }
        else
        {
            //Debug.Log("Did not rotate");
        }
    }

    private bool CanMove(Vector3 direction)
    {
        // Adjust number of raycasts and offset positions
        Vector3 offset = new Vector3(0, 0.5f, 0); // Offset for slightly elevated raycasts 

        /*
         Raycasting is a lot like sending out a laser and checking if it hits something within a certain range, in this case moveDistance. 
         The offset helps with consistency and allows for player to have a box collider.
         It will only detect walls.
         - Jesper
         */
        return !Physics.Raycast(playerObject.transform.position + offset, direction, out RaycastHit hit, moveDistance, wallLayer);
    }
}
