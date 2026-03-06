using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AreaData
{
    public int areaId;
    public string areaName;
    public AreaType areaType;


    public enum AreaType
    {
        Ocean,
        Nuclear
    }



    public static AreaData FromAreaDataModel(AreaDataModel areaModel) => new()
    {
        areaId = areaModel.areaIdx,
        areaName = areaModel.areaNm,
        areaType = (AreaType)areaModel.areaType,
    };

}
