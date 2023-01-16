using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;
using System.Collections;
using System;
using System.Collections.Generic;

namespace DevcadeGame
{
    public enum GameAction
    {
        Left,
        Right,
        Down,
        RotateLeft, // Counterclockwise rotation
        RotateRight, // Clockwise rotation
        Harddrop,
        Hold,
        Flip
    }

    public class InputManager
    {
        TetrisGame[] games;
        Menu menu;
        // Tuning options, will leave default for now.

        // The following are counted in frames/ticks
        int DAS;
        int ARR;

        int[] dasCounter;
        Direction[] dasDir;

        // Input mapping
        // Keyboard inputs
        Dictionary<GameAction, List<Keys>>[] playerKeys;
        // Controller inputs
        Dictionary<GameAction, List<Input.ArcadeButtons>> playerButtons;

        public InputManager(TetrisGame game1, TetrisGame game2, Menu menu)
        {
            games = new TetrisGame[] { game1, game2 };
            dasCounter = new int[] { 0, 0 };
            dasDir = new Direction[] { Direction.None, Direction.None };
            DAS = 16;
            ARR = 3;
            this.menu = menu;

            // Initialize keys
            #region
            playerKeys = new Dictionary<GameAction, List<Keys>>[] {
                // Player 1
                new Dictionary<GameAction, List<Keys>> {
            { GameAction.Left, new List<Keys> { Keys.Left } },
            { GameAction.Right, new List<Keys> { Keys.Right } },
            { GameAction.Down, new List<Keys> { Keys.Down } },
            { GameAction.RotateLeft, new List<Keys> { Keys.OemComma } },
            { GameAction.RotateRight, new List<Keys> { Keys.OemPeriod, Keys.Up } },
            { GameAction.Harddrop, new List<Keys> { Keys.OemQuestion } },
            { GameAction.Hold, new List<Keys> { Keys.M } },
            { GameAction.Flip, new List<Keys> { Keys.K, Keys.L } }
            },
                // Player 2
                new Dictionary<GameAction, List<Keys>> {
            { GameAction.Left, new List<Keys> { Keys.F } },
            { GameAction.Right, new List<Keys> { Keys.H } },
            { GameAction.Down, new List<Keys> { Keys.G } },
            { GameAction.RotateLeft, new List<Keys> { Keys.Z } },
            { GameAction.RotateRight, new List<Keys> { Keys.C, Keys.T } },
            { GameAction.Harddrop, new List<Keys> { Keys.Space } },
            { GameAction.Hold, new List<Keys> { Keys.LeftShift } },
            { GameAction.Flip, new List<Keys> { Keys.A, Keys.S } }
                }
            };
            playerButtons = new Dictionary<GameAction, List<Input.ArcadeButtons>> {
            { GameAction.Left, new List<Input.ArcadeButtons> { } },
            { GameAction.Right, new List<Input.ArcadeButtons> { } },
            { GameAction.Down, new List<Input.ArcadeButtons> { } },
            { GameAction.RotateLeft, new List<Input.ArcadeButtons> { } },
            { GameAction.RotateRight, new List<Input.ArcadeButtons> { } },
            { GameAction.Harddrop, new List<Input.ArcadeButtons> { } },
            { GameAction.Hold, new List<Input.ArcadeButtons> { } },
            { GameAction.Flip, new List<Input.ArcadeButtons> { } }

            };
            // TODO: Map these to the correct buttons
            #endregion

        }

        public void ProcessInput(int ticks, GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    ProcessMenuInput();
                    break;
                case GameState.Playing:
                    ProcessPlayingInput(ticks);
                    break;
                case GameState.Lost:
                    break;
            }
        }

        private void ProcessMenuInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Down) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.StickDown) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickDown))
            {
                menu.MoveSelector(Direction.Down);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.StickUp) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.StickUp))
            {
                menu.MoveSelector(Direction.None); // used as Up
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.B1) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.B1))
            {
                menu.Select();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.O) ||
            Input.GetButtonDown(1, Input.ArcadeButtons.A1) ||
            Input.GetButtonDown(2, Input.ArcadeButtons.A1))
            {
                menu.Options();
            }

        }

        private void ProcessPlayingInput(int ticks)
        {
            // Loop through users
            for (int p = 1; p <= 2; p++)
            {
                // Process left/right/down movement
                bool resetDas = true;

                if (GetActionPressed(p, GameAction.Left))
                {
                    resetDas = false;
                    if (dasDir[p] != Direction.Left)
                    {
                        // reset DAS and move
                        resetDas = true;
                        games[p].MoveActive(Direction.Left);
                    }
                    else
                    {
                        dasCounter[p] += ticks;
                    }
                }

                if (GetActionPressed(p, GameAction.Right))
                {
                    resetDas = false;
                    if (dasDir[p] != Direction.Right)
                    {
                        // reset DAS and move
                        resetDas = true;
                        games[p].MoveActive(Direction.Right);
                    }
                    else
                    {
                        dasCounter[p] += ticks;
                    }
                }

                if (GetActionPressed(p, GameAction.Down))
                {
                    resetDas = false;
                    if (dasDir[p] != Direction.Down)
                    {
                        // reset DAS and move
                        resetDas = true;
                        games[p].MoveActive(Direction.Down);
                    }
                    else
                    {
                        dasCounter[p] += ticks;
                    }
                }

                if (resetDas)
                {
                    dasDir[p] = Direction.None;
                    dasCounter[p] = 0;
                }
                else // Check for DAS
                {
                    if (dasCounter[p] >= DAS)
                    {
                        int moveCount;
                        if (ARR == 0)
                        {
                            moveCount = 10;
                        }
                        else
                        {
                            moveCount = ((dasCounter[p] - DAS) / ARR) + 1;
                        }
                        for (int i = 0; i < moveCount; i++) {
                            games[p].MoveActive(dasDir[p]);
                        }
                    }
                }

                // Process harddrop
                if (GetActionPressed(p, GameAction.Harddrop))
                {
                    games[p].Harddrop();
                }

                if (GetActionPressed(p, GameAction.RotateLeft))
                {
                    games[p].RotateActive(Rotation.Left);
                }
                if (GetActionPressed(p, GameAction.RotateRight))
                {
                    games[p].RotateActive(Rotation.Right);
                }
                if (GetActionPressed(p, GameAction.Flip))
                {
                    games[p].RotateActive(Rotation.Flip);
                }

                if (GetActionPressed(p, GameAction.Hold))
                {
                    games[p].Hold();
                }

            }

        }

        private bool GetActionPressed(int player, GameAction action)
        {
            // Check keyboard
            foreach (Keys key in playerKeys[player - 1][action])
            {
                if (Keyboard.GetState().IsKeyDown(key)) return true;
            }

            // Check controller
            foreach (Input.ArcadeButtons button in playerButtons[action])
            {
                if (Input.GetButton(player, button)) return true;
            }
            return false;
        }

    }
}
