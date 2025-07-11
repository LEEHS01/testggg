using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Onthesys
{
    public class AlarmCount
    {
        public AlarmCount() => new AlarmCount(0, 0, 0, 0);
        public AlarmCount(int green, int yellow, int red, int purple) 
        {
            this.green = green;
            this.yellow = yellow;
            this.red = red;
            this.purple = purple;
        }

        public int green;
        public int red;
        public int yellow;
        public int purple;//������ �κ�

        /*
        public void CreateAlramDatas()
        {
            this.green = Random.Range(0, 4);
            this.red = Random.Range(0, 2);
            this.yellow = Random.Range(0, 2);
            this.purple = Random.Range(0, 2);//������ �κ�
        }

        public void UpdateAlramDatas()
        {
            this.green = Random.Range(0, 4);
            this.red = Random.Range(0, 2);
            this.yellow = Random.Range(0, 2);
            this.purple = Random.Range(0, 2);//������ �κ�
        }
        */
        public int GetRedYellow()
        {
            return this.red + this.yellow;
        }

        public int GetGreen()
        {
            return this.green;
        }

        public int GetRed()
        {
            return this.red;
        }

        public int GetYellow()
        {
            return this.yellow;
        }

        public int GetPurple()//������ �κ�
        {
            return this.purple;
        }

        public void ForceAddAlramDatas(ToxinStatus status)
        {
            if (status == ToxinStatus.Green) this.green += 1;
            else if (status == ToxinStatus.Red) this.red += 1;
            else if (status == ToxinStatus.Yellow) this.yellow += 1;
        }

        public void ForceReset()
        {
            this.green = 0;
            this.red = 0;
            this.yellow = 0;
            this.purple = 0;//������ �κ�
        }
    }
}


