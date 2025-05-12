using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VoiceControls.Main;
using VoiceControls.Tools;

namespace VoiceControls.Components
{
    public class HeadTouchable : MonoBehaviour
    {
        public void OnTriggerEntered(Collider collider)
        {
            GorillaTriggerColliderHandIndicator GTCH = collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
            if (GTCH == null) return;
            Vars.StarterRecognised?.Invoke(CommandInfo.CommandType.Default);
            Vars.Default.Stop();
            Vars.DefaultCommand.Start();
        }
    }
}
