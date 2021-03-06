﻿using GameLibrary.Dependencies.Entities;

namespace GameLibrary.Entities.Components
{
    public interface IDamping : Component
    {
        /// <summary>
        /// The meter/second quantity by which the linear velocity will slow.
        /// </summary>
        float LinearDamping { set; get; }

        /// <summary>
        /// The radian/second quantity by which the angular velocity (omega) will slow.
        /// </summary>
        float AngularDamping { set; get; }
    }
}