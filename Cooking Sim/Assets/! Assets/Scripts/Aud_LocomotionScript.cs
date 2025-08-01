using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class FootstepAudioVR : MonoBehaviour
{
    public AudioSource footstepAudio;
    public InputActionProperty moveInput; // Reference to the left or right joystick input
    public float threshold = 0.1f; // How much input is "enough" to count as moving

    void Update()
    {
        Vector2 input = moveInput.action.ReadValue<Vector2>();

        bool isMoving = input.magnitude > threshold;

        if (isMoving)
        {
            if (!footstepAudio.isPlaying)
                footstepAudio.Play();
        }
        else
        {
            if (footstepAudio.isPlaying)
                footstepAudio.Stop();
        }
    }
}
