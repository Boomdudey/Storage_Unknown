using UnityEngine;

public class Cabinet : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] public Transform RightDoorhinge;
    [SerializeField] public Transform LeftDoorhinge;
    public Transform hidingPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDoor()
    {
        RightDoorhinge.Rotate(0, 90, 0);
        LeftDoorhinge.Rotate(0, -90, 0);
    }

    public void CloseDoor()
    {
        RightDoorhinge.Rotate(0, -90, 0);
        LeftDoorhinge.Rotate(0, 90, 0);
    }
}
