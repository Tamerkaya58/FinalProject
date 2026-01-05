using UnityEngine;

public class TntBomb : BaseHazard
{


    override protected void OnInitialize()
    {
        // Randomize rotation around the Y axis
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }

    /* 
        use OnHazardCollision instead of OnCollisionEnter it overrides from base class 
        doing otherwise can cause issues of not calling the base class method of OnCollisionEnter
        thus OnCollisionEnter based logics not triggering for the all hazards that has tnt bomb as child class
    */ 
    override protected void OnHazardCollision(GameObject player)
    {
        // Example: Print a message and destroy the bomb
        Debug.Log("TNT Bomb exploded on player!");
        Destroy(transform.parent.gameObject);

    }
}
