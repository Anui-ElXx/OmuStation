using Content.Goobstation.Common.Speech;
using Content.Shared.Speech;
using Robust.Shared.Random;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;

namespace Content.Goobstation.Server.Speech;

/// <summary>
/// This handles...
/// </summary>
public sealed class HorrificAccentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HorrificAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, HorrificAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message;

        message = _replacement.ApplyReplacements(message, "horrific");

        // Prefix
        if (_random.Prob(0.15f))
        {
            var pick = _random.Next(1, 9);
            message = message[0].ToString().ToLower() + message.Remove(0, 1);
            message = Loc.GetString($"accent-horrific-prefix-{pick}") + " " + message;
        }

        // Sanitize capital
        message = message[0].ToString().ToUpper() + message.Remove(0, 1);

        // Suffix
        if (_random.Prob(0.3f))
        {
            var pick = _random.Next(1, 7);
            message += Loc.GetString($"accent-horrific-suffix-{pick}");
        }

        args.Message = message;
    }
}
