using Onthesys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMP;
using UnityEngine;


public class ObsCctv : MonoBehaviour 
{
    ModelProvider modelProvider => UiManager.Instance.modelProvider;
    UniversalMediaPlayer umpVideoEquipment, umpVideoOutdoor;

    private void Awake()
    {
        umpVideoEquipment = transform.Find("Video_Player A").GetComponentInChildren<UniversalMediaPlayer>();
        umpVideoOutdoor = transform.Find("Video_Player B").GetComponentInChildren<UniversalMediaPlayer>();
    }

    private void Start()
    {
        
    }


    public void OnNavigateObs(object obj)
    {
        if (obj is not int obsId) return;

        ObsData obs = modelProvider.GetObs(obsId);

        umpVideoEquipment.Path = obs.src_video1;
        umpVideoEquipment.Prepare();
        umpVideoOutdoor.Path = obs.src_video2;
        umpVideoOutdoor.Prepare();
    }








}