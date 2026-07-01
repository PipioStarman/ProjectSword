using UnityEngine;

public class TestinThings : MonoBehaviour
{
    public GameObject redball;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Instantiate(redball,this.gameObject.transform.position, this.gameObject.transform.rotation);
        }
    }
}
