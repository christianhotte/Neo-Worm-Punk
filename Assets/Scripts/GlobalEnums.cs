using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomEnums
{
    /// <summary>
    /// Indicates association (or lack thereof) with a certain hand/side.
    /// </summary>
    public enum Handedness { Left, Right, None }
    /// <summary>
    /// Used to differentiate targeting settings between targets with different priorities and properties.
    /// </summary>
    public enum TargetType { Player, Interactable, Generic }
}
