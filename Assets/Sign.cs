using UnityEngine;

public class Sign : MonoBehaviour
{
    public GameObject signMenu;

    private void Start()
    {
        signMenu.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered");

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player touched sign");
            signMenu.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            signMenu.SetActive(false);
        }
    }
}