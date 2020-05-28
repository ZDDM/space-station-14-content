using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.GameObjects.Components.BoardGames
{
    public class SharedLudoComponent : Component
    {
        public override string Name => "Ludo";
    }

    [Serializable, NetSerializable]
    public enum LudoUiKey
    {
        Key
    }

    [Serializable, NetSerializable]
    public class LudoState : ComponentState
    {
        public LudoState() : base(ContentNetIDs.LUDO)
        {
        }
    }
}
