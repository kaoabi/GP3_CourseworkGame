using System;
using System.Collections.Generic;
using System.Text;

namespace Coursework_Game
{
    public enum e_UFOStates
    {
        e_grabbing,
        e_freeRoam,
        e_moveBackUp,
        e_moveBackAcross,
        e_drop,
    };

    static class GameVariables
    {
        //General game consts
        public const int numberOfPrizes = 20;
        public const float ufoMoveSpeed = 10f;
        public const float camRotationSpeed = 1f / 60f;
        public const float grabSpeed = 5f;
        public const float beamForce = 20;
    }
}
