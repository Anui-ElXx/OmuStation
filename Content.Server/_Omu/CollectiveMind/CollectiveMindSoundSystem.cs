using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Ghost;
using Content.Shared.Speech;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
 using Content.Shared._Omu.CollectiveMind;

namespace Content.Server._Omu.CollectiveMind;

/// <summary>
///    Plays speech sounds when an entity sends a collective mind message.
///    These are sent only to entities that share the same channel (collective mind)
///    Ghosts get skipped entirely to prevent them from hearing speech sounds from collective minds.
/// </summary>
public sealed class CollectiveMindSoundSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CollectiveMindComponent, CollectiveMindSpokeEvent>(OnCollectiveMindSpoke);
    }

    private void OnCollectiveMindSpoke(EntityUid uid, CollectiveMindComponent component, CollectiveMindSpokeEvent args)
    {
        if (!_protoManager.TryIndex<CollectiveMindPrototype>(args.Channel, out var channelProto))
            return;

        var soundsId = component.SpeechSounds ?? channelProto.SpeechSounds;
        if (soundsId == null)
            return;

        if (!_protoManager.TryIndex<SpeechSoundsPrototype>(soundsId, out var soundProto))
            return;

        var message = args.Message;
        SoundSpecifier sound;

        if (message.Length > 0)
        {
            int uppercaseCount = 0;
            foreach (var c in message)
            {
                if (char.IsUpper(c))
                    uppercaseCount++;
            }

            sound = uppercaseCount > message.Length / 2
                ? soundProto.ExclaimSound
                : message[^1] switch
                {
                    '?' => soundProto.AskSound,
                    '!' => soundProto.ExclaimSound,
                    _ => soundProto.SaySound,
                };
        }
        else
        {
            sound = soundProto.SaySound;
        }

        // applies pitch variation - parameters can be set in YAML
        var variation = component.SpeechSoundsVariation ?? soundProto.Variation;
        var scale = (float) _random.NextGaussian(1, variation);
        var audioParams = AudioParams.Default.WithPitchScale(scale);

        // builds a filter containing only players who can hear the channel
        var filter = Filter.Empty();
        var query = EntityQueryEnumerator<CollectiveMindComponent, ActorComponent>();
        while (query.MoveNext(out var listenerUid, out var listenerMind, out var actor))
        {
            // ghosts get skipped entirely
            if (HasComp<GhostComponent>(listenerUid))
                continue;

            bool canHear = listenerMind.HearAll || listenerMind.Channels.Contains(args.Channel);
            if (canHear)
                filter.AddPlayer(actor.PlayerSession);
        }

        // heard at full volume regardless of map location
        _audio.PlayGlobal(sound, filter, false, audioParams);
    }
}
