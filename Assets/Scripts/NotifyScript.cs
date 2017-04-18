using UnityEngine;
using System.Collections;

public class NotifyScript : MonoBehaviour
{
	
    // Use this for initialization
    void Start()
    {
        StartCoroutine(MyMethod());

    }

    // Update is called once per frame
    void Update()
    {


    }

    IEnumerator MyMethod()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
        yield return new WaitForSeconds(2f);
        Destroy(this.transform.gameObject);
    }
}
