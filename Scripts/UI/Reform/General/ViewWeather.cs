using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Reform.General
{
    internal class ViewWeather : MonoBehaviour
    {

        public enum WeatherType 
        {
            SUNNY,
            LITTLE_CLOUDY,
            CLOUDY,
            RAINY,
            SNOWY,
            STORMY,
        }

        TMP_Text txtWeather;
        Dictionary<WeatherType, Image> weatherImageDict = new Dictionary<WeatherType, Image>();
        Dictionary<WeatherType, string> weatherTextDict = new Dictionary<WeatherType,string>();

        private void Start()
        {
            weatherImageDict.Add(WeatherType.SUNNY, transform.Find("conWeather").Find("sunny").GetComponent<Image>());
            weatherImageDict.Add(WeatherType.LITTLE_CLOUDY, transform.Find("conWeather").Find("littleCloudy").GetComponent<Image>());
            weatherImageDict.Add(WeatherType.CLOUDY, transform.Find("conWeather").Find("cloudy").GetComponent<Image>());
            weatherImageDict.Add(WeatherType.RAINY, transform.Find("conWeather").Find("rainy").GetComponent<Image>());
            weatherImageDict.Add(WeatherType.SNOWY, transform.Find("conWeather").Find("snowy").GetComponent<Image>());
            weatherImageDict.Add(WeatherType.STORMY, transform.Find("conWeather").Find("stormy").GetComponent<Image>());

            txtWeather = transform.Find("txtWeather").GetComponent<TMP_Text>();

            weatherTextDict = new Dictionary<WeatherType, string>()
            {
                { WeatherType.SUNNY, "맑음" },
                { WeatherType.LITTLE_CLOUDY, "구름 조금" },
                { WeatherType.CLOUDY, "흐림" },
                { WeatherType.RAINY, "비" },
                { WeatherType.SNOWY, "눈" },
                { WeatherType.STORMY, "폭풍" },
            };

            UiManager.Instance.Register(UiEventType.UpdateWeather, OnUpdateWeather);
        }

        private void OnUpdateWeather(object obj)
        {
            if (obj is not int weatherCode) return;

            WeatherType weatherType = (WeatherType)weatherCode;

            foreach (var kvp in weatherImageDict)
            {
                kvp.Value.gameObject.SetActive(kvp.Key == weatherType);
            }

            if (weatherTextDict.TryGetValue(weatherType, out string weatherText))
            {
                txtWeather.text = weatherText;
            }

        }
    }
}
