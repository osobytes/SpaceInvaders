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
    // A list of enemies that currently constitue the front row of each of it's columns.
    private int[,] EnemyMatrix = new int[5, 11];
    private Dictionary<int, GameObject> Enemies = new Dictionary<int, GameObject>();
    private Vector3 Invadersize;
    private List<int> FrontRows = new List<int>();
    private MobBounds Bounds;
    private MobFireController FireController;
    private MovementCycleController MovementCycleController;

    private void Start()
    {
        FireController = new MobFireController(FireCycle, FireProbability);
        MovementCycleController = new MovementCycleController(MovementCycle, MobMovement.Left);
        Invadersize = EnemyPrefabs[0].GetComponent<SpriteRenderer>().bounds.size;
        PopulateMatrix();
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
                for (var row = 0; row < 5; row++)
                {
                    for (var col = 0; col < 11; col++)
                    {
                        var enemyIndex = EnemyMatrix[row, col];
                        if (enemyIndex == 0)
                        {
                            continue;
                        }
                        var enemy = Enemies[enemyIndex];
                        enemy.GetComponent<Invader>().SetMoveX(Invadersize.x * -1, currentDelay, movementSpeed);
                        currentDelay += delayIncrement;
                    }
                }
                break;

            case MobMovement.Right:
                for (var row = 4; row >= 0; row--)
                {
                    for (var col = 10; col >= 0; col--)
                    {
                        var enemyIndex = EnemyMatrix[row, col];
                        if (enemyIndex == 0)
                        {
                            continue;
                        }
                        var enemy = Enemies[enemyIndex];
                        enemy.GetComponent<Invader>().SetMoveX(Invadersize.x, currentDelay, movementSpeed);
                        currentDelay += delayIncrement;
                    }
                }
                break;
        }
    }
    private void MoveMobDown()
    {
        var currentDelay = 0f;
        var delayIncrement = 0.1f;
        var movementSpeed = 3f;
        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 11; col++)
            {
                var enemyIndex = EnemyMatrix[row, col];
                if (enemyIndex == 0)
                {
                    continue;
                }
                var enemy = Enemies[enemyIndex];
                enemy.GetComponent<Invader>().SetMoveY(Invadersize.y * -1, currentDelay, movementSpeed);
            }
            currentDelay += delayIncrement;
        }
    }

    private void PopulateMatrix()
    {
        var invaderWidth = Invadersize.x;
        var invaderHeight = Invadersize.y;
        var enemyIndex = 0;
        var currentX = transform.position.x;
        var currentY = transform.position.y;
        var minX = 0f;
        var maxX = 0f;
        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 11; j++)
            {
                enemyIndex++;
                var enemy = Instantiate(GetEnemyForRow(i));
                var invader = enemy.GetComponent<Invader>();
                invader.OnDestroyed += EnemyDestroyed;
                invader.SetRowAndColumn(i, j);

                Enemies.Add(enemyIndex, enemy);
                enemy.transform.position = new Vector3(currentX, currentY);

                // Calculate Min and Max X to determine Left and Right edges.
                if (currentX < minX || minX == 0f)
                {
                    minX = currentX;
                }
                if (currentX > maxX || maxX == 0f)
                {
                    maxX = currentX;
                }
                currentX += (invaderWidth + GapX);
                EnemyMatrix[i, j] = enemyIndex;

                // Add enemies to front rows if that's the case.
                if (i == 0)
                {
                    FrontRows.Add(enemyIndex);
                }
            }

            currentY += (invaderHeight + GapY);
            currentX = transform.position.x;
        }

        var leftEdge = minX - (invaderWidth / 2) - GapX;
        var rightEdge = maxX + (invaderWidth / 2) + GapX;
        Bounds = new MobBounds(leftEdge, rightEdge, Invadersize.x, GapX);
    }

    private void RecalculateValues()
    {
        FrontRows.Clear();
        var leftEdgeColumn = -1;
        var rightEdgeColumn = -1;
        for (var col = 0; col < 11; col++)
        {
            var frontFound = false;
            for (var row = 0; row < 5; row++)
            {
                if (EnemyMatrix[row, col] == 0)
                {
                    continue;
                }
                if (!frontFound)
                {
                    frontFound = true;
                    FrontRows.Add(EnemyMatrix[row, col]);
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
        Bounds.RecalculateBounds(leftEdgeColumn, rightEdgeColumn);
    }

    private void EnemyDestroyed(int row, int column)
    {
        var enemyIndex = EnemyMatrix[row, column];
        EnemyMatrix[row, column] = 0;
        Enemies.Remove(enemyIndex);
        RecalculateValues();
    }

    private void FireRandomProjectile()
    {
        if (FrontRows.Count == 0)
        {
            return;
        }
        var randomEnemyIndex = Random.Range(0, FrontRows.Count);
        var enemy = Enemies[FrontRows[randomEnemyIndex]];
        enemy.GetComponent<Invader>().FireProjectile();
    }

    private GameObject GetEnemyForRow(int row)
    {
        if (row > (EnemyPrefabs.Length - 1) || row < 0)
        {
            return EnemyPrefabs[0];
        }
        return EnemyPrefabs[row];
    }
}

public struct MovementCycleController
{
    float Cycle;
    float CurrentDelta;
    MobMovement CurrentDirection;
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
    float Cycle;
    float FireProbability;
    float CurrentDelta;
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
            if (Random.Range(0f, 100) <= (FireProbability * 100))
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
    private int InitialMinColumnOffset;
    private int InitialMaxColumnOffset;
    private int CurrentColumnOffset;
    private int MinColumnOffset;
    private int MaxColumnOffset;
    private float GapWidthRatio;
    public MobBounds(float initialLeft, float initialRight, float invaderWidth, float gapX)
    {
        // Need to calculate room to the left, and room to the right.
        var columnRoomToLeft = (int)((Edges.Instance.LeftEdge - initialLeft) / invaderWidth);
        var columnRoomToRight = (int)((Edges.Instance.RightEdge - initialRight) / invaderWidth);
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
