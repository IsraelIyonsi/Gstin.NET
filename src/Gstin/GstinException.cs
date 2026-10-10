namespace Gstin;

/// <summary>Thrown when a GSTIN fails validation.</summary>
public sealed class GstinException : FormatException
{
    /// <summary>Creates the exception with the reason the GSTIN was rejected.</summary>
    /// <param name="reason">The failure reason.</param>
    public GstinException(GstinError reason)
        : base($"Invalid GSTIN: {reason}.")
    {
        Reason = reason;
    }

    /// <summary>The reason the GSTIN was rejected.</summary>
    public GstinError Reason { get; }
}
