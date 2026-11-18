using System;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("Custom/Hair Transparent")]
public class HairTransparentVolume : VolumeComponent
{
    public BoolParameter enable = new BoolParameter(true);

    public bool IsActive() => enable.value;

    public bool IsTileCompatible() => false;
}