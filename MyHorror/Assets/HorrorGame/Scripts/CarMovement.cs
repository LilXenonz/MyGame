using UnityEngine;

public class CarMovement : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {

        transform.Translate(Vector3.forward * 5 * Time.deltaTime);

    }
}
