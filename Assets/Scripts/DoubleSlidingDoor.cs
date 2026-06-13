using UnityEngine;

public class DoubleSlidingDoor : Interactable
{
    public Transform leftDoor;
    public Transform rightDoor;

    public Vector3 leftOpenOffset = new Vector3(-1.5f, 0, 0);
    public Vector3 rightOpenOffset = new Vector3(1.5f, 0, 0);

    public float speed = 3f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;
    private bool isMoving = false;

    void Start()
    {
        leftClosedPos = leftDoor.position;
        rightClosedPos = rightDoor.position;

        leftOpenPos = leftClosedPos 
            + leftDoor.right * leftOpenOffset.x
            + leftDoor.up * leftOpenOffset.y
            + leftDoor.forward * leftOpenOffset.z;

        rightOpenPos = rightClosedPos 
            + rightDoor.right * rightOpenOffset.x
            + rightDoor.up * rightOpenOffset.y
            + rightDoor.forward * rightOpenOffset.z;
    }

    public override void Interact()
    {
        if (!isMoving)
        {
            isOpen = !isOpen;
            StartCoroutine(MoveDoors());
        }
    }

    System.Collections.IEnumerator MoveDoors()
    {
        isMoving = true;

        Vector3 leftTarget = isOpen ? leftOpenPos : leftClosedPos;
        Vector3 rightTarget = isOpen ? rightOpenPos : rightClosedPos;

        while (
            Vector3.Distance(leftDoor.position, leftTarget) > 0.01f ||
            Vector3.Distance(rightDoor.position, rightTarget) > 0.01f
        )
        {
            leftDoor.position = Vector3.Lerp(
                leftDoor.position,
                leftTarget,
                speed * Time.deltaTime
            );

            rightDoor.position = Vector3.Lerp(
                rightDoor.position,
                rightTarget,
                speed * Time.deltaTime
            );

            yield return null;
        }

        leftDoor.position = leftTarget;
        rightDoor.position = rightTarget;

        isMoving = false;
    }
}