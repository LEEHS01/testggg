using Assets.Scripts.ModelsReform;
using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public class GroupInfo
    {
        public GroupInfo(GroupData groupData)
        {
            groupIdx = groupData.GROUP_IDX;
            groupName = groupData.GROUP_NAME;
            groupType = (GroupType)groupData.GROUP_TYPE;
            coordinate = new Vector2(groupData.DP_POS_LON, groupData.DP_POS_LAT);
        }
        public int groupIdx;
        public string groupName;
        public GroupType groupType;
        public Vector2 coordinate;

        public enum GroupType
        {
            GENERAL = -1,
            OCEAN = 1,
            NUCLEAR = 2,
        }
    }
}
