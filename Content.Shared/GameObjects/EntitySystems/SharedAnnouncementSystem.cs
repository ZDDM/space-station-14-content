using System;
using Robust.Shared.GameObjects;
using Robust.Shared.GameObjects.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.GameObjects.EntitySystems
{
    public class SharedAnnouncementSystem : EntitySystem
    {

    }

    [NetSerializable, Serializable]
    public class AnnouncementMessage : EntitySystemMessage
    {
        public string Header { get; }
        public string Text { get; }

        public AnnouncementMessage(string header, string text)
        {
            Header = header;
            Text = text;
        }
    }
}
