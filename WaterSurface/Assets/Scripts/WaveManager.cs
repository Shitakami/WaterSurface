using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    struct ThreadSize {
        public int x;
        public int y;
        public int z;

        public ThreadSize(uint x, uint y, uint z) {
            this.x = (int)x;
            this.y = (int)y;
            this.z = (int)z;
        }
    }

    [SerializeField]
    private ComputeShader m_computeShader;

    [SerializeField]
    private float m_deltaSize = 0.1f;

    [SerializeField]
    private float m_waveCoef = 1.0f;

    [SerializeField]
    private float m_decayWave;

    private RenderTexture m_waveTexture, m_drawTexture;

    private int m_kernelInitialize, m_kernelUpdate, m_kernelAddPointWave;

    private ThreadSize m_threadSizeInitialize, m_threadSizeUpdate, m_threadSizeAddPointWave;

	private Vector3 m_position;
	private Vector3 m_scale;

    // Start is called before the first frame update
    void Start()
    {

		InitializeKernelID();
		
		InitializeThreadSize();

		InitializeWaveTexture();

        // 波の高さの初期化
        m_computeShader.SetTexture(m_kernelInitialize, "waveTexture", m_waveTexture);
        m_computeShader.Dispatch(m_kernelInitialize, 
                                Mathf.CeilToInt(m_waveTexture.width/ m_threadSizeInitialize.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeInitialize.y), 
                                1);
        

		// シェーダーにテクスチャを設定する
        var mat = GetComponent<Renderer>().sharedMaterial;
        mat.SetTexture("_WaveTex", m_waveTexture);
		mat.mainTexture = m_waveTexture;

		/* 
		 * マウスでクリックしたときの座標計算で使用する
		 * 変数の初期化
		 */
		m_position = transform.position;
		m_scale = transform.localScale * 10;


	}


    private void FixedUpdate() {

		// 波の高さの更新
		m_computeShader.SetFloat("deltaSize", m_deltaSize);
		m_computeShader.SetFloat("deltaTime", Time.deltaTime * 2.0f);
		m_computeShader.SetFloat("waveCoef", m_waveCoef);
		m_computeShader.SetFloat("decayWave", m_decayWave);
		m_computeShader.SetTexture(m_kernelUpdate, "waveTexture", m_waveTexture);
		m_computeShader.Dispatch(m_kernelUpdate,
                                Mathf.CeilToInt(m_waveTexture.width / m_threadSizeUpdate.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeUpdate.y),
                                1);


    }

	public void AddWave(Vector3 point, float addRange) {


		m_computeShader.SetFloat("positionX", point.x);
		m_computeShader.SetFloat("positionY", point.y);

		m_computeShader.SetFloat("planeScaleX", m_scale.x);
		m_computeShader.SetFloat("planeScaleZ", m_scale.z);

		m_computeShader.SetFloat("addRange", addRange);
		m_computeShader.SetTexture(m_kernelAddPointWave, "waveTexture", m_waveTexture);

		m_computeShader.Dispatch(m_kernelAddPointWave,
								Mathf.CeilToInt(m_waveTexture.width / m_threadSizeAddPointWave.x),
								Mathf.CeilToInt(m_waveTexture.height / m_threadSizeAddPointWave.y),
								1);

	}

	public Vector2 CalcWaterSurfacePoint(Vector3 point) {

		Vector3 pos = m_position - point;
		pos.x += m_scale.x / 2;
		pos.z += m_scale.z / 2;

		pos.x /= m_scale.x;
		pos.z /= m_scale.z;
		
		return new Vector2(pos.x, pos.z);

	}

	private void InitializeKernelID() {

		m_kernelInitialize = m_computeShader.FindKernel("Initialize");
		m_kernelUpdate = m_computeShader.FindKernel("Update");
		m_kernelAddPointWave = m_computeShader.FindKernel("AddPointWave");

	}

	private void InitializeWaveTexture() {

		// 波の高さを格納するテクスチャの作成
		m_waveTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.RG32);
		m_waveTexture.wrapMode = TextureWrapMode.Clamp;
		m_waveTexture.enableRandomWrite = true;
		m_waveTexture.Create();

	}

	private void InitializeThreadSize() {

		uint threadSizeX, threadSizeY, threadSizeZ;

		m_computeShader.GetKernelThreadGroupSizes(m_kernelInitialize, out threadSizeX, out threadSizeY, out threadSizeZ);
		m_threadSizeInitialize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

		m_computeShader.GetKernelThreadGroupSizes(m_kernelUpdate, out threadSizeX, out threadSizeY, out threadSizeZ);
		m_threadSizeUpdate = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

		m_computeShader.GetKernelThreadGroupSizes(m_kernelAddPointWave, out threadSizeX, out threadSizeY, out threadSizeZ);
		m_threadSizeAddPointWave = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);


	}

}
