namespace SAM.API.Types;

public readonly struct SteamInventoryResult : IEquatable<SteamInventoryResult>, IComparable<SteamInventoryResult>
{
    // Name: SteamInventoryResult, Type: int
    public int Value { get; init; }

    public static implicit operator SteamInventoryResult(int value) => new SteamInventoryResult() { Value = value };
    public static implicit operator int(SteamInventoryResult value) => value.Value;
    public override string ToString() => Value.ToString();
    public override int GetHashCode() => Value.GetHashCode();
    public override bool Equals(object? p)
    {
        if (p == null)
        {
            return false;
        }
        else if (p is SteamInventoryResult p2)
        {
            return Equals(p2);
        }
        else if (p is int i)
        {
            return i == Value;
        }
        return false;
    }

    public bool Equals(SteamInventoryResult p) => p.Value == Value;
    public static bool operator ==(SteamInventoryResult a, SteamInventoryResult b) => a.Equals(b);
    public static bool operator !=(SteamInventoryResult a, SteamInventoryResult b) => !a.Equals(b);
    public int CompareTo(SteamInventoryResult other) => Value.CompareTo(other.Value);
}
