using UnityEngine;

public class DestroyObjectScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
        void Start()
        {
            float length = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
            Destroy(gameObject, length);
        }
    

}
