using KinematicCharacterController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

[RequireComponent(typeof(Animator))]
//[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(PlayerAudioManager))]
public class PlayerControls : BaseControllerScript, ICharacterController
{
    #region Variables
    public float MaxEnergy;
    public float Energy;

    [Header("Input Action References")]
    public InputActionReference MovementInputs;
    public InputActionReference JumpInputs;
    public InputActionReference SprintInputs;

    [Header("Other Object References")]
    public Transform Camera;
    [SerializeField]
    private KinematicCharacterMotor KinematicMotorReference;


    [Header("Player Movement Variables")]
    [SerializeField]
    /// <summary>
    /// Current player movement speed. Sprinting increases how high it can go.
    /// </summary>
    private float speed;
    [SerializeField]
    /// <summary>
    /// Controls the rate of increase in speed.
    /// </summary>
    private float accelaration = 10f;
    [SerializeField]
    /// <summary>
    /// The top speed of the player when not sprinting.
    /// </summary>
    private float maxSpeed;
    [SerializeField]
    /// <summary>
    /// The top speed of the player when sprinting.
    /// </summary>
    private float maxSprintSpeed;
    [SerializeField]
    /// <summary>
    /// Paramater used to control stability of velocity updates. Higher value = faster velocity increase over time.
    /// </summary>
    private float stableMovementSharpness = 15f;
    /// <summary>
    /// Float used to create control resistance in the air. Player controls will slow down in air when below 1, 
    /// and increase when above 1.
    /// </summary>
    public float airMoveSlowFactor = 0.8f;
    /// <summary>
    /// A base float value for the jump height of the player. Actual height gained is affected by jump and gravity strength.
    /// </summary>
    public float jumpHeight = 20f;
    /// <summary>
    /// Float value used to multiply strength of jump height. Creates significant height increase with minimal value change.
    /// </summary>
    private float jumpStrength = 3f;
    /// <summary>
    /// The strength gravity exerts on the player object. Instatiated at the standard downward 
    /// force when component is instantiated.
    /// </summary>
    public float gravityValue = -9.81f;
    /// <summary>
    /// Boolean flag to determine whether the player is currently in contact with the ground. 
    /// If true, player velocity in the Y axis should be as close to 0 as possible.
    /// </summary>
    public bool groundedPlayer;
    /// <summary>
    /// Internal bool to detect whether the player is currently jumping.
    /// </summary>
    private bool isJumping;

    public Vector3 playerVelocity;
    private CharacterController CharCont;
    //private PlayerAudioManager AudioMan;
    /// <summary>
    /// A constant float used to control the time frame between movements in LateUpdate. Used as a replacement for DeltaTime 
    /// to prevent jittering in inconsistent framerates.
    /// </summary>
    private float _TIME_BETWEEN_MOVE = 1/60f;
    private Vector3 hitNormal;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();

        // Initialise Character Controller based on what's connected.
        if (GetComponent<CharacterController>() != null)
        {
            CharCont = GetComponent<CharacterController>();
        }
        else if (GetComponent<KinematicCharacterMotor>() != null) 
        {
            KinematicMotorReference = GetComponent<KinematicCharacterMotor>();
            KinematicMotorReference.CharacterController = this;
        }
        //AudioMan = GetComponent<PlayerAudioManager>();
        if (maxSpeed <= 0) { maxSpeed = 15; }
        if (maxSprintSpeed <= 0) { maxSprintSpeed = maxSpeed * 1.1f; }
        if (accelaration <= 0) { accelaration = 10f; }
    }

    //private bool Attacking { get { return animator.GetCurrentAnimatorStateInfo(1).IsTag("AttackState"); } }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Alive)
        {
            // Standard Character Controller Movement Controls
            if (CharCont != null)
            {
                #region Player Movement Checks

                #region Player Aerial Checks
                /// Check player is on ground using the Char Controller and their last collision angle.
                /// If the Controller recognises they grounded after the last call of Move() and their last collision was
                /// with an angle below the slope limit, enable a jump.
                /// With the slope angle, it should cause them to at least slide along, if not down slopes above the limit.
                groundedPlayer = CharCont.isGrounded && (Vector3.Angle(Vector3.up, hitNormal) <= CharCont.slopeLimit);
                if (groundedPlayer && playerVelocity.y <= 0)
                {
                    animator.SetBool("jumped", false);
                    playerVelocity.y = 0f;
                }
                #endregion

                #region Movement Input
                // If the sprint input is detected, begin sprinting. Currently only works as sprint hold,
                // should implement sprint toggle in the future.
                if (SprintInputs.action.IsPressed())
                {
                    animator.SetBool("isSprinting", true);
                    speed = maxSpeed * 1.1f;
                }
                else
                {
                    animator.SetBool("isSprinting", false);
                    speed = maxSpeed;
                }


                // When player movement input is detected, stores the movement input into a temporary vector.
                // This vector is then aligned with the camera view, so player input alongside the camera moves intuitively.
                Vector3 move = (Camera.right * MovementInputs.action.ReadValue<Vector2>().x) +
                    (Vector3.ProjectOnPlane(Camera.forward, Vector3.up).normalized * MovementInputs.action.ReadValue<Vector2>().y);
                // The move vector is then multiplied by a factor of the 
                move = move * (groundedPlayer ? speed : speed * airMoveSlowFactor);
                playerVelocity = new Vector3(move.x, playerVelocity.y, move.z);
                if (move != Vector3.zero)
                {
                    animator.SetBool("isRunning", true);
                    gameObject.transform.forward = move;
                    //AudioMan.PlayWalkSounds();
                }
                else
                {
                    animator.SetBool("isRunning", false);
                    //AudioMan.StopWalkSounds();
                }
                #endregion

                #region Jump Input
                // Changes the height position of the player
                if (JumpInputs.action.IsPressed() && groundedPlayer)
                {
                    animator.SetBool("jumped", true);
                    playerVelocity.y += Mathf.Sqrt(jumpHeight * -jumpStrength * gravityValue);

                }
                #endregion

                #region Final Movement Calcs
                playerVelocity.y += gravityValue * _TIME_BETWEEN_MOVE;
                CharCont.Move(playerVelocity * _TIME_BETWEEN_MOVE);
                #endregion

                #endregion
            }
        }
        else
        {
            Die();
        }
    }

    #region Events
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death"))
        {
            Die();
        }
    }
    #endregion

    #region Character Controller Implements
    void ICharacterController.UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        // When we get to alternate gravities, maybe this will be used to rotate on the axis.
        Vector3 look = (Camera.right * MovementInputs.action.ReadValue<Vector2>().x) +
            (Vector3.ProjectOnPlane(Camera.forward, KinematicMotorReference.CharacterUp).normalized
            * MovementInputs.action.ReadValue<Vector2>().y);

        if (look.sqrMagnitude != 0f)
        {
            currentRotation = Quaternion.LookRotation(look, KinematicMotorReference.CharacterUp);
        }
    }

    void ICharacterController.UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        #region Previous Velocity Slope Adjustments
        // Using the current velocity's magnitude, and the angled vector of the ground we're on, we can handle slope velocity.
        float currVelMag = currentVelocity.magnitude;
        Vector3 effectiveGrndNormal = KinematicMotorReference.GroundingStatus.GroundNormal;

        // Maybe implement some slope detection adjustment here to handle slope velocity later.

        #endregion

        #region Current Velocity Movement Checks

        #region Player Aerial Checks
        /// Check player is on ground using the Char Controller and their last collision angle.
        /// If the Controller recognises they grounded after the last call of Move() and their last collision was
        /// with an angle below the slope limit, enable a jump.
        /// With the slope angle, it should cause them to at least slide along, if not down slopes above the limit.
        groundedPlayer = KinematicMotorReference.GroundingStatus.IsStableOnGround;
        if (groundedPlayer && currentVelocity.y <= 0)
        {
            //animator.SetBool("jumped", false);
            currentVelocity.y = 0f;
        }
        #endregion

        #region Sprint Input
        // If the sprint input is detected, begin sprinting. Currently only works as sprint hold,
        // should implement sprint toggle in the future.
        float currMaxSpd = maxSpeed;
        if (SprintInputs.action.IsPressed())
        {
            animator.SetBool("isSprinting", true);
            currMaxSpd = maxSpeed * 1.1f;
        }
        else
        {
            animator.SetBool("isSprinting", false);
        }
        #endregion

        #region Movement Input
        // When player movement input is detected, stores the movement input into a temporary vector.
        // This vector is then aligned with the camera view, so player input alongside the camera moves intuitively.
        Vector3 move = (Camera.right * MovementInputs.action.ReadValue<Vector2>().x) +
            (Vector3.ProjectOnPlane(Camera.forward, KinematicMotorReference.CharacterUp).normalized 
            * MovementInputs.action.ReadValue<Vector2>().y);
        // The move vector is then multiplied by the speed, which is increased
        float nextSpd = speed;
        if (speed < currMaxSpd) 
        {
            nextSpd = (groundedPlayer ? (speed + accelaration) : speed + (accelaration * airMoveSlowFactor));
        }
        else
        {
            speed = currMaxSpd;
        }
        move = move * nextSpd;
        Vector3 targetVelocity = new Vector3(move.x, currentVelocity.y, move.z);
        if (move != Vector3.zero)
        {
            animator.SetBool("isRunning", true);
            gameObject.transform.forward = move;
            //AudioMan.PlayWalkSounds();
        }
        else
        {
            animator.SetBool("isRunning", false);
            //AudioMan.StopWalkSounds();
        }
        #endregion

        #region Final Movement Calcs
        // Calculate effects of gravity
        if (!groundedPlayer)
        {
            targetVelocity.y += gravityValue * _TIME_BETWEEN_MOVE;
        }

        // Smoothen acceleration to target velocity
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 
            1f - Mathf.Exp(-stableMovementSharpness * _TIME_BETWEEN_MOVE));

        Vector3 planarVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        playerVelocity = currentVelocity;
        speed = planarVelocity.magnitude;
        #endregion

        #region Jump Input
        // Changes the height position of the player
        if (JumpInputs.action.IsPressed() && groundedPlayer && !isJumping)
        {
            currentVelocity += jumpHeight * KinematicMotorReference.CharacterUp;
            isJumping = true;
            KinematicMotorReference.ForceUnground();
            animator.SetBool("jumped", true);
        }
        #endregion

        #endregion
    }

    void ICharacterController.BeforeCharacterUpdate(float deltaTime)
    {
        
    }

    void ICharacterController.PostGroundingUpdate(float deltaTime)
    {
        
    }

    void ICharacterController.AfterCharacterUpdate(float deltaTime)
    {
        
    }

    bool ICharacterController.IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    void ICharacterController.OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        animator.SetBool("jumped", false);
        isJumping = false;
        //currentVelocity.y = 0f;
    }

    void ICharacterController.OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
        
    }

    void ICharacterController.OnDiscreteCollisionDetected(Collider hitCollider)
    {
        
    }
    #endregion
}
