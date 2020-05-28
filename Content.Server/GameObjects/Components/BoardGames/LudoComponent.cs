using Content.Shared.GameObjects.Components.BoardGames;
using Robust.Server.Interfaces.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;

namespace Content.Server.GameObjects.Components.BoardGames
{
    [RegisterComponent]
    public class LudoComponent : SharedLudoComponent
    {
        /// <summary>
        ///     Number of squares the board has.
        /// </summary>
        public const int BoardSize = 68;

        /// <summary>
        ///     Squares before reaching the goal.
        /// </summary>
        public const int BoardFinishSize = 7;

        /// <summary>
        ///     Squares between the different starting squares.
        /// </summary>
        public const int StartDistance = 17;

        public const int StartingSquareOffset = 5;

        public const int TokenCount = 4;
        public const int MaxTokensInSquare = 2;
        public int TotalTokenCount => TokenCount * PlayerCount;

        public int PlayerCount { get; private set; }

        private readonly LudoPlayer[] _players = new LudoPlayer[4];

        private readonly int[] _safeSpots = {5, 12, 17, 22, 29, 34, 39, 46, 51, 56, 63, 68};

        public enum PlayerColor : byte
        {
            Yellow = 0,
            Blue = 1,
            Red = 2,
            Green = 3
        }

        public class LudoPlayer
        {
            public IPlayerSession Player { get; set; }
            public readonly LudoToken[] Tokens = new LudoToken[TokenCount];
            public PlayerColor Color { get; }
            public int StartingSquare => ((int) Color * StartDistance) + StartingSquareOffset;
            public int FinishSquare => (((int) Color * StartDistance) + BoardSize) % BoardSize + (Color != PlayerColor.Yellow ? 1 : 0);

            public LudoPlayer(PlayerColor color)
            {
                Color = color;

                Logger.Info($"PLAYER COLOR: {Color} || STARTING SQUARE: {StartingSquare} || FINISH SQUARE: {FinishSquare}");

                for (var i = 0; i < TokenCount; i++)
                {
                    Tokens[i] = new LudoToken(Color);
                }
            }
        }

        public class LudoToken
        {
            public int Position { get; set; } = 0;
            public int FinishPosition { get; set; } = 0;
            public PlayerColor Color { get; }

            public bool IsAtHome => Position == 0 && (FinishPosition == 0);
            public bool IsInFinishLane => FinishPosition != 0;
            public bool HasFinished => FinishPosition == BoardFinishSize + 1;

            public LudoToken(PlayerColor color)
            {
                Color = color;
            }
        }

        public bool IsPlaying(IPlayerSession player)
        {
            for (var i = 0; i < 4; i++)
            {
                if (_players[i]?.Player == player)
                    return true;
            }

            return false;
        }

        public LudoPlayer GetPlayer(IPlayerSession player)
        {
            for (var i = 0; i < PlayerCount; i++)
            {
                if (_players[i]?.Player == player)
                    return _players[i];
            }

            return null;
        }

        public LudoPlayer GetPlayerByColor(PlayerColor color)
        {
            for (var index = 0; index < PlayerCount; index++)
            {
                var player = _players[index];
                if (player.Color == color)
                    return player;
            }

            return null;
        }

        public LudoToken[] GetAllTokens()
        {
            var tokens = new LudoToken[TotalTokenCount];

            for (var i = 0; i < PlayerCount; i++)
            {
                var player = _players[i];

                for (var j = 0; j < TokenCount; j++)
                {
                    tokens[(TokenCount*i)+j] = player.Tokens[j];
                }
            }

            return tokens;
        }

        public LudoToken[] GetAllTokensAt(int position)
        {
            var tokens = GetAllTokens();
            var tokensAtPos = new LudoToken[MaxTokensInSquare];

            var count = 0;

            for (var i = 0; i < TotalTokenCount; i++)
            {
                var token = tokens[i];
                if (token.Position != position) continue;
                tokensAtPos[count++] = token;

                if (count == MaxTokensInSquare)
                    break;
            }

            return tokensAtPos;
        }

        public bool IsSafeSquare(int position)
        {
            foreach (var safeSpot in _safeSpots)
            {
                if (position == safeSpot)
                    return true;
            }

            return false;
        }

        public bool HasBlockade(int position)
        {
            return HasBlockade(position, out _);
        }

        public bool HasBlockade(int position, out PlayerColor? blockadeColor)
        {
            var tokens = GetAllTokensAt(position);

            blockadeColor = null;

            for (var i = 0; i < MaxTokensInSquare; i++)
            {
                var token = tokens[i];

                if (token == null)
                    return false;

                if (blockadeColor == null)
                    blockadeColor = token.Color;

                else if (blockadeColor != token.Color)
                    return false;
            }

            return true;
        }

        public bool Move(LudoToken token, int squares)
        {
            if (token.IsAtHome || token.IsInFinishLane)
                return false;

            if (token.IsInFinishLane)
                return MoveInsideFinishLane(token, squares);

            if (!CanMove(token, squares))
                return false;

            if (WouldMoveIntoFinishLane(token, squares))
                return MoveIntoFinishLane(token, squares);

            var newPos = GetNextSquare(token.Position, squares);

            TryCapture(newPos, token.Color);

            token.Position = newPos;

            return true;
        }

        public bool MoveIntoFinishLane(LudoToken token, int squares)
        {
            if (!WouldMoveIntoFinishLane(token, squares, out var squaresUntilFinish))
                return false;

            squares--;
            token.FinishPosition = 1;

            return MoveInsideFinishLane(token, squares - squaresUntilFinish);
        }

        public bool MoveInsideFinishLane(LudoToken token, int squares)
        {
            if (!token.IsInFinishLane || token.HasFinished) return false;

            token.FinishPosition = (squares) % (BoardFinishSize+2);

            return true;
        }

        public bool CanMove(LudoToken token, int squares)
        {
            for (var i = 1; i <= squares; i++)
            {
                var pos = GetNextSquare(token.Position, i);
                if (HasBlockade(pos))
                    return false;
            }

            return true;
        }

        public bool WouldMoveIntoFinishLane(LudoToken token, int squares)
        {
            return WouldMoveIntoFinishLane(token, squares, out _);
        }

        public bool WouldMoveIntoFinishLane(LudoToken token, int squares, out int squaresUntilFinish)
        {
            var finish = GetPlayerByColor(token.Color).FinishSquare;

            squaresUntilFinish = 0;

            for (var i = 1; i < squares; i++)
            {
                var pos = GetNextSquare(token.Position, i);
                squaresUntilFinish = i;
                if (pos == finish)
                    return true;
            }

            return false;
        }

        public bool TryCapture(int position, PlayerColor color)
        {
            if (HasBlockade(position) || IsSafeSquare(position)) return false;

            var tokens = GetAllTokensAt(position);

            if (tokens[0] == null) return false;

            return tokens[0].Color != color && CaptureToken(tokens[0]);
        }

        public int GetNextSquare(int position, int move = 1)
        {
            return (position + move) % BoardSize + ((position + move >= BoardSize) ? 1 : 0);
        }

        public bool CaptureToken(LudoToken token)
        {
            if (token.IsAtHome || token.IsInFinishLane || token.HasFinished)
                return false;

            token.Position = 0;

            return true;
        }
    }
}
