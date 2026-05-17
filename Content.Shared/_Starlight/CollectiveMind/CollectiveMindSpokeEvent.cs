using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CollectiveMind;

/// <summary>
/// raised on the entity after it sends a message through a collective mind channel
/// and then used by CollectiveMindSoundSystem to play speech sounds to all channel members
/// </summary>
public sealed class CollectiveMindSpokeEvent : EntityEventArgs
{
    /// <summary> the prototype ID of the collective mind channel the message was sent on</summary>
    public ProtoId<CollectiveMindPrototype> Channel { get; }

    /// <summary> raw message text used to pick ask/exclaim/say sound variants</summary>
    public string Message { get; }

    public CollectiveMindSpokeEvent(ProtoId<CollectiveMindPrototype> channel, string message)
    {
        Channel = channel;
        Message = message;
    }
}
