using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform doorMesh; // The actual door geometry
    public Transform hiddenPosition; // Where the door slides up to
    public float slideSpeed = 2f;

    private bool isOpen = false;
    private Vector3 closedPos;

    private void Start()
    {
        closedPos = doorMesh.position;
    }

    private void Update()
    {
        Vector3 targetPos = isOpen ? hiddenPosition.position : closedPos;
        doorMesh.position = Vector3.Lerp(doorMesh.position, targetPos, Time.deltaTime * slideSpeed);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
}
