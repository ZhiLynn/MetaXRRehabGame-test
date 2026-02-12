using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrabPoleKeyboardTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string grabPoleTag = "Player";
    public Color highlightColor = Color.yellow;

    private TMP_InputField inputField;
    private Image backgroundImage;
    private Color originalColor;

    private bool isColliding = false;
    private TouchScreenKeyboard overlayKeyboard;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        if (inputField == null)
            Debug.LogWarning("GrabPoleKeyboardTrigger: TMP_InputField missing!");

        backgroundImage = GetComponentInChildren<Image>();
        if (backgroundImage != null)
            originalColor = backgroundImage.color;

        Debug.Log("GrabPoleKeyboardTrigger initialized.");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(grabPoleTag))
        {
            isColliding = true;
            if (backgroundImage != null)
                backgroundImage.color = highlightColor;

            Debug.Log("GrabPole collided with input field. Highlight ON.");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(grabPoleTag))
        {
            isColliding = false;
            if (backgroundImage != null)
                backgroundImage.color = originalColor;

            Debug.Log("GrabPole exited collision. Highlight OFF.");
        }
    }

    /// <summary>
    /// Call this from your panel/manager when trigger is pressed
    /// </summary>
    public void OnTriggerPressed()
    {
        if (!isColliding)
        {
            Debug.Log("Trigger pressed, but not colliding. Keyboard will NOT open.");
            return;
        }

        Debug.Log("Trigger pressed AND colliding. Opening keyboard!");

        if (inputField == null) return;

        if (TouchScreenKeyboard.isSupported)
        {
            overlayKeyboard = TouchScreenKeyboard.Open(
                inputField.text,
                TouchScreenKeyboardType.Default,
                false, false, false, false,
                "Enter text..."
            );
        }
        else
        {
            Debug.Log("TouchScreenKeyboard not supported on this platform.");
        }
    }

    void Update()
    {
        if (overlayKeyboard != null)
        {
            try
            {
                if (overlayKeyboard.active)
                {
                    inputField.text = overlayKeyboard.text;
                    Debug.Log($"Keyboard active. Current text: {inputField.text}");
                }
            }
            catch
            {
                overlayKeyboard = null; // prevent null reference
                Debug.Log("Keyboard reference lost or not supported.");
            }
        }
    }
}
