using UnityEngine;
using System.Collections;
using GameInputActions;

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

    // Simulation
    private bool isTest = false;

    // Movement/rotation variables
    private float nextMoveTime = 0.0f;
    private float nextRotateTime = 0.0f;
    private float rotationValue = 0.0f;
    private bool isMoving = false;
    private bool keepMoving = false;
    private bool isRotating = false;
    private bool keepRotating = false;
    private bool movementEnabled = false;
    private GameObject playerObject = null;
    public GameObject PlayerObject
    {
        get { return playerObject; }
        set { playerObject = value; }
    }

    // Run once when first initialized, similar to a constructor
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        // Subscribe to started events, started is called on initial keypress
        playerInputActions.Player.Move.started += ctx => OnMoveStart();
        playerInputActions.Player.Rotate.started += ctx => OnRotateStart();

        // Subscribe to canceled events, canceled is called when key is released
        playerInputActions.Player.Move.canceled += ctx => OnMoveRelease();
        playerInputActions.Player.Rotate.canceled += ctx => OnRotateRelease();
    }

    void Update()
    {
        // Check valid playerObject, enabled movement and player movement not already in progress
        if (playerObject != null && movementEnabled && !isMoving && !isRotating)
        {
            // Prioritize movement over rotation, checking timers
            if (keepMoving && Time.time >= nextMoveTime)
            {
                TryMovePlayer();
            }
            else if (keepRotating && Time.time >= nextRotateTime)
            {
                TryRotatePlayer();
            }
        }
    }

    #region Event Methods
    /// <summary>
    /// subscribed to .started event for "Move" action
    /// </summary>
    public void OnMoveStart()
    {
        keepMoving = true;
    }

    /// <summary>
    /// subscribed to .canceled event for "Move" action
    /// </summary>
    public void OnMoveRelease()
    {
        keepMoving = false;
    }

    /// <summary>
    /// subscribed to .started event for "Rotate"
    /// </summary>
    public void OnRotateStart()
    {
        keepRotating = true;
    }

    /// <summary>
    /// subscribed to .canceled event for "Rotate" action
    /// </summary>
    public void OnRotateRelease()
    {
        keepRotating = false;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Disables player movement
    /// </summary>
    public void DisableMovement()
    {
        movementEnabled = false;
    }

    /// <summary>
    /// Enables player movement
    /// </summary>
    public void EnableMovement()
    {
        movementEnabled = true;
    }

    public float GetMoveDistance()
    {
        return moveDistance;
    }

    public float GetMoveDelay()
    {
        return moveDelay;
    }

    public float GetRotateDelay()
    {
        return rotateDelay;
    }

    public void EnableSimulation()
    {
        isTest = true;
    }

    public void SetRotationValue(float value)
    {
        rotationValue = value;
    }
    #endregion

    private void TryMovePlayer()
    {
        // Update timer
        nextMoveTime = Time.time + moveDelay + 0.1f; // Extra .1 second delay for coroutine to finish

        // Get the current forward direction
        Vector3 forwardDirection = playerObject.transform.forward.normalized;

        // Multiple raycasts for 3D collision detection
        if (CanMove(forwardDirection, playerObject))
        {
            // Begin movement
            isMoving = true;

            // Start movement coroutine
            StartCoroutine(SmoothMove(forwardDirection * moveDistance));
        }
    }

    private void TryRotatePlayer()
    {
        // Get rotation input
        if (!isTest)
            rotationValue = playerInputActions.Player.Rotate.ReadValue<float>();

        // Update timer
        nextRotateTime = Time.time + rotateDelay + 0.1f; // Extra .1 second delay for coroutine to finish

        if (Mathf.Abs(rotationValue) > 0f) // Check for any rotation value
        {
            float rotationAmount = rotationValue > 0 ? -90f : 90f;

            // Begin rotation
            isRotating = true;

            // Start rotation coroutine
            StartCoroutine(SmoothRotate(rotationAmount));
        }
    }

    // Coroutine for smooth movement
    IEnumerator SmoothMove(Vector3 targetOffset)
    {
        Vector3 startingPosition = playerObject.transform.position;
        Vector3 targetPosition = startingPosition + targetOffset;
        float startTime = Time.time;

        while (playerObject.transform.position != targetPosition)
        {
            isMoving = true;
            float timeFraction = (Time.time - startTime) / moveDelay; // Use moveDelay for duration 
            playerObject.transform.position = Vector3.Lerp(startingPosition, targetPosition, timeFraction);
            yield return null;
        }

        // Ensure the object ends exactly at the final position
        playerObject.transform.position = targetPosition;

        // Alert movement stop
        isMoving = false;
    }

    // Coroutine for smooth rotations
    IEnumerator SmoothRotate(float rotationAmount) // Expects rotation in degrees
    {
        Quaternion startingRotation = playerObject.transform.rotation;
        Quaternion targetRotation = startingRotation * Quaternion.Euler(0, rotationAmount, 0);
        float startTime = Time.time;

        while (playerObject.transform.rotation != targetRotation)
        {
            float timeFraction = (Time.time - startTime) / rotateDelay; // Use moveDelay for duration 
            playerObject.transform.rotation = Quaternion.Slerp(startingRotation, targetRotation, timeFraction);
            yield return null;
        }

        // Ensure the object ends exactly at the final rotation
        playerObject.transform.rotation = targetRotation;

        // Alert rotation stop
        isRotating = false;
    }

    private bool CanMove(Vector3 direction, GameObject source)
    {
        // Adjust number of raycasts and offset positions
        Vector3 offset = new Vector3(0, 0.5f, 0); // Offset for slightly elevated raycasts 

        /*
         Raycasting is a lot like sending out a laser and checking if it hits something within a certain range, in this case moveDistance. 
         The offset helps with consistency, but basically just moves the source of the laser up a bit.
         It will only detect walls.
         - Jesper
         */
        return !Physics.Raycast(source.transform.position + offset, direction, out RaycastHit hit, moveDistance, wallLayer);
    }
}