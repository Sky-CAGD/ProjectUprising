using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightColors : SingletonPattern<HighlightColors>
{
    [field: SerializeField] public Color MoveAreaColor { get; private set; }
    [field: SerializeField] public Color ValidPathColor { get; private set; }
    [field: SerializeField] public Color InvalidPathColor { get; private set; }
    [field: SerializeField] public Color UnitSelectionColor { get; private set; }
}
