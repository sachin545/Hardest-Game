using UnityEngine;

public class DoorController : MonoBehaviour
{

    public int requiredKeys = 1;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
          int cnt = collision.gameObject.GetComponent<PlayerController>().KeyCount; 

            if(cnt==requiredKeys)
            {
                Destroy(this.gameObject);
            }
        }
    }
}

