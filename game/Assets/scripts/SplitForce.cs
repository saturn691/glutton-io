using UnityEngine;

public class SplitForce : MonoBehaviour
{
    public float Speed;
    public float loseSpeed = 0f; // Adjust this value for slower decay
    public float DefaultSpeed = 50; // Adjust this value for initial speed

    public bool ApplyForce = false;

public void SplitForceMethod()
{
    GetComponent<CircleCollider2D>().enabled = false;
    GetComponent<PlayerMovement>().LockActions = true;

    Vector2 Dir = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    float rot = Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg + 90f;
    transform.rotation = Quaternion.Euler(0, 0, rot);

    Speed = DefaultSpeed;
    Debug.Log("Default Speed: " + DefaultSpeed); // Debug log for DefaultSpeed
    ApplyForce = true;
}

private void Update()
{
    if (ApplyForce == false)
    {
        enabled = false;
        return;
    }

    transform.Translate(Vector2.up * Speed * Time.deltaTime);
    Speed -= loseSpeed * Time.deltaTime;

    Debug.Log("Speed: " + Speed); // Debug log for Speed
    Debug.Log("Lose Speed: " + loseSpeed); // Debug log for loseSpeed

    if (Speed <= 0)
    {
        GetComponent<CircleCollider2D>().enabled = true;
        GetComponent<PlayerMovement>().LockActions = false;
        enabled = false;
    }
}

}
