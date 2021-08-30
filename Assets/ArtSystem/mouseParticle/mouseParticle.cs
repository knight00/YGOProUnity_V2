using UnityEngine;
using System.Collections;

public class mouseParticle : MonoBehaviour {
    public Camera camera;
    public ParticleSystem e1;
    public ParticleSystem e2;
    public Transform trans;
    // Use this for initialization
    void Start () {
        camera.depth = 99999;
    }
    float time = 0;
	// Update is called once per frame
	void Update () {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 10;
        trans.position = camera.ScreenToWorldPoint(screenPoint);

        if (Input.GetMouseButtonDown(0))
        {
            e1.Play();
            e2.Play();
        }

        if (Input.GetMouseButtonUp(0))
        {
            e1.Stop();
            e2.Stop();
        }
    }
}
