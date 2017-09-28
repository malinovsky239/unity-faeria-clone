using System;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Constants
    {
        public const float SmallEps = 0.001f;
        public const float LargeEps = 0.1f;

        public static readonly float Sin30 = Mathf.Sin(Mathf.PI / 6);
        public static readonly float Sin45 = Mathf.Sin(Mathf.PI / 4);
        public static readonly float Sin60 = Mathf.Sin(Mathf.PI / 3);

        public enum Neighbours
        {
            Up = 0,
            Down = 1,
            UpLeft = 2,
            DownRight = 3,
            DownLeft = 4,
            UpRight = 5
        }

        public static readonly int[] HexCoordShiftsX =
        {
            -1, 1, 0, 0, 1, -1
        };

        public static readonly int[] HexCoordShiftsY =
        {
            0, 0, -1, 1, -1, 1
        };

        public const float CellPadding = 0.05f;

        public const float SwordIconRotationSpeed = 180f;
        public const float SmileyDevilScaleCoeff = 0.8f;

        public const int StartMoveTerrainCellsCount = 2;
        public const int StartMoveEnergyBonus = 3;
        public const int CardsPerSideAtStart = 3;

        public const int ControlledMagicSourceEnergyBonus = 2;
        public const int MagicSourceControllersCount = 4;
        public const float MagicSourceScaleCoeff = 0.9f;

        public const int MessageMaxSizeScaleCoeff = 3;
        public const float MessageScaleMultiplier = 1.01f;

        public const float PointerSingleDotScale = 0.1f;
        public const int PointerDotsCount = 20;

        public class Card
        {
            public const float ScaleUpperBound = 1.5f;
            public const float InDeckScale = 1.0f;
            public const float InDeckShiftOnSelection = 1.0f;
            public const float InDeckShiftOnSelectionSpeed = 8.0f;
            public const float OnTheFieldScale = 0.55f;
            public const float OnTheFieldAdjustmentSpeed = 1.0f;
            public const float ReverseScaleSpeedCoeff = 0.5f;
            public const int SortingOrderShiftForTopmost = Int16.MaxValue / 2;
        }

        public class Bubble
        {
            public const int ShortestLifeDuration = 50;
            public const int LongestLifeDuration = 100;
            public const float MinSpeed = 0.2f;
            public const float MaxSpeed = 0.7f;
            public const float MinScale = 0.5f;
            public const float MaxScale = 2.0f;
            public const float InitMinShiftX = -0.8f;
            public const float InitMaxShiftX = 0.8f;
            public const float InitMinShiftY = 0f;
            public const float InitMaxShiftY = 0.5f;
            public const int MaxNumberPerMagicSource = 7;
            public const float DestructionRange = 0.1f;
            public const float SpeedTowardsOrb = 0.025f;
        }

        public class StringLiterals
        {
            public const string GameController = "GameController";
            public const string PlayerDeck = "PlayerDeck";
            public const string Pointer = "Pointer";
            public const string AddTerrain = "AddTerrain";
            public const string Board = "Board";
            public const string MessageController = "MessageController";
            public const string Graphics = "Graphics";
            public const string FlyingBubbleSortingLayer = "FlyingBubble";
            public const string Hourglass = "Hourglass";
        }

        public class Intervals
        {
            public const float AttackIconRotation = 1.0f;
            public const float CardDeath = 1.0f;
            public const float BeforeNPCTurn = 1.0f;
            public const float BeforeOrbDeath = 0.5f;
            public const float AfterTerrainExpansionByNPC = 0.5f;
        }

        public const int RightMouseButton = 1;
    }
}
