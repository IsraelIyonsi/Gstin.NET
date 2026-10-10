namespace Gstin.Tests;

public class GstinNumberTests
{
    public static TheoryData<string, string, string, char, char> ValidVectors => new()
    {
        { "27AAPFU0939F1ZV", "27", "AAPFU0939F", '1', 'V' },
        { "29AAGCB7383J1Z4", "29", "AAGCB7383J", '1', '4' },
        { "33AAACH7409R1Z8", "33", "AAACH7409R", '1', '8' },
        { "27AAACR5055K1Z7", "27", "AAACR5055K", '1', '7' },
        { "09AAACH7409R1ZZ", "09", "AAACH7409R", '1', 'Z' },
        { "07AABCT1332L1ZG", "07", "AABCT1332L", '1', 'G' },
        { "01ABCDE1234F1ZE", "01", "ABCDE1234F", '1', 'E' },
        { "37ABCDE1234F1ZZ", "37", "ABCDE1234F", '1', 'Z' },
        { "38ABCDE1234F1ZX", "38", "ABCDE1234F", '1', 'X' },
        { "27ABCDE1234F1Z0", "27", "ABCDE1234F", '1', '0' },
        { "29AABCT1332LBZ0", "29", "AABCT1332L", 'B', '0' },
        { "29AAICA3918J2ZD", "29", "AAICA3918J", '2', 'D' },
    };

    public static TheoryData<string?, GstinError> InvalidVectors => new()
    {
        { null, GstinError.Null },
        { "", GstinError.InvalidLength },
        { "27AAPFU0939F1Z", GstinError.InvalidLength },
        { "27AAPFU0939F1ZVV", GstinError.InvalidLength },
        { "27aapfu0939f1zv", GstinError.InvalidFormat },
        { "27AAPFU0939F1Zv", GstinError.InvalidFormat },
        { "27AAPFU0939f1ZV", GstinError.InvalidFormat },
        { "27AAPFU0939F1Z ", GstinError.InvalidFormat },
        { "27AAPFU0939F1Z-", GstinError.InvalidFormat },
        { "A7AAPFU0939F1ZV", GstinError.InvalidFormat },
        { "2AAAPFU0939F1ZV", GstinError.InvalidFormat },
        { "00AAPFU0939F1ZV", GstinError.InvalidStateCode },
        { "39AAPFU0939F1ZV", GstinError.InvalidStateCode },
        { "99AAPFU0939F1ZV", GstinError.InvalidStateCode },
        { "27AAPF10939F1ZV", GstinError.InvalidFormat },
        { "27AAPFUA939F1ZV", GstinError.InvalidFormat },
        { "27AAPFU09391" + "1ZV", GstinError.InvalidFormat },
        { "27AAPFU0939F1AV", GstinError.InvalidDefaultChar },
        { "27AAPFU0939F10V", GstinError.InvalidDefaultChar },
        { "27AAPFU0939F1ZW", GstinError.InvalidChecksum },
        { "27AAPFU0939F1Z0", GstinError.InvalidChecksum },
        { "27ABCDE1234F1ZZ", GstinError.InvalidChecksum },
        { "29AABCT1332LBZ1", GstinError.InvalidChecksum },
    };

    [Theory]
    [MemberData(nameof(ValidVectors))]
    public void Parse_decodes_valid_vector(string gstin, string state, string pan, char entity, char check)
    {
        var info = GstinNumber.Parse(gstin);

        Assert.Equal(new GstinInfo(gstin, state, pan, entity, check), info);
        Assert.True(GstinNumber.IsValid(gstin));
        GstinNumber.Validate(gstin);
        Assert.True(GstinNumber.TryValidate(gstin, out var error));
        Assert.Null(error);
        Assert.True(GstinNumber.TryParse(gstin, out var tried));
        Assert.Equal(info, tried);
    }

    [Theory]
    [MemberData(nameof(ValidVectors))]
    public void ComputeCheckChar_matches_position_15(string gstin, string state, string pan, char entity, char check)
    {
        Assert.Equal(check, GstinNumber.ComputeCheckChar(gstin[..14]));
        Assert.Equal(gstin, state + pan + entity + "Z" + check);
    }

    [Fact]
    public void Entity_zero_is_rejected_even_when_checksum_is_valid()
    {
        const string payload = "27AAPFU0939F0Z";
        var gstin = payload + GstinNumber.ComputeCheckChar(payload);

        Assert.False(GstinNumber.IsValid(gstin));
        Assert.False(GstinNumber.TryValidate(gstin, out var error));
        Assert.Equal(GstinError.InvalidEntityNumber, error);
    }

    [Theory]
    [MemberData(nameof(InvalidVectors))]
    public void Invalid_vector_is_rejected_with_reason(string? gstin, GstinError expected)
    {
        Assert.False(GstinNumber.IsValid(gstin));
        Assert.False(GstinNumber.TryValidate(gstin, out var error));
        Assert.Equal(expected, error);
        Assert.False(GstinNumber.TryParse(gstin, out var info));
        Assert.Equal(default, info);

        var ex = Assert.Throws<GstinException>(() => GstinNumber.Validate(gstin));
        Assert.Equal(expected, ex.Reason);
        Assert.Throws<GstinException>(() => GstinNumber.Parse(gstin));
    }

    [Fact]
    public void Lowercase_is_never_silently_uppercased()
    {
        Assert.True(GstinNumber.IsValid("27AAPFU0939F1ZV"));
        Assert.False(GstinNumber.IsValid("27AAPFU0939F1ZV".ToLowerInvariant()));
    }

    [Fact]
    public void Zero_remainder_wraps_to_check_char_zero_not_36()
    {
        Assert.Equal('0', GstinNumber.ComputeCheckChar("27ABCDE1234F1Z"));
        Assert.Equal('0', GstinNumber.ComputeCheckChar("29AABCT1332LBZ"));
    }

    [Fact]
    public void Every_state_code_01_to_38_is_accepted_and_others_rejected()
    {
        for (var s = 0; s <= 99; s++)
        {
            var payload = s.ToString("D2", System.Globalization.CultureInfo.InvariantCulture) + "ABCDE1234F1Z";
            var gstin = payload + GstinNumber.ComputeCheckChar(payload);
            Assert.Equal(s is >= 1 and <= 38, GstinNumber.IsValid(gstin));
        }
    }

    [Fact]
    public void Mutation_flipping_any_single_payload_char_invalidates()
    {
        const string valid = "27AAPFU0939F1ZV";
        const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (var i = 0; i < valid.Length; i++)
        {
            foreach (var c in alphabet)
            {
                if (c == valid[i])
                {
                    continue;
                }

                var mutated = valid.Remove(i, 1).Insert(i, c.ToString());
                Assert.False(GstinNumber.IsValid(mutated), mutated);
            }
        }
    }

    [Fact]
    public void Mutation_flipped_pan_char_changes_expected_check_char()
    {
        Assert.NotEqual('V', GstinNumber.ComputeCheckChar("27AAPFU0939G1Z"));
        Assert.False(GstinNumber.IsValid("27AAPFU0939G1ZV"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("27AAPFU0939F1ZV")]
    [InlineData("27AAPFU0939f1Z")]
    public void ComputeCheckChar_rejects_bad_payload(string? payload)
    {
        if (payload is null)
        {
            Assert.Throws<ArgumentNullException>(() => GstinNumber.ComputeCheckChar(payload!));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => GstinNumber.ComputeCheckChar(payload));
        }
    }

    [Fact]
    public void Exception_message_names_reason()
    {
        var ex = Assert.Throws<GstinException>(() => GstinNumber.Validate("27AAPFU0939F1ZW"));
        Assert.Equal("Invalid GSTIN: InvalidChecksum.", ex.Message);
    }
}
