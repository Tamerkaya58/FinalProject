using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Car")
        {
            // Aracın o anki hızına göre kazanılacak puanı hesapla
            float speed = other.GetComponent<Rigidbody>().velocity.magnitude * 3.6f; // km/h cinsinden
            float bonusPoints = speed * 5f;

            // Puanı GameManager'daki yeni sisteme ekle
            GameManager.instance.currentPoints += bonusPoints;

            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        transform.localEulerAngles += new Vector3(0, 1, 0);
    }
}