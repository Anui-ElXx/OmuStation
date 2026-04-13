using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Server.Chat.Systems;
using Content.Omu.Server.EmoteOnFinalDamage.Components;

namespace Content.Omu.Server.EmoteOnFinalDamage.Systems;

public sealed class EmoteOnFinalDamageSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // DamageChangedEvent fires after resistances/modifiers are applied,
        // so args.Damageable.TotalDamage reflects the true final value.
        SubscribeLocalEvent<EmoteOnFinalDamageComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<EmoteOnFinalDamageComponent> ent, ref DamageChangedEvent args)
    {
        // Only care about damage increases, not healing.
        if (!args.DamageIncreased)
            return;

        // DeltaDamage is the net change this event; it reflects post-armor final damage.
        var delta = args.DamageDelta;
        if (delta == null)
            return;

        var totalDelta = delta.GetTotal();
        if (totalDelta < ent.Comp.DamageThreshold)
            return;

        if (ent.Comp.LastEmoteTime + ent.Comp.EmoteCooldown > _gameTiming.CurTime)
            return;

        if (ent.Comp.Emotes.Count == 0)
            return;

        if (!_random.Prob(ent.Comp.EmoteChance))
            return;

        var emote = _random.Pick(ent.Comp.Emotes);
        if (ent.Comp.WithChat)
            _chatSystem.TryEmoteWithChat(ent, emote, ent.Comp.HiddenFromChatWindow ? ChatTransmitRange.HideChat : ChatTransmitRange.Normal);
        else
            _chatSystem.TryEmoteWithoutChat(ent, emote, voluntary: false);

        ent.Comp.LastEmoteTime = _gameTiming.CurTime;
    }

    /// <summary>
    /// Adds an emote to the entity's pool. Returns false if the component is missing or the emote already exists.
    /// </summary>
    public bool AddEmote(EntityUid uid, string emotePrototypeId, EmoteOnFinalDamageComponent? comp = null)
    {
        if (!Resolve(uid, ref comp, logMissing: false))
            return false;

        DebugTools.Assert(comp.LifeStage <= ComponentLifeStage.Running);
        DebugTools.Assert(_prototypeManager.HasIndex<EmotePrototype>(emotePrototypeId), "Prototype not found. Did you make a typo?");

        return comp.Emotes.Add(emotePrototypeId);
    }

    /// <summary>
    /// Removes an emote from the entity's pool. By default, removes the component when the pool is empty.
    /// </summary>
    public bool RemoveEmote(EntityUid uid, string emotePrototypeId, EmoteOnFinalDamageComponent? comp = null, bool removeEmpty = true)
    {
        if (!Resolve(uid, ref comp, logMissing: false))
            return false;

        DebugTools.Assert(_prototypeManager.HasIndex<EmotePrototype>(emotePrototypeId), "Prototype not found. Did you make a typo?");

        if (!comp.Emotes.Remove(emotePrototypeId))
            return false;

        if (removeEmpty && comp.Emotes.Count == 0)
            RemCompDeferred(uid, comp);

        return true;
    }
}
