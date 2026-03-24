namespace Streak.Core.Services.Validators;

public static class EmojiValidationHelper
{
    public static bool IsEmptyOrSingleEmoji(string? value)
    {
        var normalizedValue = NormalizeOptionalText(value);
        if (normalizedValue is null)
            return true;

        if (StringInfo.ParseCombiningCharacters(normalizedValue).Length != 1)
            return false;

        var runes = normalizedValue.EnumerateRunes().ToArray();
        var hasOtherSymbol = runes.Any(rune => Rune.GetUnicodeCategory(rune) == UnicodeCategory.OtherSymbol);
        var hasEmojiPresentationControls = runes.Any(rune => rune.Value is 0xFE0F or 0x20E3 or 0x200D);
        var hasKeycapBase = runes.Any(IsKeycapBase);
        var isFlagEmoji = runes.Length == 2 && runes.All(IsRegionalIndicator);

        return hasOtherSymbol || isFlagEmoji || (hasEmojiPresentationControls && hasKeycapBase);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsRegionalIndicator(Rune rune)
    {
        return rune.Value is >= 0x1F1E6 and <= 0x1F1FF;
    }

    private static bool IsKeycapBase(Rune rune)
    {
        return rune.Value is >= '0' and <= '9' or '#' or '*';
    }
}