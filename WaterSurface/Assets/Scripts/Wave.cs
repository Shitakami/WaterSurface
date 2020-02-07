using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave : MonoBehaviour
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
    private GameObject m_plane;

    [SerializeField]
    private ComputeShader m_computeShader;

    [SerializeField]
    private float m_deltaSize = 0.1f;

    [SerializeField]
    private float m_waveCoef = 1.0f;

    private RenderTexture m_waveTexture, m_drawTexture;

    private int m_kernelInitialize, m_kernelAddWave, m_kernelUpdate, m_kernelDraw;

    private ThreadSize m_threadSizeInitialize, m_threadSizeUpdate, m_threadSizeDraw;
    

    // Start is called before the first frame update
    void Start()
    {
        
        m_kernelInitialize = m_computeShader.FindKernel("Initialize");
        m_kernelAddWave = m_computeShader.FindKernel("AddWave");
        m_kernelUpdate = m_computeShader.FindKernel("Update");
        m_kernelDraw = m_computeShader.FindKernel("Draw");


        // 波の高さを格納するテクスチャの作成
        m_waveTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.RG32);
        m_waveTexture.wrapMode = TextureWrapMode.Clamp;
        m_waveTexture.enableRandomWrite = true;
        m_waveTexture.Create();

        // レンダリング用のテクスチャの作成
        m_drawTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        m_drawTexture.enableRandomWrite = true;
        m_drawTexture.Create();

        // スレッド数の取得
        uint threadSizeX, threadSizeY, threadSizeZ;
        m_computeShader.GetKernelThreadGroupSizes(m_kernelInitialize, out threadSizeX, out threadSizeY, out threadSizeZ);
        m_threadSizeInitialize = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        m_computeShader.GetKernelThreadGroupSizes(m_kernelUpdate, out threadSizeX, out threadSizeY, out threadSizeZ);
        m_threadSizeUpdate = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);
        m_computeShader.GetKernelThreadGroupSizes(m_kernelDraw, out threadSizeX, out threadSizeY, out threadSizeZ);
        m_threadSizeDraw = new ThreadSize(threadSizeX, threadSizeY, threadSizeZ);

        // 波の高さの初期化
        m_computeShader.SetTexture(m_kernelInitialize, "waveTexture", m_waveTexture);
        m_computeShader.Dispatch(m_kernelInitialize, 
                                Mathf.CeilToInt(m_waveTexture.width/ m_threadSizeInitialize.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeInitialize.y), 
                                1);
        
    }

    private void FixedUpdate() {

        // 波の追加
        m_computeShader.SetFloat("time", Time.time);
        m_computeShader.SetTexture(m_kernelAddWave, "waveTexture", m_waveTexture);
        m_computeShader.Dispatch(m_kernelAddWave,
                                Mathf.CeilToInt(m_waveTexture.width / m_threadSizeUpdate.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeUpdate.y),
                                1);

        // 波の高さの更新
        m_computeShader.SetFloat("deltaSize", m_deltaSize);
        m_computeShader.SetFloat("deltaTime", Time.deltaTime * 2.0f);
        m_computeShader.SetFloat("waveCoef", m_waveCoef);
        m_computeShader.SetTexture(m_kernelUpdate, "waveTexture", m_waveTexture);
        m_computeShader.Dispatch(m_kernelUpdate,
                                Mathf.CeilToInt(m_waveTexture.width / m_threadSizeUpdate.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeUpdate.y),
                                1);
        
        // 波の高さを元にレンダリング用のテクスチャを作成
        m_computeShader.SetTexture(m_kernelDraw, "waveTexture", m_waveTexture);
        m_computeShader.SetTexture(m_kernelDraw, "drawTexture", m_drawTexture);
        m_computeShader.Dispatch(m_kernelDraw,
                                Mathf.CeilToInt(m_waveTexture.width / m_threadSizeDraw.x),
                                Mathf.CeilToInt(m_waveTexture.height / m_threadSizeDraw.y),
                                1);
        m_plane.GetComponent<Renderer>().material.mainTexture = m_drawTexture;

    }

}
