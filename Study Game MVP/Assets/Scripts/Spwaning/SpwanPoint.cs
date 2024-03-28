using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class SpwanPoint : MonoBehaviour
{
    public float radius;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector2 GetSpwanPoint(){
        //First, we want to set the point to our position
        Vector2 point = transform.position;

        //Next, we do calculations for how much to offset it
        float angle = Random.Range(0f, 360f);
        float power = Random.Range(0f, radius);
        
        Vector2 offset = new Vector2(Mathf.Cos(angle) * power, Mathf.Sin(angle) * power);

        //Finally, we apply that offest to the point
        return point + offset;
    }

    void OnDrawGizmos() {
        Gizmos.color = UnityEngine.Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
