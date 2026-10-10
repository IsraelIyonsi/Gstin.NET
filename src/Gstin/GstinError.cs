namespace Gstin;

/// <summary>Why a GSTIN was rejected.</summary>
public enum GstinError
{
    /// <summary>The value is null.</summary>
    Null,

    /// <summary>The value is not 15 characters long.</summary>
    InvalidLength,

    /// <summary>A character is outside 0-9 and uppercase A-Z, or the state/PAN layout is violated.</summary>
    InvalidFormat,

    /// <summary>The state code is outside 01 to 38.</summary>
    InvalidStateCode,

    /// <summary>The entity number at position 13 is 0; real registrations run 1-9 then A-Z.</summary>
    InvalidEntityNumber,

    /// <summary>Position 14 is not the literal Z.</summary>
    InvalidDefaultChar,

    /// <summary>The check character does not match.</summary>
    InvalidChecksum,
}
