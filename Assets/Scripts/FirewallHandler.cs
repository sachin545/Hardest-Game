using System.Collections;
using UnityEngine;

public class FirewallHandler : MonoBehaviour
{
    public bool Alternate=false;
    public GameObject[] Firewall;



    public void Start()
    {
        StartCoroutine(FireWallToggle());
    }

    IEnumerator FireWallToggle()
    {


        while (true)
        {
            Firewall[0].SetActive(true);
            Firewall[1].SetActive(false);
            if (!Alternate)
            {
                Firewall[2].SetActive(true);
                Firewall[3].SetActive(false);
            }
            yield return new WaitForSeconds(4f);
            Firewall[0].SetActive(false);
            Firewall[1].SetActive(false);
            if (!Alternate)
            {
                Firewall[2].SetActive(false);
                Firewall[3].SetActive(false);
            }
            yield return new WaitForSeconds(3f);
            Firewall[0].SetActive(false);
            Firewall[1].SetActive(true);
            if (!Alternate)
            {
                Firewall[2].SetActive(false);
                Firewall[3].SetActive(true);
            }
            yield return new WaitForSeconds(4f);
            Firewall[0].SetActive(false);
            Firewall[1].SetActive(false);
            if (!Alternate)
            {
                Firewall[2].SetActive(false);
                Firewall[3].SetActive(false);
            }
            yield return new WaitForSeconds(2f);
        }
    }
}
