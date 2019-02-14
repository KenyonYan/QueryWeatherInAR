using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocationServer : MonoBehaviour {

    public Text CurrCity;               //当前城市
    public Text Longitude;           //经度
    public Text Latitude;               //纬度
    public Text Altitude;               //海拔
    public Text text;                       //测试

    private float _timeout = 20f;      //超时时间

	void Start ()
    {
	    if(!Input.location.isEnabledByUser)
        {
            text.text += "您的设备不支持GPS功能\n";
            //print("您的设备不支持GPS功能");
            return;
        }

     StartCoroutine(GetLocationGPS());                   //开启GPS
	}

    IEnumerator GetLocationGPS()
    {
        Input.location.Start();                 //打开GPS
        float time = 0;                              //检测时间
        while(Input.location.status == LocationServiceStatus.Initializing && _timeout > time)           //循环检测打开GPS是否超时
        {
            yield return new WaitForSeconds(1f);
            time++;
        }

        if((Input.location.status == LocationServiceStatus.Failed) || (time >= _timeout) 
            || (Input.location.status != LocationServiceStatus.Running))
        {
            text.text += "GPS服务启动失败\n";
            //print("GPS服务启动失败");
            yield break;
        }
        else                    //获取经纬度信息
        {
            Longitude.text = Input.location.lastData.longitude.ToString();
            Latitude.text = Input.location.lastData.latitude.ToString();
            Altitude.text = Input.location.lastData.altitude.ToString();
        }

        string url = "http://api.map.baidu.com/geocoder/v2/?location=" + Input.location.lastData.latitude + ", " +
                              Input.location.lastData.longitude + "&output=json&pois=1&ak=E15fQ4IuLwFfkjaGdgp2bTiSHOTiykDV";

        WWW www = new WWW(url);
        yield return www;
        BaiduLocation locationData = JsonUtility.FromJson<BaiduLocation>(www.text);
        CurrCity.text = locationData.result.addressComponent.city;
        WeatherQuerier localWeather = new WeatherQuerier();
        localWeather.GetLocalWeather(CurrCity.text);                        //获取本地天气
    }

	[System.Serializable]
    public class BaiduLocation
    {
        public Result result;
    }

    [System.Serializable]
    public class Result
    {
        public AddressComponent addressComponent;
    }

    [System.Serializable]
    public class AddressComponent
    {
        public string city;
    }



}
