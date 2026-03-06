using Assets.Scripts.ModelsReform;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Assets.Scripts.Info
{
    public class BoardSpecInfo
    {

        public enum BoardType
        {
            TOXIN,
            HNS,
            WQ,
        }

        public BoardSpecInfo(BoardSpecData brdSpecData)
        {
            modelCode = brdSpecData.BRD_MODEL_CODE;
            nameText = brdSpecData.BRD_NAME;
            manufacturer = brdSpecData.BRD_MANUFACTURER;
            contactManager = brdSpecData.BRD_CONTACT_MANAGER;
            contactCallNums = brdSpecData.BRD_CONTACT_CALL;
            descriptionText = brdSpecData.BRD_DESCRIPTION;

            try {
                var default_json = JsonConvert.DeserializeObject<
                    Dictionary<string, ThresholdDto>
                >(brdSpecData.BRD_SENSOR_DEFAULT_JSON);

                sensorsDefaultMap = default_json.ToDictionary(
                    kv => int.Parse(kv.Key),
                    kv => (kv.Value.th_hi, kv.Value.th_lo)
                );


                var define_json = JsonConvert.DeserializeObject<
                    Dictionary<string, NameUnitDto>
                >(brdSpecData.BRD_SENSOR_DEFINITION_JSON);

                sensorsDefinitionMap = define_json.ToDictionary(
                    kv => int.Parse(kv.Key),
                    kv => (kv.Value.name, kv.Value.unit)
                );
            }
            catch(Exception ex)
            {
                //UnityEngine.Debug.LogError($"BoardSpecInfo JSON Parse Error: {ex.Message}");
                sensorsDefaultMap = new Dictionary<int, (float th_hi, float th_lo)>();
                sensorsDefinitionMap = new Dictionary<int, (string name, string unit)>();
            }
        }


        public string modelCode;
        public string nameText;
        public string? manufacturer;
        public string? contactManager;
        public string? contactCallNums;
        public string descriptionText;

        public Dictionary<int, (float th_hi,float th_lo)> sensorsDefaultMap;
        public Dictionary<int, (string name, string unit)> sensorsDefinitionMap;


        class ThresholdDto
        {
            public float th_hi;
            public float th_lo;
        }
        class NameUnitDto
        {
            public string name;
            public string unit;
        }

    }
}
