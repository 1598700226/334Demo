using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    /*############## 相机相关UI控件 ##############*/
    public RawImage rawImage1;
    private Texture2D RawImageTexture1;
    public RawImage rawImage2;
    private Texture2D RawImageTexture2;
    public TMP_Dropdown dropdown_Camera1Show;
    public TMP_Dropdown dropdown_Camera2Show;
    public TMP_Text laber_camera_fps;

    public Button SubscribeCamera1Btn;
    public Button SubscribeCamera2Btn;
    public Button UnSubscribeAllBtn;

    private uint lastCameraNanoTime = 0;
    private uint lastCameraSecTime = 0;

    /*############## RealSense相机内参 ##############*/
    private int Image_Width = 640;
    private int Image_Height = 480;
    private float fx = 608.912109375f;
    private float fy = 607.4900512695312f;
    private float ppx = 316.9831848144531f;
    private float ppy = 245.9309844970703f;

    public bool IsBuliding = false; // 是否正在构建点云
    public int showType_camera1 = 0; // 0是不显示，1是彩色图 2是深度图
    public int showType_camera2 = 0; // 0是不显示，1是彩色图 2是深度图
    private Color[] Colors;     // 仅使用1的数据
    private ushort[] Depths;

    /*############## ROS话题与服务 ##############*/
    [Space(5)]
    public string color_image_1_topic = "/camera1/color/image_raw/compressed";
    public string depth_image_1_topic = "/camera1/aligned_depth_to_color/image_raw";
    public string color_image_2_topic = "/camera2/color/image_raw/compressed";
    public string depth_image_2_topic = "/camera2/aligned_depth_to_color/image_raw";

    // Start is called before the first frame update
    void Start()
    {
        RawImageTexture1 = new Texture2D(Image_Width, Image_Height);
        RawImageTexture2 = new Texture2D(Image_Width, Image_Height);
        SubscribeCamera1Btn.onClick.AddListener(SubscribeCamera1);
        SubscribeCamera2Btn.onClick.AddListener(SubscribeCamera2);
        UnSubscribeAllBtn.onClick.AddListener(UnSubscribeAll);
    }

    // Update is called once per frame
    void Update()
    {
        showType_camera1 = dropdown_Camera1Show.value;
        showType_camera2 = dropdown_Camera2Show.value;
        CameraViewUpdate();
    }

    void SubscribeCamera1()
    {
        Subscribe_Camera1_Topics(true, true);
    }

    void SubscribeCamera2()
    {
        Subscribe_Camera2_Topics(true, true);
    }

    void UnSubscribeAll()
    {
        UnSubscribe_Camera1_Topics();
        UnSubscribe_Camera2_Topics();
    }

    void CameraViewUpdate()
    {
        // 图片控件显示
        if (showType_camera1 <= 0)
        {
            rawImage1.enabled = false;
        }
        else
        {
            rawImage1.enabled = true;
        }

        if (showType_camera2 <= 0)
        {
            rawImage2.enabled = false;
        }
        else
        {
            rawImage2.enabled = true;
        }
    }

    public void Subscribe_Camera1_Topics(bool color_topic = true, bool depth_topic = true)
    {
        if (color_topic)
        {
            if (!ROSConnection.GetOrCreateInstance().HasSubscriber(color_image_1_topic))
                ROSConnection.GetOrCreateInstance().
                    Subscribe<RosMessageTypes.Sensor.CompressedImageMsg>(color_image_1_topic, Camera1_CompressedColorImageCall);
        }

        if (depth_topic)
        {
            if (!ROSConnection.GetOrCreateInstance().HasSubscriber(depth_image_1_topic))
                ROSConnection.GetOrCreateInstance().
                    Subscribe<RosMessageTypes.Sensor.ImageMsg>(depth_image_1_topic, Camera1_CompressedDepthImageCall);
        }
    }

    public void Subscribe_Camera2_Topics(bool color_topic = true, bool depth_topic = true)
    {
        if (color_topic)
        {
            if (!ROSConnection.GetOrCreateInstance().HasSubscriber(color_image_2_topic))
                ROSConnection.GetOrCreateInstance().
                    Subscribe<RosMessageTypes.Sensor.CompressedImageMsg>(color_image_2_topic, Camera2_CompressedColorImageCall);
        }

        if (depth_topic)
        {
            if (!ROSConnection.GetOrCreateInstance().HasSubscriber(depth_image_2_topic))
                ROSConnection.GetOrCreateInstance().
                    Subscribe<RosMessageTypes.Sensor.ImageMsg>(depth_image_2_topic, Camera2_CompressedDepthImageCall);
        }
    }

    public void UnSubscribe_Camera1_Topics()
    {
        ROSConnection.GetOrCreateInstance().Unsubscribe(color_image_1_topic);
        ROSConnection.GetOrCreateInstance().Unsubscribe(depth_image_1_topic);
    }

    public void UnSubscribe_Camera2_Topics()
    {
        ROSConnection.GetOrCreateInstance().Unsubscribe(color_image_2_topic);
        ROSConnection.GetOrCreateInstance().Unsubscribe(depth_image_2_topic);
    }

    void Camera1_CompressedColorImageCall(RosMessageTypes.Sensor.CompressedImageMsg CompressedColorImage)
    {
        if (!IsBuliding)
        {
            // 计算视频反馈时间
            uint nano_time = CompressedColorImage.header.stamp.nanosec;
            uint sec_time = CompressedColorImage.header.stamp.sec;
            int diffmstime = (int)(sec_time - lastCameraSecTime) * 1000;
            if (nano_time > lastCameraNanoTime)
            {
                diffmstime += (int)((nano_time - lastCameraNanoTime) / 1000000);
            }
            else
            {
                diffmstime -= (int)((lastCameraNanoTime - nano_time) / 1000000);
            }
            float fps = (float)(1000.0 / diffmstime);
            laber_camera_fps.text = fps.ToString("F2");
            lastCameraNanoTime = nano_time;
            lastCameraSecTime = sec_time;

            Debug.Log($"【CompressedColorImageCall】获得压缩图片, sec:{sec_time},nano_time:{nano_time}");
            Colors = JPEGToRGB(CompressedColorImage.data);

            if (showType_camera1 == 1)
                UpdateCompressedColorImage(CompressedColorImage.data, rawImage1, RawImageTexture1);
        }
    }

    void Camera2_CompressedColorImageCall(RosMessageTypes.Sensor.CompressedImageMsg CompressedColorImage)
    {
        if (!IsBuliding)
        {
            Debug.Log("【CompressedColorImageCall】获得压缩图片");
            if (showType_camera2 == 1)
                UpdateCompressedColorImage(CompressedColorImage.data, rawImage2, RawImageTexture2);
        }
    }

    private Color[] JPEGToRGB(byte[] jpegData)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(jpegData);
        Color[] rgbArray = texture.GetPixels();
        Color[] ret = new Color[rgbArray.Length];
        for (int w = 0; w < Image_Width; w++)
            for (int h = 0; h < Image_Height; h++)
            {
                // 需要换乘左上角开始，GetPixels()是左下角的数据
                ret[w + h * Image_Width] = rgbArray[w + (Image_Height - 1 - h) * Image_Width];
            }

        Destroy(texture);
        return ret;
    }

    private void UpdateCompressedColorImage(byte[] data, RawImage rawImage, Texture2D RawImageTexture)
    {
        if (rawImage == null)
        {
            Debug.Log("【UnitySubscription_PointCloud error】rawImage控件为空");
            return;
        }

        RawImageTexture.LoadImage(data);
        // 将Texture2D显示在RawImage上
        RawImageTexture.Apply();  // 应用像素更改
        rawImage.texture = RawImageTexture;
    }

    void Camera1_CompressedDepthImageCall(RosMessageTypes.Sensor.ImageMsg DepthImage)
    {
        if (!IsBuliding)
        {
            Depths = ConvertBytesToUShort(DepthImage.data);
            if (showType_camera1 == 2)
                UpdateRawImageByUshortData(Depths, rawImage1, RawImageTexture1);
        }
    }

    void Camera2_CompressedDepthImageCall(RosMessageTypes.Sensor.ImageMsg DepthImage)
    {
        if (!IsBuliding)
        {
            if (showType_camera2 == 2)
                UpdateRawImageByUshortData(ConvertBytesToUShort(DepthImage.data), rawImage2, RawImageTexture2);
        }
    }

    ushort[] ConvertBytesToUShort(byte[] bytesData)
    {
        if (bytesData.Length % 2 != 0)
        {
            Debug.LogError("Invalid byte array length for 16UC1 data.");
            return null;
        }
        ushort[] ushortData = new ushort[bytesData.Length / 2];

        ushort max = ushort.MinValue;
        ushort min = ushort.MaxValue;
        for (int i = 0; i < ushortData.Length; i++)
        {
            // 根据端序将 byte[] 转为 ushort
            if (System.BitConverter.IsLittleEndian)
            {
                ushortData[i] = (ushort)(bytesData[2 * i] | (bytesData[2 * i + 1] << 8));
            }
            else
            {
                ushortData[i] = (ushort)((bytesData[2 * i] << 8) | bytesData[2 * i + 1]);
            }

            if (ushortData[i] > max)
                max = ushortData[i];
            if (ushortData[i] < min)
                min = ushortData[i];
        }
        return ushortData;
    }

    private void UpdateRawImageByUshortData(ushort[] data, RawImage rawImage, Texture2D RawImageTexture)
    {
        if (rawImage == null)
        {
            Debug.Log("【UnitySubscription_PointCloud error】rawImage控件为空");
            return;
        }
        RawImageTexture = new Texture2D(Image_Width, Image_Height, TextureFormat.RGBA32, false);
        ushort max = 0;
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] > max)
                max = data[i];
        }
        // 将 ushort[] 数据转换为颜色信息
        Color[] colors = new Color[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            float normalizedValue = (float)data[i] / max;
            colors[i] = new Color(normalizedValue, normalizedValue, normalizedValue, 255);
        }
        // 将颜色信息应用到 Texture2D 上, 需要颠倒显示
        for (int y = 0; y < Image_Height; y++)
        {
            for (int x = 0; x < Image_Width; x++)
            {
                RawImageTexture.SetPixel(x, Image_Height - 1 - y, colors[x + y * Image_Width]);  // 需要翻转y轴以匹配Unity坐标系
            }
        }
        RawImageTexture.Apply();  // 应用像素更改
        rawImage.texture = RawImageTexture;
    }
}
