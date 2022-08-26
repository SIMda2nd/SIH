using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sitinship : MonoBehaviour
{

    [SerializeField] private Spaceship spaceship;
    private ZeroGMovement player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.gameObject.GetComponentInParent<ZeroGMovement>();
            if (player != null)
                player.ShipToEnter = spaceship;
            // uiObject.SetActive(true);
            print("Player in Interaction Zone");
            //StartCoroutine("WaitForSec");

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("Player is exiting zone");
            if (player != null)
                player.ShipToEnter = null;
        }
    }

    //IEnumerator WaitForSec()
    //{
    //    yield return new WaitForSeconds(5);
    //    Destroy(uiObject);
    //}
}
