using UnityEngine;

public class FollowTheCar : MonoBehaviour
{
    public GameObject car;
    public GameObject barrier;

    [Tooltip("Şerit çizgisinin yol yüzeyinden Y offset'i (z-fighting'i engellemek için küçük bir değer).")]
    [SerializeField] private float laneYOffset = 0.01f;

    void Start()
    {
        PlaceLineSeperators();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Car")
        {
            PlaceLineSeperators();
        }
    }

    void PlaceLineSeperators()
    {
        transform.position = new Vector3(0, laneYOffset, car.transform.position.z - 10);
    }

    void PlaceBarriers()
    {
        
        for (int i = 0; i < 1000; i += 3)
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                GameObject barriersL = Instantiate(barrier, new Vector3(-5, 0.5f, i), Quaternion.identity);
                barriersL.transform.SetParent(GameObject.Find("Statics").transform);
            }
        }
        for (int i = 0; i < 1000; i += 3)
        {
            int random = Random.Range(0, 2);
            if (random == 0)
            {
                GameObject barriersR = Instantiate(barrier, new Vector3(5, 0.5f, i), Quaternion.identity);
                barriersR.transform.SetParent(GameObject.Find("Statics").transform);
            }
        }
    }
    

}
