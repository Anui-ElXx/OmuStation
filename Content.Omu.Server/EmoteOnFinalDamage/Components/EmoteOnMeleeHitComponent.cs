using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Content.Omu.Server.EmoteOnFinalDamage.Systems;

namespace Content.Omu.Server.EmoteOnFinalDamage.Components;

/// <summary>
/// causes it to emote when it receives final (post-armor) damage at or above threshold.
/// </summary>
[RegisterComponent, Access(typeof(EmoteOnFinalDamageSystem)), AutoGenerateComponentPause]
public sealed partial class EmoteOnFinalDamageComponent : Component
{
    /// <summary>
    /// Minimum total damage (after armor) in a single hit required to trigger an emote.
    /// </summary>
    [DataField]
    public float DamageThreshold = 12f;

    /// <summary>
    /// Chance of performing an emote when the threshold is met and the cooldown has expired.
    /// </summary>
    [DataField]
    public float EmoteChance = 0.5f;

    /// <summary>
    /// A set of emotes that will be randomly picked from.
    /// <see cref="EmotePrototype"/>
    /// </summary>
    [DataField("emotes", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EmotePrototype>))]
    public HashSet<string> Emotes = new();

    /// <summary>
    /// Also send the emote in chat.
    /// </summary>
    [DataField]
    public bool WithChat = true;

    /// <summary>
    /// Hide the chat message from the chat window, only showing the popup.
    /// This does nothing if WithChat is false.
    /// </summary>
    [DataField]
    public bool HiddenFromChatWindow;

    /// <summary>
    /// The simulation time of the last emote performed due to damage.
    /// </summary>
    [DataField("lastEmoteTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan LastEmoteTime = TimeSpan.Zero;

    /// <summary>
    /// Minimum time between emotes.
    /// </summary>
    [DataField]
    public TimeSpan EmoteCooldown = TimeSpan.FromSeconds(1);
}
