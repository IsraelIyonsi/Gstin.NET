namespace Gstin;

/// <summary>Validates and decodes India's 15-character GSTIN. Input is never normalised: lowercase and whitespace are rejected.</summary>
public static class GstinNumber
{
    private const int Length = 15;
    private const int StateCodeLength = 2;
    private const int PanStart = 2;
    private const int PanLength = 10;
    private const int PanLetterPrefix = 5;
    private const int PanDigitRun = 4;
    private const int EntityIndex = 12;
    private const int DefaultCharIndex = 13;
    private const int CheckIndex = 14;
    private const int PayloadLength = 14;
    private const int MinStateCode = 1;
    private const int MaxStateCode = 38;
    private const int DecimalBase = 10;
    private const int Radix = 36;
    private const int AlternateFactor = 2;
    private const int InitialFactor = 1;
    private const char DefaultChar = 'Z';
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Returns true if the value is a valid GSTIN.</summary>
    /// <param name="value">The candidate GSTIN.</param>
    /// <returns>True when valid.</returns>
    public static bool IsValid(string? value) => Check(value) is null;

    /// <summary>Throws <see cref="GstinException"/> if the value is not a valid GSTIN.</summary>
    /// <param name="value">The candidate GSTIN.</param>
    /// <exception cref="GstinException">The value is invalid.</exception>
    public static void Validate(string? value)
    {
        if (Check(value) is { } error)
        {
            throw new GstinException(error);
        }
    }

    /// <summary>Validates the value and reports the failure reason without throwing.</summary>
    /// <param name="value">The candidate GSTIN.</param>
    /// <param name="error">The failure reason when invalid.</param>
    /// <returns>True when valid.</returns>
    public static bool TryValidate(string? value, out GstinError? error)
    {
        error = Check(value);
        return error is null;
    }

    /// <summary>Validates and decodes a GSTIN.</summary>
    /// <param name="value">The GSTIN.</param>
    /// <returns>The decoded components.</returns>
    /// <exception cref="GstinException">The value is invalid.</exception>
    public static GstinInfo Parse(string? value)
    {
        Validate(value);
        return Decode(value!);
    }

    /// <summary>Validates and decodes a GSTIN without throwing.</summary>
    /// <param name="value">The GSTIN.</param>
    /// <param name="info">The decoded components when valid, otherwise default.</param>
    /// <returns>True when valid.</returns>
    public static bool TryParse(string? value, out GstinInfo info)
    {
        if (Check(value) is null)
        {
            info = Decode(value!);
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>Computes the check character for the first 14 characters of a GSTIN.</summary>
    /// <param name="payload">Exactly 14 characters of 0-9 and A-Z.</param>
    /// <returns>The mod-36 check character.</returns>
    /// <exception cref="ArgumentException">The payload is not 14 characters of 0-9 and A-Z.</exception>
    public static char ComputeCheckChar(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (payload.Length != PayloadLength || !IsAlphanumeric(payload))
        {
            throw new ArgumentException("Payload must be 14 characters of 0-9 and A-Z.", nameof(payload));
        }

        return Checksum(payload);
    }

    private static GstinInfo Decode(string v) =>
        new(v, v[..StateCodeLength], v.Substring(PanStart, PanLength), v[EntityIndex], v[CheckIndex]);

    private static GstinError? Check(string? value)
    {
        if (value is null)
        {
            return GstinError.Null;
        }

        if (value.Length != Length)
        {
            return GstinError.InvalidLength;
        }

        if (!IsAlphanumeric(value) || !IsDigit(value[0]) || !IsDigit(value[1]))
        {
            return GstinError.InvalidFormat;
        }

        var state = ((value[0] - '0') * DecimalBase) + (value[1] - '0');
        if (state < MinStateCode || state > MaxStateCode)
        {
            return GstinError.InvalidStateCode;
        }

        if (!IsPan(value.AsSpan(PanStart, PanLength)))
        {
            return GstinError.InvalidFormat;
        }

        if (value[EntityIndex] == '0')
        {
            return GstinError.InvalidEntityNumber;
        }

        if (value[DefaultCharIndex] != DefaultChar)
        {
            return GstinError.InvalidDefaultChar;
        }

        return Checksum(value) == value[CheckIndex] ? null : GstinError.InvalidChecksum;
    }

    private static bool IsPan(ReadOnlySpan<char> pan)
    {
        for (var i = 0; i < pan.Length; i++)
        {
            var digitSlot = i >= PanLetterPrefix && i < PanLetterPrefix + PanDigitRun;
            if (digitSlot ? !IsDigit(pan[i]) : !IsUpper(pan[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static char Checksum(string s)
    {
        var sum = 0;
        var factor = InitialFactor;
        for (var i = 0; i < PayloadLength; i++)
        {
            var product = Alphabet.IndexOf(s[i]) * factor;
            sum += (product / Radix) + (product % Radix);
            factor = factor == InitialFactor ? AlternateFactor : InitialFactor;
        }

        return Alphabet[(Radix - (sum % Radix)) % Radix];
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsUpper(char c) => c is >= 'A' and <= 'Z';

    private static bool IsAlphanumeric(string s)
    {
        foreach (var c in s)
        {
            if (!IsDigit(c) && !IsUpper(c))
            {
                return false;
            }
        }

        return true;
    }
}
