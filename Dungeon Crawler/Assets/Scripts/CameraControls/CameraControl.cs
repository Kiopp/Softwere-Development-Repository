using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;

    // Update is called once per frame
    void Update()
    {

    }

    public void DisableMovement()
    {
        gameInput.DisableMovement();
    }

    public void EnableMovement()
    {
        gameInput.EnableMovement();
    }

    void Awake()
    {
        gameInput.EnableMovement();
        gameInput.PlayerObject = this.gameObject; // Set camera reference inside input class
    }


}
