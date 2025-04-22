using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSoundHandler : MonoBehaviour
{
    #region
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip buttonHoverSound;

    [SerializeField] private Button button;
    #endregion

    void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(audioSource != null && buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound);
        }
    }

    public void OnButtonClick()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

}
