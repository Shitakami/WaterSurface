using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWave : MonoBehaviour
{
    [SerializeField]
    private float m_addRange;

	[SerializeField]
	private WaveManager m_waveManager;
    private Camera m_camera;



    // Start is called before the first frame update
    void Start()
    {
        m_camera = Camera.main;

		if (m_waveManager == null)
			m_waveManager = GameObject.FindGameObjectWithTag("WaveSurface").GetComponent<WaveManager>();
		

    }

    // Update is called once per frame
    void Update()
    {
        
        if(!Input.GetMouseButton(0))
            return;

        RaycastHit hit;
		Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

		if (!Physics.Raycast(ray, out hit) || !hit.collider.CompareTag("WaterSurface"))
            return;

		var point = m_waveManager.CalcWaterSurfacePoint(hit.point);

		m_waveManager.AddWave(point, m_addRange);


    }


}
