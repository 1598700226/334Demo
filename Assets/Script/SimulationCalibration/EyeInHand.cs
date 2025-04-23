using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeInHand : MonoBehaviour
{

    public GameObject EndPoint;
    public GameObject Base;
    public GameObject Camera;
    public GameObject Target;

    public Matrix4x4 Camera2EndPoint_True;
    public Matrix4x4 Camera2EndPoint;
    public Matrix4x4 EndPoint2Base;
    public Matrix4x4 Target2Camera;

    public bool isClearData = false;
    public bool isAddRT = false;
    public bool isCalibration = false;

    private List<Matrix<double>> e2blist_r = new List<Matrix<double>>();
    private List<Matrix<double>> t2clist_r = new List<Matrix<double>>();
    private List<Matrix<double>> e2blist_t = new List<Matrix<double>>();
    private List<Matrix<double>> t2clist_t = new List<Matrix<double>>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 目标到相机的变换矩阵
        Target2Camera = CalculateRelativeTransform(Target.transform, Camera.transform);
        // 相机到base变换矩阵
        EndPoint2Base = CalculateRelativeTransform(EndPoint.transform, Base.transform);
        // 相机到末端
        Camera2EndPoint_True = CalculateRelativeTransform(Camera.transform, EndPoint.transform);

        if (isClearData)
        {
            isClearData = false;
            e2blist_r.Clear();
            e2blist_t.Clear();
            t2clist_r.Clear();
            t2clist_t.Clear();
        }

        if (isAddRT || Input.GetKey(KeyCode.KeypadEnter) || Input.GetKey(KeyCode.Return))
        {
            isAddRT = false;
            Matrix4x4ToEmgucvMatrixRT(EndPoint2Base, out Matrix<double> e2b_r, out Matrix<double> e2b_t);
            Matrix4x4ToEmgucvMatrixRT(Target2Camera, out Matrix<double> t2c_r, out Matrix<double> t2c_t);
            e2blist_r.Add(e2b_r);
            e2blist_t.Add(e2b_t);
            t2clist_r.Add(t2c_r);
            t2clist_t.Add(t2c_t);
        }

        if (isCalibration)
        {
            isCalibration = false;
            EyeInHandCalibration(e2blist_r, e2blist_t, t2clist_r, t2clist_t, out Matrix<double> r, out Matrix<double> t);
            Camera2EndPoint = BuildMatrix4x4ByRT(r, t);
        }
    }

    /// <summary>
    ///  A 到 B 的位姿变换关系
    /// </summary>
    Matrix4x4 CalculateRelativeTransform(Transform A, Transform B)
    {
        Matrix4x4 matrixA = A.localToWorldMatrix;
        Matrix4x4 matrixB = B.localToWorldMatrix;
        Matrix4x4 inverseMatrixB = matrixB.inverse;
        Matrix4x4 relativeMatrix = inverseMatrixB * matrixA;
        return relativeMatrix;
    }

    /// <summary>
    /// 眼在手上标定，输入的是3*3的旋转矩阵
    /// </summary>
    private void EyeInHandCalibration(List<Matrix<double>> endPoint2BaseR, List<Matrix<double>> endPoint2BaseT,
        List<Matrix<double>> target2CameraR, List<Matrix<double>> target2CameraT,
        out Matrix<double> camera2EndpointR, out Matrix<double> camera2EndpointT)
    {
        // 旋转矩阵转为旋转向量
        VectorOfMat vMatEndPoint2BaseR = new VectorOfMat();
        VectorOfMat vMatEndPoint2BaseT = new VectorOfMat();
        VectorOfMat vMatTarget2CameraR = new VectorOfMat();
        VectorOfMat vMatTarget2CameraT = new VectorOfMat();
        for (int i = 0; i < endPoint2BaseR.Count; i++)
        {
            vMatEndPoint2BaseR.Push(endPoint2BaseR[i]);
            vMatTarget2CameraR.Push(target2CameraR[i]);
            vMatEndPoint2BaseT.Push(endPoint2BaseT[i]);
            vMatTarget2CameraT.Push(target2CameraT[i]);
        }

        Mat temp_r = new Mat();
        Mat temp_t = new Mat();
        CvInvoke.CalibrateHandEye(vMatEndPoint2BaseR, vMatEndPoint2BaseT,
            vMatTarget2CameraR, vMatTarget2CameraT,
            temp_r, temp_t,
            HandEyeCalibrationMethod.Tsai);

        // 旋转矩阵和平移矩阵
        camera2EndpointR = new Matrix<double>(3, 3);
        temp_r.CopyTo(camera2EndpointR);

        camera2EndpointT = new Matrix<double>(3, 1);
        temp_t.CopyTo(camera2EndpointT);
    }

    private void Matrix4x4ToEmgucvMatrixRT(Matrix4x4 matrix, out Emgu.CV.Matrix<double> R, out Emgu.CV.Matrix<double> T)
    {
        R = new Emgu.CV.Matrix<double>(3, 3);
        T = new Emgu.CV.Matrix<double>(3, 1);
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                R.Data[i, j] = matrix[i, j];
            }
        }
        // 提取平移矩阵
        T[0, 0] = matrix[0, 3];
        T[1, 0] = matrix[1, 3];
        T[2, 0] = matrix[2, 3];
    }

    public Matrix4x4 BuildMatrix4x4ByRT(Emgu.CV.Matrix<double> rotation, Emgu.CV.Matrix<double> translation)
    {
        Matrix4x4 matrix = new Matrix4x4(
            new Vector4((float)rotation.Data[0, 0], (float)rotation.Data[1, 0], (float)rotation.Data[2, 0], 0),
            new Vector4((float)rotation.Data[0, 1], (float)rotation.Data[1, 1], (float)rotation.Data[2, 1], 0),
            new Vector4((float)rotation.Data[0, 2], (float)rotation.Data[1, 2], (float)rotation.Data[2, 2], 0),
            new Vector4((float)translation.Data[0, 0], (float)translation.Data[1, 0], (float)translation.Data[2, 0], 1)
            );
        return matrix;
    }
}