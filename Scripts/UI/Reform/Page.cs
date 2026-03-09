using Assets.Scripts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Page : MonoBehaviour
{
    public virtual UiEventType CallingEventType => UiEventType.PopupError;
    public bool IsShown { get; private set; } = false;
    public virtual void Show(UiEventType from, UiEventType to)
    {
        gameObject.SetActive(true);
        IsShown = true;
    }
    public virtual void Hide(UiEventType from, UiEventType to)
    {
        gameObject.SetActive(false);
        IsShown = false;
    }







}
