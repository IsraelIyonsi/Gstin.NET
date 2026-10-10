namespace Gstin;

/// <summary>The decoded components of a valid GSTIN.</summary>
/// <param name="Value">The full 15-character GSTIN.</param>
/// <param name="StateCode">Two-digit state or union territory code (positions 1-2), 01 to 38.</param>
/// <param name="Pan">The 10-character PAN of the registrant (positions 3-12).</param>
/// <param name="EntityNumber">Registration number under the same PAN within the state (position 13), 1-9 then A-Z.</param>
/// <param name="CheckChar">The mod-36 check character (position 15).</param>
public readonly record struct GstinInfo(string Value, string StateCode, string Pan, char EntityNumber, char CheckChar);
