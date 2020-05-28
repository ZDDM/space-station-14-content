using Content.Client.BoardGames;
using Content.Shared.GameObjects.Components.BoardGames;
using Robust.Client.GameObjects.Components.UserInterface;

namespace Content.Client.GameObjects.Components.BoardGames
{
    public class LudoBoundUserInterface : BoundUserInterface
    {
        private LudoUI _ludoUI;

        public LudoBoundUserInterface(ClientUserInterfaceComponent owner, object uiKey) : base(owner, uiKey)
        {
            _ludoUI = new LudoUI();
        }
    }
}
