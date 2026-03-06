using Assets.Scripts.Info;
using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Assets.Scripts.Info.ObservatoryInfo;

namespace Assets.Scripts.Info
{
    class BoardStateInfo
    {

        public BoardStateInfo(BoardStateData data)
        {
            obsIdx = data.OBS_IDX;
            timestamp = DateTime.ParseExact(data.MEAS_DT,
                "yyyyMMddHHmmss",
                System.Globalization.CultureInfo.InvariantCulture);

            BoardInfo toxinBoard = new BoardInfo(
                data.BRD_TOXIN_MODEL_CODE,

                data.BRD_TOXIN_LIFE_STATE,
                data.BRD_TOXIN_OP_STATE,
                data.BRD_TOXIN_COM_STATE,

                false,
                false,

                0,
                false,
                0,
                false,
                0,
                false
            );
            boards.Add((BoardSpecInfo.BoardType.TOXIN, toxinBoard));

            BoardInfo hnsBoard = new BoardInfo(
                data.BRD_HNS_MODEL_CODE,

                data.BRD_HNS_LIFE_STATE,
                data.BRD_HNS_OP_STATE,
                data.BRD_HNS_COM_STATE,

                false,
                false,

                0,
                false,
                0,
                false,
                0,
                false
            );
            boards.Add((BoardSpecInfo.BoardType.HNS, hnsBoard));

            BoardInfo wqBoard = new BoardInfo(
                data.BRD_WQ_MODEL_CODE,

                data.BRD_WQ_LIFE_STATE,
                data.BRD_WQ_OP_STATE,
                data.BRD_WQ_COM_STATE,

                false,
                false,

                0,
                false,
                0,
                false,
                0,
                false
            );
            boards.Add((BoardSpecInfo.BoardType.WQ, wqBoard));
        }

        public int obsIdx;
        public DateTime timestamp;
        public List<(BoardSpecInfo.BoardType type, BoardInfo info)> boards = new();



    }
}
