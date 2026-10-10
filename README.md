# Gstin.Net

Zero-dependency validator and decoder for India's 15-character GSTIN (Goods and Services Tax Identification Number), the tax ID printed on every Indian B2B invoice.

[![NuGet](https://img.shields.io/nuget/v/Gstin.Net.svg)](https://www.nuget.org/packages/Gstin.Net) &nbsp; MIT &nbsp; Zero dependencies &nbsp; Native AOT clean

## Why

If you invoice, onboard or reconcile with Indian businesses you need to know whether a GSTIN is well formed before you hit the GST portal. The format and check character are public, but there was no small .NET library for it. `Gstin.Net` is that primitive.

## Install

```
dotnet add package Gstin.Net
```

## Use

```csharp
using Gstin;

bool ok = GstinNumber.IsValid("27AAPFU0939F1ZV");   // true

GstinInfo info = GstinNumber.Parse("27AAPFU0939F1ZV");
// info.StateCode    "27"
// info.Pan          "AAPFU0939F"
// info.EntityNumber '1'
// info.CheckChar    'V'

if (!GstinNumber.TryValidate(input, out GstinError? error))
{
    // error: Null, InvalidLength, InvalidFormat, InvalidStateCode, InvalidDefaultChar, InvalidChecksum
}

GstinNumber.Validate(input);                        // throws GstinException (with .Reason)
bool parsed = GstinNumber.TryParse(input, out var decoded);
```

## Format

| Positions | Meaning |
|---|---|
| 1-2 | State code, `01` to `38` |
| 3-12 | PAN of the registrant, `AAAAA9999A` |
| 13 | Entity number: registrations under the same PAN in the state, `1-9` then `A-Z` |
| 14 | Literal `Z` |
| 15 | Check character |

## Check character

Mod-36 over the first 14 characters. Map `0-9` to 0-9 and `A-Z` to 10-35, multiply by a factor alternating `1, 2, 1, 2, ...` starting at 1 for position 1, add `product / 36 + product % 36` for each character, and the check value is `(36 - sum % 36) % 36`. `GstinNumber.ComputeCheckChar` exposes it.

## Strictness

Input is never normalised. Lowercase, whitespace and hyphens are rejected, not repaired, so a malformed value is never silently accepted. Trim and upper-case at your own boundary if you want that behaviour.

State names are intentionally not bundled; only the numeric state code is decoded.

## Credits

Algorithm and test approach follow the `in.gst` module of [python-stdnum](https://arthurdejong.org/python-stdnum/) (LGPL), reimplemented independently here.

## License

MIT
