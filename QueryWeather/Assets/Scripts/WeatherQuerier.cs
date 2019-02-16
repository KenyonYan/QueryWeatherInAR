using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using Unity.IO.Compression;
using UnityEngine;
using UnityEngine.UI;

public class WeatherQuerier : MonoBehaviour {

    public Text InputText;                                   //输入的城市
    public Text Display;                                       //显示查询城市的天气信息
    

    private ResponseData response;              //反序列天气信息
    private string _cityName;

    public void Query()
    {
        Display.text = "";
        _cityName = InputText.text;
        StartCoroutine(Weather());             
    }

    public void GetLocalWeather(string cityName)            //获取本地天气
    {
        _cityName = cityName;
        StartCoroutine(Weather());
    }

    IEnumerator Weather()
    {
        WWW www = new WWW("http://wthrcdn.etouch.cn/weather_mini?city=" + _cityName);              //通过天气api获取天气信息
        yield return www;
        //print(www.text);
        //string result = Decompress(www.bytes);              //解压缩天气信息
        string result = ungzip(www.bytes);              //解压缩天气信息
        print(result);
        GetWeatherDate(result);
    }


/// <summary>
/// 解压缩文件
/// </summary>
/// <param name="bytes"></param>
/// <returns></returns>
string Decompress(byte[] bytes)
{
    var lengthBuffer = new byte[4];
    System.Array.Copy(bytes, bytes.Length - 4, lengthBuffer, 0, 4);
    int uncompressedSize = System.BitConverter.ToInt32(lengthBuffer, 0);
    var buffer = new byte[uncompressedSize];
    using  (var ms = new MemoryStream(bytes))
    {
       using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
       {
           gzip.Read(buffer, 0, uncompressedSize);
       }
    }
return System.Text.Encoding.UTF8.GetString(buffer);
}


    /// <summary>
    /// 测试gzip解压
    /// </summary>
    /// <param name="compressedStr"></param>
    /// <returns></returns>
    public static string ungzip(byte[] bytes)
    {
        using (var compressedStream = new MemoryStream(bytes))
        using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            var buffer = new byte[4096 * 2];
            int read;

            while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                resultStream.Write(buffer, 0, read);
            }

            return Encoding.UTF8.GetString(resultStream.ToArray());
        }
    }




    [System.Serializable]
    public class ResponseData
    {
        public string desc;                             //是否成功获取天气信息
        public WeatherData data;               
    }

    [System.Serializable]
    public class WeatherData
    {
        public string city;                                                        //城市名称
        public string ganmao;                                               //穿衣建议
        public string wendu;                                                 //今日温度
        public weatherForecastData[] forecast;               //未来几日信息
    }

    [System.Serializable]
    public class weatherForecastData
    {
        public string date;                         //日期信息
        public string high;                         //最高气温
        public string fengxiang;               //风向
        public string low;                           //最低气温
        public string fengli;                       //风力
        public string type;                         //天气类型
    }

    private void GetWeatherDate(string result)
    {
        response = JsonUtility.FromJson<ResponseData>(result);
        if(response.desc == "OK")
        {
            TodayWeatherData();                             //今日天气
            FutureWeatherData();                            //未来4日天气
        }
        else
        {
            Display.text = "无该城市天气信息，请重新输入";
        }
     }

    private int SolveWindPower(string str)
    {
        return str.IndexOf(']');                                //提取正确风力信息
    }

    private void TodayWeatherData()
    {
        Display.text += "城市：" + response.data.city + "\n" +
                                     "今日 " + response.data.forecast[0].date + "\n" +
                                     "最高温度：" + response.data.forecast[0].high + "\n" +
                                     "最低温度：" + response.data.forecast[0].low + "\n" +
                                     "风向：" + response.data.forecast[0].fengxiang + "\n" +
                                     "风力：" + response.data.forecast[0].fengli.Substring(9, SolveWindPower(response.data.forecast[0].fengli) - 9) + "\n" +
                                     "天气：" + response.data.forecast[0].type + "\n" +
                                     "穿衣建议：" + response.data.ganmao + "\n\n";
    }

    private void FutureWeatherData()
    {
        Display.text += "未来4天天气：\n";
        for (int i = 1; i < response.data.forecast.Length; i++)
        {
            Display.text += "日期：" + response.data.forecast[i].date + "\n" +
                                          "最高温度：" + response.data.forecast[i].high + "\n" +
                                          "最低温度：" + response.data.forecast[i].low + "\n" +
                                          "风向：" + response.data.forecast[i].fengxiang + "\n" +
                                          "风力：" + response.data.forecast[i].fengli.Substring(9, SolveWindPower(response.data.forecast[i].fengli) - 9) + "\n" +
                                          "天气：" + response.data.forecast[i].type + "\n\n";
        }
    }
}
