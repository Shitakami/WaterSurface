using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneReflection : MonoBehaviour
{

    private Transform m_trfMainCamera, m_trfRefCamera;
    private RenderTexture m_refTexture;
    private GameObject m_objRefCamera;
    private Camera m_mainCamera, m_refCamera;
    private Material m_matRefPlane;

    // Start is called before the first frame update
    void Start()
    {
        
        m_refTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);

        m_mainCamera = Camera.main;
        m_trfMainCamera = Camera.main.transform;

        m_objRefCamera = new GameObject();
        m_objRefCamera.name = "Reflection Camera";
        m_refCamera = m_objRefCamera.AddComponent<Camera>();
        m_refCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("Mirror"));

        m_trfRefCamera = m_objRefCamera.transform;
        m_refCamera.targetTexture = m_refTexture;

        m_matRefPlane = gameObject.GetComponent<Renderer>().sharedMaterial;
        m_matRefPlane.SetTexture("_ReflectionTex", m_refTexture);

        Matrix4x4 v = m_refCamera.worldToCameraMatrix;
        Matrix4x4 p = m_refCamera.projectionMatrix;

        // m_matRefPlane.SetMatrix("_RefVR", v * p);
        // m_matRefPlane.SetMatrix("_RefW", transform.localToWorldMatrix);
    }

    private void SetReflectionCamera() {

        Vector3 normal = transform.up;
        Vector3 pos = transform.position;
        Matrix4x4 mainCamMatrix = m_mainCamera.worldToCameraMatrix;

        float d = -Vector3.Dot(normal, pos);
        Matrix4x4 refMatrix = CalcReflectionMatrix(new Vector4(normal.x, normal.y, normal.z, d));

        m_refCamera.worldToCameraMatrix = m_mainCamera.worldToCameraMatrix * refMatrix;

        Vector3 cpos = m_refCamera.worldToCameraMatrix.MultiplyPoint(pos);

        Vector3 cnormal = m_refCamera.worldToCameraMatrix.MultiplyVector(normal).normalized;

        Vector4 clipPlane = new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));

        m_refCamera.projectionMatrix = m_mainCamera.CalculateObliqueMatrix(clipPlane);
    }

    private Matrix4x4 CalcReflectionMatrix(Vector4 n) {

        Matrix4x4 refMatrix = new Matrix4x4();

        refMatrix.m00 = 1f - 2f * n.x * n.x;
        refMatrix.m01 = -2f * n.x * n.y;
        refMatrix.m02 = -2f * n.x * n.z;
        refMatrix.m03 = -2f * n.x * n.w;

        refMatrix.m10 = -2f * n.x * n.y;
        refMatrix.m11 = 1f - 2f * n.y * n.y;
        refMatrix.m12 = -2f * n.y * n.z;
        refMatrix.m13 = -2f * n.y * n.w;

        refMatrix.m20 = -2f * n.z * n.x;
        refMatrix.m21 = -2f * n.z * n.y;
        refMatrix.m22 = 1f - 2f * n.z * n.z;
        refMatrix.m23 = -2f * n.z * n.w;

        refMatrix.m30 = 0f;
        refMatrix.m31 = 0f;
        refMatrix.m32 = 0f;
        refMatrix.m33 = 1f;

        return refMatrix;
    }

    private void OnWillRenderObject() {
        SetReflectionCamera();
        GL.invertCulling = true;
        m_refCamera.Render();
        GL.invertCulling = false;
        m_matRefPlane.SetTexture("_ReflectioTex", m_refTexture);
    }


}
