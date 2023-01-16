using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Devcade;
using System.Collections;
using System;
using System.Collections.Generic;

namespace DevcadeGame
{
    public enum State
    {
        Starting,
        Playing,
        Lost
    }
    public enum Piece
    {
        None,
        T,
        I,
        L,
        J,
        Z,
        S,
        O
    }

    public enum Rotation : int
    {
        Spawn = 0,
        Left = 1,
        Right = 3,
        Flip = 2
    }

    public class TetrisGame
    {
        private static Piece[] bag = {
            Piece.T,
            Piece.I,
            Piece.L,
            Piece.J,
            Piece.Z,
            Piece.S,
            Piece.O
        };

        // Game Variables
        #region
        State state;
        Piece currentPiece;
        Piece holdPiece;
        bool swapped;
        Piece[,] board;
        Queue<Piece> queue;
        Rotation rotActive;
        double gravity;
        int gravityCounter;
        int lockDelay; // Measured in frames
        int lockCounter;
        Random queueRng;
        Random garbageRng;
        int player;
        // InputManager inputManager;
        int colActive, lineActive;
        Renderer renderer;
        #endregion

        public TetrisGame(int seed, int player)
        {
            state = State.Starting;
            board = new Piece[10, 24];
            queue = new Queue<Piece>();
            queueRng = new Random(seed);
            garbageRng = new Random(seed);
            swapped = false;
            lockDelay = 30;
            gravity = 1 / 64;
            QueueAddBag();
            this.player = player;
            // this.inputManager = im;
        }

        public void Initialize(Renderer r)
        {
            this.renderer = r;
        }

        // Game Helpers
        #region

        private bool checkPieceIs(int x, int y, Piece piece)
        {
            if (!(x >= 0 && x < 10 && y >= 0 && y < 24))
            {
                return false;
            }
            return this.board[x, y] == piece;
        }

        private void QueueAddBag()
        {
            // Randomize bag
            Piece[] bag = new Piece[7];
            TetrisGame.bag.CopyTo(bag, 0);
            int i = 7;
            while (i > 1)
            {
                int k = this.queueRng.Next(i--);
                Piece temp = bag[i];
                bag[i] = bag[k];
                bag[k] = temp;
            }

            // Append to queue
            foreach (Piece p in bag)
            {
                this.queue.Enqueue(p);
            }
        }

        private void SetPieceAt(Piece shape, Rotation rot, Piece fill, int line, int col)
        {
            if (shape == Piece.None) return;
            Point active = new Point(line, col);
            foreach (Point p in GetDeltas(shape, rot))
            {
                Point curr = active + p;
                this.board[curr.X, curr.Y] = fill;
            }
        }

        private void SpawnNext(Piece piece)
        {
            this.gravityCounter = 0;
            this.colActive = 4;
            this.lineActive = 3;
            this.rotActive = Rotation.Spawn;
            this.SetPieceAt(this.currentPiece, this.rotActive, piece, this.lineActive, this.colActive);
        }

        private void SpawnNext()
        {
            Console.WriteLine("SpawnNext: P: {0}, gc: {1}, ca: {2}, la: {3}", player, gravityCounter, colActive, lineActive);
            if (this.queue.Count <= 7)
            {
                QueueAddBag();
            }
            this.currentPiece = this.queue.Dequeue();
            SpawnNext(this.currentPiece);
        }

        private bool ToppedOut()
        {
            return !(this.board[2, 3] == Piece.None &&
                     this.board[2, 4] == Piece.None &&
                     this.board[2, 5] == Piece.None &&
                     this.board[3, 3] == Piece.None &&
                     this.board[3, 4] == Piece.None &&
                     this.board[3, 5] == Piece.None &&
                     this.board[3, 6] == Piece.None);
        }

        private void LockActive()
        {
            this.lockCounter = 0;

            // process line clearing
            int linesCleared = 0;
            int i = 20;
            while (i > 0)
            {
                // check line is full
                for (int j = 0; j < 10; j++)
                {
                    if (this.board[i, j] == Piece.None)
                    {
                        i--;
                        continue;
                    }
                }

                // line is full
                linesCleared++;
                for (int j = i; j > 1; j--)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        this.board[j, k] = this.board[j - 1, k];
                    }
                }
            }
        }

        public void Render()
        {
            renderer.Render(this.board, GetQueue(), this.holdPiece, player);
        }

        public void RotateActive(Rotation rot)
        {
            Rotation rotFinal = (Rotation)(((int)rot + (int)rotActive) % 4);
            if (rot == Rotation.Flip)
            {
                // Remove active piece from board for tests
                this.SetPieceAt(this.currentPiece, this.rotActive, Piece.None, this.lineActive, this.colActive);
                int l = this.lineActive;
                int c = this.colActive;
                Point[] deltas = GetDeltas(this.currentPiece, rotFinal);
                // Check if this rotaiton works. Rotate if it does, do nothing if not
                if (board[l + deltas[0].X, c + deltas[0].Y] == Piece.None &&
                    board[l + deltas[1].X, c + deltas[1].Y] == Piece.None &&
                    board[l + deltas[2].X, c + deltas[2].Y] == Piece.None &&
                    board[l + deltas[3].X, c + deltas[3].Y] == Piece.None)
                {
                    this.rotActive = rotFinal;
                }
                SetPieceAt(this.currentPiece, this.rotActive, this.currentPiece, this.lineActive, this.colActive);
            }

            Point[] tests;
            switch (this.currentPiece)
            {
                case Piece.T:
                case Piece.L:
                case Piece.J:
                case Piece.Z:
                case Piece.S:
                    switch ((this.rotActive, rotFinal))
                    {
                        case (Rotation.Right, Rotation.Spawn):
                        case (Rotation.Right, Rotation.Flip):
                            tests = new Point[] { new Point(0, 1), new Point(1, 1), new Point(-2, 0), new Point(-2, 1) };
                            break;
                        case (Rotation.Spawn, Rotation.Right):
                        case (Rotation.Flip, Rotation.Right):
                            tests = new Point[] { new Point(0, -1), new Point(-1, -1), new Point(2, 0), new Point(2, -1) };
                            break;
                        case (Rotation.Flip, Rotation.Left):
                        case (Rotation.Spawn, Rotation.Left):
                            tests = new Point[] { new Point(0, 1), new Point(-1, 1), new Point(2, 0), new Point(2, 1) };
                            break;
                        case (Rotation.Left, Rotation.Flip):
                        case (Rotation.Left, Rotation.Spawn):
                            tests = new Point[] { new Point(0, -1), new Point(1, -1), new Point(-2, 0), new Point(-2, -1) };
                            break;
                        default:
                            tests = new Point[] { };
                            break;
                    }
                    break;
                // TODO: potentially fix I piece rotation
                case Piece.I:
                    switch ((this.rotActive, rotFinal))
                    {
                        case (Rotation.Spawn, Rotation.Right):
                        case (Rotation.Left, Rotation.Flip):
                            tests = new Point[] { new Point(0, -2), new Point(0, 1), new Point(1, -2), new Point(-2, 1) };
                            break;
                        case (Rotation.Right, Rotation.Spawn):
                        case (Rotation.Flip, Rotation.Left):
                            tests = new Point[] { new Point(0, 2), new Point(0, -1), new Point(-1, 2), new Point(2, -1) };
                            break;
                        case (Rotation.Right, Rotation.Flip):
                        case (Rotation.Spawn, Rotation.Left):
                            tests = new Point[] { new Point(0, -1), new Point(0, 2), new Point(-2, -1), new Point(1, 2) };
                            break;
                        case (Rotation.Flip, Rotation.Right):
                        case (Rotation.Left, Rotation.Spawn):
                            tests = new Point[] { new Point(0, 1), new Point(0, -2), new Point(2, 1), new Point(-1, -2) };
                            break;
                        default:
                            tests = new Point[] { };
                            break;
                    }
                    break;
                case Piece.O:
                case Piece.None:
                default:
                    tests = new Point[] { };
                    break;
            }

            // Remove active piece from board for tests
            this.SetPieceAt(this.currentPiece, this.rotActive, Piece.None, this.lineActive, this.colActive);
            int line = this.lineActive;
            int col = this.colActive;
            Point[] ds = GetDeltas(this.currentPiece, rotFinal);
            // Run tests
            for (int i = 0; i < tests.Length + 1; i++)
            {
                Point test;
                if (i == 0)
                {
                    test = new Point(0, 0);
                }
                else
                {
                    test = tests[i - 1];
                }

                int lt = line + tests[i].X;
                int ct = col + tests[i].Y;
                if (this.board[lt + ds[0].X, ct + ds[0].Y] == Piece.None &&
                    this.board[lt + ds[1].X, ct + ds[1].Y] == Piece.None &&
                    this.board[lt + ds[2].X, ct + ds[2].Y] == Piece.None &&
                    this.board[lt + ds[3].X, ct + ds[3].Y] == Piece.None)
                {
                    // Put piece in place; test passed.
                    this.lineActive = line + tests[i].X;
                    this.colActive = col + tests[i].Y;
                    this.rotActive = rotFinal;
                    SetPieceAt(this.currentPiece, rotFinal, this.currentPiece, this.lineActive, this.colActive);
                    return;
                }

            }

            // Rotation unsuccessful; put piece back
            SetPieceAt(this.currentPiece, rotFinal, this.currentPiece, this.lineActive, this.colActive);
        }

        public void Hold()
        {
            if (this.swapped) return;

            this.swapped = true;
            this.SetPieceAt(this.currentPiece, this.rotActive, Piece.None, this.lineActive, this.colActive);
            Piece temp = this.holdPiece;
            this.holdPiece = this.currentPiece;
            this.currentPiece = temp;
            if (this.currentPiece == Piece.None)
            {
                SpawnNext();
            }
            else
            {
                SpawnNext(this.currentPiece);
            }
        }

        public void Harddrop()
        {
            while (MoveActive(Direction.Down)) ;
            LockActive();
        }

        public bool MoveActive(Direction dir)
        {
            return MoveActive(dir, true);
        }


        public bool MoveActive(Direction dir, bool performMove)
        {
            Point[] ds = GetDeltas(this.currentPiece, this.rotActive);
            int l = this.lineActive;
            int c = this.colActive;

            // Remove current piece for test
            SetPieceAt(this.currentPiece, this.rotActive, Piece.None, this.lineActive, this.colActive);

            Point d;
            switch (dir)
            {
                case Direction.Left:
                    d = new Point(0, -1);
                    break;
                case Direction.Down:
                    d = new Point(-1, 0);
                    break;
                case Direction.Right:
                    d = new Point(0, 1);
                    break;
                default:
                    d = new Point(0, 0);
                    break;
            }

            // Test if the move is possible
            try
            {
                if (
                    checkPieceIs(l + d.X + ds[0].X, c + d.Y + ds[0].Y, Piece.None) &&
                    checkPieceIs(l + d.X + ds[1].X, c + d.Y + ds[1].Y, Piece.None) &&
                    checkPieceIs(l + d.X + ds[2].X, c + d.Y + ds[2].Y, Piece.None) &&
                    checkPieceIs(l + d.X + ds[3].X, c + d.Y + ds[3].Y, Piece.None)
                )
                {
                    if (performMove)
                    {
                        this.lineActive = l + d.X;
                        this.colActive = c + d.Y;
                    }
                    SetPieceAt(this.currentPiece, this.rotActive, this.currentPiece, this.lineActive, this.colActive);
                    return true;
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", l, d.X, ds[0].X, c, d.Y, ds[0].Y);
                throw;
            }
            return false;
        }
        #endregion

        public Piece[] GetQueue()
        {
            Piece[] q = this.queue.ToArray();
            Piece[] outArr = new Piece[5];
            outArr[0] = q[0];
            outArr[1] = q[1];
            outArr[2] = q[2];
            outArr[3] = q[3];
            outArr[4] = q[4];
            return outArr;
        }

        public void UpdatePre(int ticks)
        {
            // Check state
            if (this.state == State.Lost || this.state == State.Starting)
            {
                return;
            }
            // TODO: Process gravity

            // Check top out
            if (ToppedOut())
            {
                this.state = State.Lost;
            }
        }

        public void Update(int ticks) { } // Input is processed during this

        public void UpdatePost(int ticks)
        {
            // Check if on ground
            if (!MoveActive(Direction.Down, false))
            {
                Console.WriteLine("Player: {0}, On ground", player);
                // Process lock if applicable, and spawn piece
                this.lockCounter += ticks;
                if (this.lockCounter >= this.lockDelay)
                {
                    SpawnNext();
                }
            }
            else
            {
                Console.WriteLine("Player: {0}, Falling", player);
                // Update gravity
                this.gravityCounter += ticks;
                if (this.gravityCounter >= this.gravity)
                {
                    this.gravityCounter = 0;
                    MoveActive(Direction.Down);
                }
            }
        }

        // Start GetDeltas Function
        public static Point[] GetDeltas(Piece p, Rotation r)
        {
            Point[] outArr = new Point[4];
            switch (p)
            {
                case Piece.T:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(-1, 0);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                    }
                    break;
                case Piece.I:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(-1, 0);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(2, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(0, -1);
                            outArr[3] = new Point(0, -2);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(1, 1);
                            outArr[1] = new Point(1, 0);
                            outArr[2] = new Point(1, -1);
                            outArr[3] = new Point(1, -2);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(-1, -1);
                            outArr[1] = new Point(0, -1);
                            outArr[2] = new Point(1, -1);
                            outArr[3] = new Point(2, -1);
                            break;
                    }
                    break;
                case Piece.L:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(1, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(0, -1);
                            outArr[3] = new Point(1, -1);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(-1, 1);
                            outArr[1] = new Point(0, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(-1, 0);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(-1, -1);
                            break;
                    }
                    break;
                case Piece.J:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(-1, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(-1, -1);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(-1, 0);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(1, -1);
                            break;
                    }
                    break;
                case Piece.Z:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(-1, 1);
                            outArr[1] = new Point(0, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(-1, -1);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(-1, 0);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(0, -1);
                            outArr[3] = new Point(1, -1);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(1, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                    }
                    break;
                case Piece.S:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(-1, 0);
                            outArr[3] = new Point(0, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(-1, 1);
                            outArr[1] = new Point(-1, 0);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(0, 0);
                            outArr[1] = new Point(1, 0);
                            outArr[2] = new Point(-1, -1);
                            outArr[3] = new Point(0, -1);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(0, 0);
                            outArr[2] = new Point(1, 0);
                            outArr[3] = new Point(1, -1);
                            break;
                    }
                    break;
                case Piece.O:
                    switch (r)
                    {
                        case Rotation.Spawn:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Left:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Right:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                        case Rotation.Flip:
                            outArr[0] = new Point(0, 1);
                            outArr[1] = new Point(1, 1);
                            outArr[2] = new Point(0, 0);
                            outArr[3] = new Point(1, 0);
                            break;
                    }
                    break;
            }
            return outArr;
        }

        // End GetDeltas Function
    }
}
