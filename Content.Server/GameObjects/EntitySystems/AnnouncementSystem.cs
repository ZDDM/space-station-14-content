using Content.Shared.GameObjects.EntitySystems;
using JetBrains.Annotations;
using Robust.Server.Interfaces.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;

namespace Content.Server.GameObjects.EntitySystems
{
    [UsedImplicitly]
    public class AnnouncementSystem : SharedAnnouncementSystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;

        public const int AnnouncementHeaderCharacterLimit = 64;
        public const int AnnouncementCharacterLimit = 256;

        /// <summary>
        ///     Sends an announcements to players on a grid, or all players.
        /// </summary>
        /// <param name="header">Announcement header. Limited to <see cref="AnnouncementHeaderCharacterLimit"/> characters.</param>
        /// <param name="text">Announcement text. Limited to <see cref="AnnouncementCharacterLimit"/> characters.</param>
        /// <param name="grid">Grid where the announcement will be sent to, or invalid grid to send this to everyone.</param>
        public void Announce(string header, string text, GridId grid = default)
        {
            if (header.Length > AnnouncementHeaderCharacterLimit)
                header = header.Substring(0, AnnouncementHeaderCharacterLimit);

            if (text.Length > AnnouncementCharacterLimit)
                text = text.Substring(0, AnnouncementCharacterLimit);

            var message = new AnnouncementMessage(header, text);

            if (!grid.IsValid())
            {
                RaiseNetworkEvent(message);
                return;
            }

            foreach (var player in _playerManager.GetPlayersBy((player) => player.AttachedEntity?.Transform.GridID == grid))
            {
                RaiseNetworkEvent(message, player.ConnectedClient);
            }
        }
    }
}
