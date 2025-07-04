﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PACKMAN;

namespace KW_Pacman
{
    internal class Player
    {
        public enum Direction { Left, Right, Up, Down, None }
        public enum PlayerState { Ready, Normal, Powered, Dead, Stopped }

        /* === 공개 프로퍼티 (Form이 읽음) === */
        public PointF Position { get; private set; }
        public Direction Facing { get; private set; }
        public int FrameIndex { get; private set; } = 0;    // 0,1,2
        public PlayerState State { get; private set; } = PlayerState.Ready;
        private Direction nextDirection = Direction.None;
        private bool isMoving = true;

        /* === 이벤트 === */
        public event EventHandler Died;

        /* === 내부 필드 === */
        private PointF spawn;
        private Direction spawnDir;
        private int readyTimer = 2000;      // ms
        private int frameTick;
        private const int FrameMs = 80;
        private float currentSpeed = 6f; // px/frame
        private const float NORMAL_SPEED = 6f;
        private const float POWERED_SPEED = 9f; // 파워 상태일 때 1.5배 속도
        private Point gridPosition;  // 격자 위치
        private PointF targetPosition;  // 목표 픽셀 위치
        private bool isMovingToTarget = false;

        public Player(PointF startPos, Direction startDir)
        {
            Position = spawn = startPos;
            Facing = spawnDir = startDir;
            gridPosition = new Point((int)startPos.X / 24, (int)startPos.Y / 24);
            targetPosition = startPos;
            isMoving = true;
        }

        private int powerTimer = 0;
        private const int POWER_DURATION = 8000; // 8초

        public void Update(int deltaTime)
        {
            if (State == PlayerState.Ready)
            {
                readyTimer -= deltaTime;
                if (readyTimer <= 0)
                    State = PlayerState.Normal;
                return;
            }

            // 파워 상태 타이머 처리
            if (State == PlayerState.Powered)
            {
                powerTimer -= deltaTime;
                if (powerTimer <= 0)
                {
                    State = PlayerState.Normal;
                    currentSpeed = NORMAL_SPEED; // 속도 복원
                }
            }

            if (State != PlayerState.Normal && State != PlayerState.Powered) return;

            // 다음 방향 체크
            if (nextDirection != Direction.None && CanMoveToNextGrid(nextDirection))
            {
                Facing = nextDirection;
                nextDirection = Direction.None;
            }

            // 현재 목표 지점으로 이동 중이면 계속 이동
            if (isMovingToTarget)
            {
                MoveTowardsTarget();
            }
            else
            {
                // 새로운 격자로 이동 시도
                if (CanMoveToNextGrid(Facing))
                {
                    StartMoveToNextGrid(Facing);
                }
            }

            // 프레임 애니메이션
            frameTick += deltaTime;
            if (frameTick >= FrameMs)
            {
                FrameIndex = (FrameIndex + 1) % 3;
                frameTick = 0;
            }
        }

        public void SetPowered()
        {
            State = PlayerState.Powered;
            powerTimer = POWER_DURATION;
            currentSpeed = POWERED_SPEED; // 속도 증가
        }

        public bool IsPowered()
        {
            return State == PlayerState.Powered;
        }

        private bool CanMoveToNextGrid(Direction dir)
        {
            if (dir == Direction.None) return false;

            Point nextGrid = gridPosition;

            switch (dir)
            {
                case Direction.Left:
                    nextGrid.X -= 1;
                    break;
                case Direction.Right:
                    nextGrid.X += 1;
                    break;
                case Direction.Up:
                    nextGrid.Y -= 1;
                    break;
                case Direction.Down:
                    nextGrid.Y += 1;
                    break;
            }

            // 터널 처리 (Row 10에서 좌우 이동 시)
            if (gridPosition.Y == 10)
            {
                if (nextGrid.X < 0)
                {
                    // 왼쪽 터널로 나가면 오른쪽 터널로 이동 가능
                    return true;
                }
                if (nextGrid.X >= 19)
                {
                    // 오른쪽 터널로 나가면 왼쪽 터널로 이동 가능
                    return true;
                }
            }

            // 경계 체크
            if (nextGrid.X < 0 || nextGrid.X >= 19 || nextGrid.Y < 0 || nextGrid.Y >= 21)
                return false;

            // 벽 체크 (터널 입구는 값이 2, 닷은 3, 파워펠렛은 4이므로 모두 통과 가능)
            return Form1.Instance.MazeGrid[nextGrid.Y, nextGrid.X] == 0 ||
                   Form1.Instance.MazeGrid[nextGrid.Y, nextGrid.X] == 2 ||
                   Form1.Instance.MazeGrid[nextGrid.Y, nextGrid.X] == 3 ||
                   Form1.Instance.MazeGrid[nextGrid.Y, nextGrid.X] == 4;
        }

        private void StartMoveToNextGrid(Direction dir)
        {
            Point nextGrid = gridPosition;

            switch (dir)
            {
                case Direction.Left:
                    nextGrid.X -= 1;
                    break;
                case Direction.Right:
                    nextGrid.X += 1;
                    break;
                case Direction.Up:
                    nextGrid.Y -= 1;
                    break;
                case Direction.Down:
                    nextGrid.Y += 1;
                    break;
            }

            // 터널 처리 (Row 10에서 좌우 이동 시)
            if (gridPosition.Y == 10)
            {
                if (nextGrid.X < 0)
                {
                    // 왼쪽으로 나가면 오른쪽 터널로 순간이동
                    nextGrid.X = 18;
                    Position = new PointF(18 * 24, 10 * 24); // 즉시 위치 변경
                }
                else if (nextGrid.X >= 19)
                {
                    // 오른쪽으로 나가면 왼쪽 터널로 순간이동
                    nextGrid.X = 0;
                    Position = new PointF(0 * 24, 10 * 24); // 즉시 위치 변경
                }
            }

            gridPosition = nextGrid;
            targetPosition = new PointF(nextGrid.X * 24, nextGrid.Y * 24);
            isMovingToTarget = true;
        }

        private void MoveTowardsTarget()
        {
            float deltaX = targetPosition.X - Position.X;
            float deltaY = targetPosition.Y - Position.Y;
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (distance <= currentSpeed) // SPEED 대신 currentSpeed 사용
            {
                // 목표 도달
                Position = targetPosition;
                isMovingToTarget = false;
            }
            else
            {
                // 목표 방향으로 이동
                float moveX = (deltaX / distance) * currentSpeed; // SPEED 대신 currentSpeed 사용
                float moveY = (deltaY / distance) * currentSpeed; // SPEED 대신 currentSpeed 사용
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
        }

        public void SetNextDirection(Direction dir)
        {
            nextDirection = dir;
        }

        // 목숨을 항상 3으로 유지
        public int lives { get; set; } = 3; // private set을 set으로 변경

        // 기존 Respawn 메서드를 다음과 같이 수정:
        public void Respawn()
        {
            Position = spawn;
            Facing = spawnDir;
            State = PlayerState.Ready;
            readyTimer = 2000;
            FrameIndex = 0;
            currentSpeed = NORMAL_SPEED; // 속도 초기화

            // 격자 위치와 이동 관련 변수들 초기화
            gridPosition = new Point((int)spawn.X / 24, (int)spawn.Y / 24);
            targetPosition = spawn;
            isMovingToTarget = false;
            nextDirection = Direction.None;

            // 파워 상태 초기화
            powerTimer = 0;
            if (State == PlayerState.Powered)
                State = PlayerState.Ready;
        }

        // 새로운 리셋 메서드 추가
        public void ResetToPosition(PointF newPos, Direction newDir)
        {
            Position = spawn = newPos;
            Facing = spawnDir = newDir;
            State = PlayerState.Ready;
            readyTimer = 2000;
            FrameIndex = 0;
            currentSpeed = NORMAL_SPEED; // 속도 초기화

            // 격자 위치와 이동 관련 변수들 초기화
            gridPosition = new Point((int)newPos.X / 24, (int)newPos.Y / 24);
            targetPosition = newPos;
            isMovingToTarget = false;
            nextDirection = Direction.None;

            // 파워 상태 초기화
            powerTimer = 0;

            // lives = 3; // 이 줄 제거 - 목숨은 유지
        }

        public void Die()
        {
            if (State is PlayerState.Dead) return;
            State = PlayerState.Dead;
            lives--; // 목숨 감소
            Died.Invoke(this, new EventArgs());
        }

        public void SetNormal()
        {
            State = PlayerState.Normal;
            currentSpeed = NORMAL_SPEED; // 속도 초기화
        }

        public void SetStopped()
        {
            State = PlayerState.Stopped;
        }

    }
}

