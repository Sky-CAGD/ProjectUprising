using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HighlightType
{
    none,
    validPath,
    invalidPath,
    moveArea,
    unitSelection,
    attackArea,
    attackTarget
}

public class HighlightColors : SingletonPattern<HighlightColors>
{
    [field: SerializeField] public Color MoveAreaColor { get; private set; }
    [field: SerializeField] public Color ValidPathColor { get; private set; }
    [field: SerializeField] public Color InvalidPathColor { get; private set; }
    [field: SerializeField] public Color UnitSelectionColor { get; private set; }
    [field: SerializeField] public Color AttackAreaColor { get; private set; }
    [field: SerializeField] public Color AttackTargetColor { get; private set; }
}
