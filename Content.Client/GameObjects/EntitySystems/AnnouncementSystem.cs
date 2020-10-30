using Content.Shared.GameObjects.EntitySystems;
using JetBrains.Annotations;
using Robust.Client.Interfaces.UserInterface;
using Robust.Shared.IoC;

namespace Content.Client.GameObjects.EntitySystems
{
    [UsedImplicitly]
    public class AnnouncementSystem : SharedAnnouncementSystem
    {
        [Dependency] private IUserInterfaceManager _uiManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeNetworkEvent<AnnouncementMessage>(AnnouncementHandler);
        }

        public override void Shutdown()
        {
            base.Shutdown();

            UnsubscribeNetworkEvent<AnnouncementMessage>();
        }

        public void AnnouncementHandler(AnnouncementMessage message)
        {
            _uiManager.Popup(message.Text, message.Header);
        }
    }
}
