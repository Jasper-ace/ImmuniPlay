using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterWalkToTarget : MonoBehaviour
{
    //====================================================
    // TARGET
    //====================================================

    [Header("Target")]
    [Tooltip("The RectTransform that the character will walk toward.")]
    public RectTransform target;


    //====================================================
    // MOVEMENT
    //====================================================

    [Header("Movement")]
    public float moveSpeed = 500f;

    public float stopDistance = 20f;


    //====================================================
    // LOOKING BACK
    //====================================================

    [Header("Looking Back")]
    [Tooltip("Animator trigger that plays the looking-back animation.")]
    public string lookingBackTrigger = "LookingBack";

    [Tooltip("How long the character stays after looking back.")]
    public float stayDuration = 2f;


    //====================================================
    // NEXT GAMEOBJECT
    //====================================================

    [Header("GameObjects After Reaching Target")]
    [Tooltip("GameObjects to show after the character reaches the target and finishes the wait.")]
    public GameObject[] showGameObjects;

    [Tooltip("GameObjects to hide after the character reaches the target and finishes the wait.")]
    public GameObject[] hideGameObjects;


    //====================================================
    // ANIMATOR
    //====================================================

    [Header("Animator")]
    public Animator animator;

    [Tooltip("Animator bool parameter used for walking.")]
    public string walkingParameter = "IsWalking";


    //====================================================
    // SWIPE
    //====================================================

    [Header("Swipe")]
    [Tooltip("Minimum horizontal distance required to start walking.")]
    public float dragThreshold = 50f;


    //====================================================
    // INTERNAL VARIABLES
    //====================================================

    private RectTransform character;

    private bool isWalking = false;
    private bool reachedTarget = false;

    private Vector2 dragStartPosition;


    //====================================================
    // IMPORTANT INPUT PROTECTION
    //====================================================

    // TRUE when we are waiting for the previous
    // dialogue tap to be released.
    private bool waitingForInputRelease = true;


    //====================================================
    // STATE
    //====================================================

    private enum MovementState
    {
        WaitingForSwipe,
        Walking,
        Finished
    }

    private MovementState currentState =
        MovementState.WaitingForSwipe;


    //====================================================
    // AWAKE
    //====================================================

    private void Awake()
    {
        character = GetComponent<RectTransform>();

        if (character == null)
        {
            Debug.LogError(
                "CharacterWalkToTarget: RectTransform is missing!",
                this
            );
        }
    }


    //====================================================
    // ON ENABLE
    //====================================================

    private void OnEnable()
    {
        ResetMovement();

        // IMPORTANT:
        // Ignore whatever touch/mouse press activated this object.
        waitingForInputRelease = true;

        Debug.Log(
            "CharacterWalkToTarget: Enabled. Waiting for player to release previous input.",
            this
        );
    }


    //====================================================
    // UPDATE
    //====================================================

    private void Update()
    {
        // -----------------------------------------------
        // Already finished
        // -----------------------------------------------

        if (currentState == MovementState.Finished)
            return;


        // -----------------------------------------------
        // Waiting for a NEW swipe
        // -----------------------------------------------

        if (currentState == MovementState.WaitingForSwipe)
        {
            HandleInput();
            return;
        }


        // -----------------------------------------------
        // Walking
        // -----------------------------------------------

        if (currentState == MovementState.Walking)
        {
            MoveToTarget();
        }
    }


    //====================================================
    // INPUT
    //====================================================

    private void HandleInput()
    {
        //================================================
        // MOBILE TOUCH
        //================================================

        if (Touchscreen.current != null)
        {
            var touch =
                Touchscreen.current.primaryTouch;


            //================================================
            // IMPORTANT:
            // WAIT FOR THE PREVIOUS TAP TO BE RELEASED
            //================================================

            if (waitingForInputRelease)
            {
                if (!touch.press.isPressed)
                {
                    waitingForInputRelease = false;

                    Debug.Log(
                        "CharacterWalkToTarget: Previous touch released. Ready for a NEW swipe.",
                        this
                    );
                }

                // DO NOT process this touch as a swipe.
                return;
            }


            //================================================
            // START NEW TOUCH
            //================================================

            if (touch.press.wasPressedThisFrame)
            {
                dragStartPosition =
                    touch.position.ReadValue();

                return;
            }


            //================================================
            // CHECK NEW SWIPE
            //================================================

            if (touch.press.isPressed)
            {
                Vector2 currentPosition =
                    touch.position.ReadValue();

                Vector2 drag =
                    currentPosition - dragStartPosition;


                // Only accept RIGHT swipe
                if (
                    drag.x >= dragThreshold &&
                    Mathf.Abs(drag.x) >
                    Mathf.Abs(drag.y)
                )
                {
                    StartWalking();
                }
            }
        }


        //================================================
        // UNITY EDITOR MOUSE
        //================================================

        if (Mouse.current != null)
        {
            //================================================
            // WAIT FOR PREVIOUS CLICK TO BE RELEASED
            //================================================

            if (waitingForInputRelease)
            {
                if (!Mouse.current.leftButton.isPressed)
                {
                    waitingForInputRelease = false;

                    Debug.Log(
                        "CharacterWalkToTarget: Previous mouse click released. Ready for a NEW drag.",
                        this
                    );
                }

                return;
            }


            //================================================
            // START NEW MOUSE DRAG
            //================================================

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                dragStartPosition =
                    Mouse.current.position.ReadValue();

                return;
            }


            //================================================
            // CHECK NEW MOUSE DRAG
            //================================================

            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 currentPosition =
                    Mouse.current.position.ReadValue();

                Vector2 drag =
                    currentPosition - dragStartPosition;


                // Only accept RIGHT drag
                if (
                    drag.x >= dragThreshold &&
                    Mathf.Abs(drag.x) >
                    Mathf.Abs(drag.y)
                )
                {
                    StartWalking();
                }
            }
        }
    }


    //====================================================
    // START WALKING
    //====================================================

    private void StartWalking()
    {
        // -----------------------------------------------
        // Safety check
        // -----------------------------------------------

        if (target == null)
        {
            Debug.LogError(
                "CharacterWalkToTarget: Target is NOT assigned!",
                this
            );

            return;
        }


        if (character == null)
        {
            Debug.LogError(
                "CharacterWalkToTarget: Character RectTransform is missing!",
                this
            );

            return;
        }


        // -----------------------------------------------
        // Only start from waiting state
        // -----------------------------------------------

        if (
            currentState !=
            MovementState.WaitingForSwipe
        )
        {
            return;
        }


        // -----------------------------------------------
        // Start walking
        // -----------------------------------------------

        currentState =
            MovementState.Walking;

        isWalking = true;


        Debug.Log(
            "CharacterWalkToTarget: NEW swipe detected. Character started walking!",
            this
        );


        // -----------------------------------------------
        // Walking animation
        // -----------------------------------------------

        if (animator != null)
        {
            animator.SetBool(
                walkingParameter,
                true
            );
        }
    }


    //====================================================
    // MOVE TO TARGET
    //====================================================

    private void MoveToTarget()
    {
        if (target == null)
            return;

        if (character == null)
            return;


        // -----------------------------------------------
        // Calculate distance
        // -----------------------------------------------

        float distance =
            Vector2.Distance(
                character.anchoredPosition,
                target.anchoredPosition
            );


        // -----------------------------------------------
        // Target reached
        // -----------------------------------------------

        if (distance <= stopDistance)
        {
            ReachTarget();
            return;
        }


        // -----------------------------------------------
        // Move character
        // -----------------------------------------------

        character.anchoredPosition =
            Vector2.MoveTowards(
                character.anchoredPosition,
                target.anchoredPosition,
                moveSpeed * Time.deltaTime
            );
    }


    //====================================================
    // REACH TARGET
    //====================================================

    private void ReachTarget()
    {
        if (
            currentState !=
            MovementState.Walking
        )
        {
            return;
        }


        // -----------------------------------------------
        // Change state
        // -----------------------------------------------

        currentState =
            MovementState.Finished;

        isWalking = false;
        reachedTarget = true;


        // -----------------------------------------------
        // Stop walking animation
        // -----------------------------------------------

        if (animator != null)
        {
            animator.SetBool(
                walkingParameter,
                false
            );
        }


        // -----------------------------------------------
        // Snap to target
        // -----------------------------------------------

        character.anchoredPosition =
            target.anchoredPosition;


        Debug.Log(
            "CharacterWalkToTarget: Character reached target!",
            this
        );


        // -----------------------------------------------
        // Start looking-back sequence
        // -----------------------------------------------

        StartCoroutine(
            TargetSequence()
        );
    }


    //====================================================
    // TARGET SEQUENCE
    //
    // REACHED TARGET
    //      ↓
    // LOOK BACK
    //      ↓
    // WAIT
    //      ↓
    // NEXT GAMEOBJECT
    //====================================================

    private IEnumerator TargetSequence()
    {
        //================================================
        // LOOK BACK
        //================================================

        if (
            animator != null &&
            !string.IsNullOrEmpty(
                lookingBackTrigger
            )
        )
        {
            animator.SetTrigger(
                lookingBackTrigger
            );

            Debug.Log(
                "CharacterWalkToTarget: Looking back.",
                this
            );
        }


        //================================================
        // WAIT
        //================================================

        yield return new WaitForSeconds(
            stayDuration
        );


        //================================================
        // SWITCH GAMEOBJECTS
        //================================================

        Debug.Log(
            "CharacterWalkToTarget: Updating GameObjects after reaching target.",
            this
        );

        // -----------------------------------------------
        // Hide assigned GameObjects
        // -----------------------------------------------

        if (hideGameObjects != null)
        {
            foreach (GameObject obj in hideGameObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        // -----------------------------------------------
        // Show assigned GameObjects
        // -----------------------------------------------

        if (showGameObjects != null)
        {
            foreach (GameObject obj in showGameObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        // -----------------------------------------------
        // Disable this GameObject last
        // -----------------------------------------------

        gameObject.SetActive(false);
    }


    //====================================================
    // RESET
    //====================================================

    public void ResetMovement()
    {
        // -----------------------------------------------
        // Stop all previous coroutines
        // -----------------------------------------------

        StopAllCoroutines();


        // -----------------------------------------------
        // Reset movement
        // -----------------------------------------------

        isWalking = false;
        reachedTarget = false;


        // -----------------------------------------------
        // IMPORTANT:
        // Start in waiting state
        // -----------------------------------------------

        currentState =
            MovementState.WaitingForSwipe;


        // -----------------------------------------------
        // Stop walking animation
        // -----------------------------------------------

        if (animator != null)
        {
            animator.SetBool(
                walkingParameter,
                false
            );


            if (
                !string.IsNullOrEmpty(
                    lookingBackTrigger
                )
            )
            {
                animator.ResetTrigger(
                    lookingBackTrigger
                );
            }
        }


        // -----------------------------------------------
        // Reset drag position
        // -----------------------------------------------

        dragStartPosition =
            Vector2.zero;
    }
}
