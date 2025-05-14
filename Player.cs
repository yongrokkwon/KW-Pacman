using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KW_Pacman
{
    internal class Player
    {
        public enum Direction { Left, Right, Up, Down, None }
        public enum PlayerState { Ready, Normal, Powered, Dead }

        /* === 공개 프로퍼티 (Form이 읽음) === */
        public PointF Position { get; private set; }
        public Direction Facing { get; private set; }
        public int FrameIndex { get; private set; } = 0;    // 0,1,2
        public PlayerState State { get; private set; } = PlayerState.Ready;

        /* === 이벤트 === */
        public event EventHandler Died;

        /* === 내부 필드 === */
        private PointF spawn;
        private Direction spawnDir;
        private int readyTimer = 2000;      // ms
        private int frameTick;
        private const int FrameMs = 80;

        public Player(PointF startPos, Direction startDir)
        {
            Position = spawn = startPos;
            Facing = spawnDir = startDir;
        } 

        public void SetDirection(Direction d)
        {
            if(d != Direction.None)
                Facing = d;
        }

        public void Update(int dt)
        {
            switch (State)
            {
                case PlayerState.Ready:
                    readyTimer -= dt;
                    if (readyTimer <= 0) 
                        State = PlayerState.Normal;
                    break;

                case PlayerState.Normal:
                    // move
                    break;
            }

            // 애니메이션 프레임
            frameTick += dt;
            if (frameTick >= FrameMs)
            {
                frameTick = 0;
                FrameIndex = (FrameIndex + 1) % 3;
            }
        }

        public void Respawn()
        {
            Position = spawn;
            Facing = spawnDir;
            State = PlayerState.Ready;
            readyTimer = 2000;
            FrameIndex = 0;
        }

        public void Die()
        {
            if (State is PlayerState.Dead) return;
            State = PlayerState.Dead;
            Died.Invoke(this, new EventArgs());
        }
    }
}

