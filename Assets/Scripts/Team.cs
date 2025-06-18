using UnityEngine;

/// <summary>
/// Represents a team. Attach to an empty GameObject for each team.
/// </summary>
public class Team : MonoBehaviour
{
    public string TeamName;
    public Color TeamColor = Color.white;

    public override string ToString() => TeamName;
}