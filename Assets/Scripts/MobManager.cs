using System;
using System.Collections.Generic;
using UnityEngine;

internal class MobManager : MonoBehaviour
{
    public GameObject[] EnemyPrefabs;
    public float GapX = 0.5f;
    public float GapY = 0.5f;
    public float FireCycle = 0.2f;
    public float FireProbability = 0.5f;
    public float MovementCycle = 2f;
    public int Rows = 5;
    public int Columns = 11;
    private InvaderMob Invaders;
    private Vector3 Invadersize;
    private MobBounds Bounds;
    private MobFireController FireController;
    private MovementCycleController MovementCycleController;

    private void Start()
    {
        Invaders = new InvaderMob(Rows, Columns);
        FireController = new MobFireController(FireCycle, FireProbability);
        MovementCycleController = new MovementCycleController(MovementCycle, MobMovement.Left);
        Invadersize = EnemyPrefabs[0].GetComponent<SpriteRenderer>().bounds.size;
        Bounds = Invaders.Fill(EnemyPrefabs, Invadersize, transform.position, GapX, GapY, (prefab, row, column, x, y) =>
        {
            var enemy = Instantiate(prefab);
            var invader = enemy.GetComponent<Invader>();
            invader.OnDestroyed += EnemyDestroyed;
            invader.SetRowAndColumn(row, column);
            enemy.transform.position = new Vector3(x, y);
            return enemy;
        });
    }

    private void Update()
    {
        if (FireController.ShouldFire())
        {
            FireRandomProjectile();
        }
        if (MovementCycleController.ShouldMove(out var moveDirection))
        {
            if (Bounds.CanMove(moveDirection))
            {
                // Adjust new offset.
                MoveMob(moveDirection);
                Bounds.AdjustOffset(moveDirection);
            }
            else
            {
                MoveMobDown();
                MovementCycleController.SwitchDirection();
            }
        }
    }

    private void MoveMob(MobMovement movement)
    {
        // Movement will move from left to right or from right to left depending on the movement.
        var currentDelay = 0f;
        var delayIncrement = 0.01f;
        var movementSpeed = 3f;
        switch (movement)
        {
            case MobMovement.Left:
                foreach(var invader in Invaders.EnumerateLeftToRight())
                {
                    invader.SetMoveX(Invadersize.x * -1, currentDelay, movementSpeed);
                    currentDelay += delayIncrement;
                }
                break;

            case MobMovement.Right:
                foreach(var invader in Invaders.EnumerateRightToLeft())
                {
                    invader.SetMoveX(Invadersize.x, currentDelay, movementSpeed);
                    currentDelay += delayIncrement;
                }
                break;
        }
    }
    private void MoveMobDown()
    {
        var currentDelay = 0f;
        var delayIncrement = 0.1f;
        var movementSpeed = 3f;
        foreach(var invaderRow in Invaders.EnumerateRowsBottomToTop())
        {
            foreach(var invader in invaderRow)
            {
                invader.SetMoveY(Invadersize.y * -1, currentDelay, movementSpeed);
            }
            currentDelay += delayIncrement;
        }
    }
    private void RecalculateValues()
    {
        var newColumnEdges = Invaders.RecalculateValues();
        Bounds.RecalculateBounds(newColumnEdges.leftEdgeCol, newColumnEdges.rightEdgeCol);
    }
    private void EnemyDestroyed(int row, int column)
    {
        Invaders.DestroyAtRowColumn(row, column);
        RecalculateValues();
    }
    private void FireRandomProjectile()
    {
        var frontRowInvader = Invaders.GetRandomFromFrontRow();
        if(frontRowInvader == null)
        {
            return;
        }

        frontRowInvader.FireProjectile();
    }
}

internal class InvaderMob
{
    private readonly int[,] Matrix;
    private readonly Dictionary<int, GameObject> InvaderMap;
    private readonly List<int> FrontRows;

    public InvaderMob(int rows, int columns)
    {
        Matrix = new int[rows, columns];
        InvaderMap = new Dictionary<int, GameObject>();
        FrontRows = new List<int>();
    }

    public MobBounds Fill(GameObject[] prefabs,
        Vector3 invaderSize,
        Vector3 position,
        float gapX,
        float gapY,
        Func<GameObject, int, int, float, float, GameObject> createInvaderInstance)
    {
        var invaderWidth = invaderSize.x;
        var invaderHeight = invaderSize.y;
        var invaderIndex = 0;
        var currentX = position.x;
        var currentY = position.y;
        var minX = 0f;
        var maxX = 0f;

        for (var i = 0; i < Matrix.GetLength(0); i++)
        {
            for (var j = 0; j < Matrix.GetLength(1); j++)
            {
                invaderIndex++;
                var invaderGo = createInvaderInstance(GetPrefabForRow(i, prefabs), i, j, currentX, currentY);
                InvaderMap.Add(invaderIndex, invaderGo);
                // Calculate Min and Max X to determine Left and Right edges.
                if (currentX < minX || minX == 0f)
                {
                    minX = currentX;
                }
                if (currentX > maxX || maxX == 0f)
                {
                    maxX = currentX;
                }
                currentX += (invaderWidth + gapX);
                Matrix[i, j] = invaderIndex;

                // Add enemies to front rows if that's the case.
                if (i == 0)
                {
                    FrontRows.Add(invaderIndex);
                }
            }

            currentY += (invaderHeight + gapY);
            currentX = position.x;
        }

        var leftEdge = minX - (invaderWidth / 2) - gapX;
        var rightEdge = maxX + (invaderWidth / 2) + gapX;
        return new MobBounds(leftEdge, rightEdge, invaderSize.x, gapX);
    }

    public Invader GetFromRowColumn(int row, int column)
    {
        var index = Matrix[row, column];
        if(index == 0)
        {
            return null;
        }
        return InvaderMap[index].GetComponent<Invader>();
    }

    public Invader GetRandomFromFrontRow()
    {
        if (FrontRows.Count == 0)
        {
            return null;
        }
        var randomEnemyIndex = UnityEngine.Random.Range(0, FrontRows.Count);
        var enemy = InvaderMap[FrontRows[randomEnemyIndex]];
        return enemy.GetComponent<Invader>();
    }

    public (int leftEdgeCol, int rightEdgeCol) RecalculateValues()
    {
        FrontRows.Clear();
        var leftEdgeColumn = -1;
        var rightEdgeColumn = -1;
        for (var col = 0; col < Matrix.GetLength(1); col++)
        {
            var frontFound = false;
            for (var row = 0; row < Matrix.GetLength(0); row++)
            {
                if (Matrix[row, col] == 0)
                {
                    continue;
                }
                if (!frontFound)
                {
                    frontFound = true;
                    FrontRows.Add(Matrix[row, col]);
                }
                if (leftEdgeColumn == -1)
                {
                    leftEdgeColumn = col;
                }
                if (col > rightEdgeColumn)
                {
                    rightEdgeColumn = col;
                }
            }
        }
        return (leftEdgeColumn, rightEdgeColumn);
    }

    public void DestroyAtRowColumn(int row, int column)
    {
        var enemyIndex = Matrix[row, column];
        Matrix[row, column] = 0;
        InvaderMap.Remove(enemyIndex);
    }

    public IEnumerable<Invader> EnumerateLeftToRight()
    {
        for (var row = 0; row < Matrix.GetLength(0); row++)
        {
            for (var col = 0; col < Matrix.GetLength(1); col++)
            {
                var invader = GetFromRowColumn(row, col);
                if (invader == null)
                {
                    continue;
                }
                yield return invader;
            }
        }
    }

    public IEnumerable<Invader> EnumerateRightToLeft()
    {
        for (var row = Matrix.GetLength(0) - 1; row >= 0; row--)
        {
            for (var col = Matrix.GetLength(1) - 1; col >= 0; col--)
            {
                var invader = GetFromRowColumn(row, col);
                if (invader == null)
                {
                    continue;
                }
                yield return invader;
            }
        }
    }

    public IEnumerable<Invader[]> EnumerateRowsBottomToTop()
    {
        for (var row = 0; row < Matrix.GetLength(0); row++)
        {
            var rowInvaders = new List<Invader>();
            for (var col = 0; col < Matrix.GetLength(1); col++)
            {
                var invader = GetFromRowColumn(row, col);
                if (invader == null)
                {
                    continue;
                }
                rowInvaders.Add(invader);
            }
            if(rowInvaders.Count == 0)
            {
                continue;
            }
            yield return rowInvaders.ToArray();
        }
    }

    private GameObject GetPrefabForRow(int row, GameObject[] prefabs)
    {
        if (row > (prefabs.Length - 1) || row < 0)
        {
            return prefabs[0];
        }
        return prefabs[row];
    }
}

public struct MovementCycleController
{
    private float Cycle;
    private float CurrentDelta;
    private MobMovement CurrentDirection;
    public MovementCycleController(float cycle, MobMovement startDirection)
    {
        Cycle = cycle;
        CurrentDirection = startDirection;
        CurrentDelta = 0f;
    }

    public bool ShouldMove(out MobMovement movement)
    {
        movement = CurrentDirection;
        if (CurrentDelta >= Cycle)
        {
            CurrentDelta = 0f;
            return true;
        }
        else
        {
            CurrentDelta += Time.deltaTime;
        }
        return false;
    }

    public void SwitchDirection()
    {
        if (CurrentDirection == MobMovement.Left)
        {
            CurrentDirection = MobMovement.Right;
        }
        else
        {
            CurrentDirection = MobMovement.Left;
        }
    }
}

public enum MobMovement
{
    Left,
    Right
}

public struct MobFireController
{
    private float Cycle;
    private float FireProbability;
    private float CurrentDelta;
    public MobFireController(float cycle, float fireProbability)
    {
        Cycle = cycle;
        FireProbability = fireProbability;
        CurrentDelta = 0f;
    }

    public bool ShouldFire()
    {
        if (CurrentDelta >= Cycle)
        {
            CurrentDelta = 0f;
            if (UnityEngine.Random.Range(0f, 100) <= (FireProbability * 100))
            {
                return true;
            }
        }
        else
        {
            CurrentDelta += Time.deltaTime;
        }
        return false;
    }
}

public struct MobBounds
{
    private readonly int InitialMinColumnOffset;
    private readonly int InitialMaxColumnOffset;
    private int CurrentColumnOffset;
    private int MinColumnOffset;
    private int MaxColumnOffset;
    private float GapWidthRatio;
    public MobBounds(float initialLeft, float initialRight, float invaderWidth, float gapX)
    {
        // Need to calculate room to the left, and room to the right.
        var columnRoomToLeft = (int)((Edges.Values.Left - initialLeft) / invaderWidth);
        var columnRoomToRight = (int)((Edges.Values.Right - initialRight) / invaderWidth);
        InitialMinColumnOffset = MinColumnOffset = columnRoomToLeft;
        InitialMaxColumnOffset = MaxColumnOffset = columnRoomToRight;
        GapWidthRatio = invaderWidth / gapX;
        CurrentColumnOffset = 0;
    }

    public bool CanMove(MobMovement movement)
    {
        switch (movement)
        {
            case MobMovement.Left:
                if (CurrentColumnOffset > MinColumnOffset)
                {
                    return true;
                }
                break;

            case MobMovement.Right:
                if (CurrentColumnOffset < MaxColumnOffset)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    public void AdjustOffset(MobMovement movement)
    {
        switch (movement)
        {
            case MobMovement.Left:
                CurrentColumnOffset -= 1;
                break;

            case MobMovement.Right:
                CurrentColumnOffset += 1;
                break;
        }
    }

    public void RecalculateBounds(int leftColumn, int rightColumn)
    {
        var leftOffset = leftColumn;
        var rightOffset = (10 - rightColumn);
        MinColumnOffset = InitialMinColumnOffset - leftOffset - (int) (leftOffset / GapWidthRatio);
        MaxColumnOffset = InitialMaxColumnOffset + rightOffset + (int) (rightOffset / GapWidthRatio);
    }
}
