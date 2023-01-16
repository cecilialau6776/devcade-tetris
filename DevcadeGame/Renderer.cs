using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;
using System.Collections;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace DevcadeGame
{
    public class Renderer
    {
        private Texture2D boardTexture;
        private Dictionary<Piece, Texture2D> pieceTextures;
        private SpriteBatch spriteBatch;
        private ContentManager Content;
        private GraphicsDeviceManager graphics;
        private Rectangle screenSize;
        private int players;
        private const double boardSize = 0.9; // Size of the board relative to the bounding axis of its screen

        private Rectangle[] boards;


        public Renderer(SpriteBatch spriteBatch, ContentManager content, GraphicsDeviceManager graphics)
        {
            this.spriteBatch = spriteBatch;
            this.Content = content;
            this.graphics = graphics;
            screenSize = graphics.GraphicsDevice.Viewport.Bounds;
            boards = new Rectangle[2];
            pieceTextures = new Dictionary<Piece, Texture2D>();
        }

        public void Render(Piece[,] board, Piece[] queue, Piece hold, int player)
        {
            DrawBoard(player);
            FillBoard(board, player);
            DrawQueue(queue, player);
            // DrawHold(hold, player);
        }
        public void LoadTextures()
        {
            boardTexture = Content.Load<Texture2D>("board");

            pieceTextures.Add(Piece.T, Content.Load<Texture2D>("tPiece"));
            pieceTextures.Add(Piece.I, Content.Load<Texture2D>("iPiece"));
            pieceTextures.Add(Piece.L, Content.Load<Texture2D>("lPiece"));
            pieceTextures.Add(Piece.J, Content.Load<Texture2D>("jPiece"));
            pieceTextures.Add(Piece.Z, Content.Load<Texture2D>("zPiece"));
            pieceTextures.Add(Piece.S, Content.Load<Texture2D>("sPiece"));
            pieceTextures.Add(Piece.O, Content.Load<Texture2D>("oPiece"));
        }

        public void SetPlayers(int players)
        {
            this.players = players;
        }

        public void DrawBoard(int player)
        {
            if (players == 1)
            {
                boards[0] = GetBoardRect(screenSize);
            }
            else
            {
                Rectangle screen;
                if (screenSize.Width > screenSize.Height)
                {
                    int w = screenSize.Width / 2;
                    if (player == 1) screen = new Rectangle(screenSize.X, screenSize.Y, w, screenSize.Height);
                    else screen = new Rectangle(screenSize.X + w, screenSize.Y, w, screenSize.Height);
                }
                else
                {
                    int h = screenSize.Height / 2;
                    if (player == 1) screen = new Rectangle(screenSize.X, screenSize.Y, screenSize.Width, h);
                    else screen = new Rectangle(screenSize.X, screenSize.Y + h, screenSize.Width, h);
                }
                boards[player - 1] = GetBoardRect(screen);
            }
            // Console.WriteLine(boards[player - 1]);
            spriteBatch.Draw(boardTexture, boards[player - 1], Color.White);
        }

        public void FillBoard(Piece[,] board, int player)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    // Console.WriteLine("({0}, {1})", i, j);
                    DrawTile(board[i, j], i, j, boards[player - 1]);
                }
            }
        }

        public void DrawQueue(Piece[] queue, int player)
        {
            Console.WriteLine("Player: " + player);
            int tileSize = boards[0].Width / 10;
            Rectangle cb = boards[player - 1];
            Rectangle center = new Rectangle(cb.X + cb.Width + (tileSize * 3), cb.Y + (tileSize * 3 / 2), tileSize, tileSize);
            foreach (Piece p in queue)
            {
                switch (p)
                {
                    case Piece.I:
                        center.Offset(-tileSize / 2, tileSize / 2);
                        DrawPiece(p, Rotation.Spawn, center);
                        center.Offset(tileSize / 2, -tileSize / 2);
                        break;
                    case Piece.O:
                        center.Offset(-tileSize / 2, 0);
                        DrawPiece(p, Rotation.Spawn, center);
                        center.Offset(tileSize / 2, 0);
                        break;
                    default:
                        DrawPiece(p, Rotation.Spawn, center);
                        break;
                }
                center.Offset(0, tileSize * 3);
            }
        }

        public void DrawHold(Piece p, int player)
        {
            int tileSize = boards[0].Width / 10;
            Rectangle cb = boards[player - 1];
            Rectangle c = new Rectangle(cb.X - tileSize * 4, cb.Y + tileSize * 3 / 2, tileSize, tileSize);
            DrawPiece(p, Rotation.Spawn, c);
        }

        private Rectangle GetBoardRect(Rectangle screen)
        {
            // Find bounding axis
            double ratio = ((double)screen.Height) / ((double)screen.Width);
            if (ratio > (24d / 21d))
            {
                // Bound by the width
                int padding = (int)(screen.Width * (1 - boardSize) / 2);
                int width = (int)(screen.Width * boardSize * 10 / 21);
                int height = 2 * width;
                int tileSize = width / 10;
                return new Rectangle(screen.X + padding + tileSize * 11 / 2, screen.Y + padding, width, height);
            }
            else
            {
                // Bound by the height
                int padding = (int)(screen.Height * (1 - boardSize) / 2);
                int height = (int)(screen.Height * boardSize);
                int width = (int)(24d / 21d * height);
                return new Rectangle(screen.X + padding, screen.Y + padding, width, height);
            }
        }

        public void DrawPiece(Piece piece, Rotation rot, Rectangle center)
        {
            if (piece == Piece.None) return;
            foreach (Point delta in TetrisGame.GetDeltas(piece, rot))
            {
                Rectangle rect = GetRectangle(delta, center);
                spriteBatch.Draw(pieceTextures[piece], rect, Color.White);
            }
        }

        public void DrawPiece(Piece piece, Rotation rot, int line, int col, Rectangle board)
        {
            // Compute center
            DrawPiece(piece, rot, GetTileRect(line, col, board));
        }

        public void DrawTile(Piece piece, int line, int col, Rectangle board)
        {
            if (piece == Piece.None) return;
            spriteBatch.Draw(pieceTextures[piece], GetTileRect(line, col, board), Color.White);
        }

        private Rectangle GetTileRect(int line, int col, Rectangle board)
        {
            int w = board.Width / 10;
            int x = board.X + w * col;
            int y = board.Y + w * (line - 4);
            return new Rectangle(x, y, w, w);
        }

        private Rectangle GetRectangle(Point delta, Rectangle center)
        {
            int h = center.Height;
            int w = center.Width;
            return new Rectangle(center.X + (delta.X * w), center.Y + (delta.Y * w), w, h);
            // return new Rectangle(0, 0, 0, 0);
        }
    }
}
