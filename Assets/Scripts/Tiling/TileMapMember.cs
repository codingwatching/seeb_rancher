using Assets.Scripts.Utilities.SaveObjects;
using Assets.Tiling;
using Assets.WorldObjects.SaveObjects;
using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Tiling
{
    /// <summary>
    /// Provides methods to navigate through a tileMap
    ///     stores information about the current location in the tileMap
    ///     
    /// </summary>
    public class TileMapMember : MonoBehaviour
    {
        [SerializeField]
        protected UniversalCoordinate coordinatePosition;
        public UniversalCoordinate CoordinatePosition => coordinatePosition;

        public void SetPosition(TileMapMember otherMember)
        {
            SetPosition(otherMember.coordinatePosition);
        }

        public void SetPosition(UniversalCoordinate position)
        {
            coordinatePosition = position;
            var newPosition = coordinatePosition.ToPositionInPlane();
            transform.position = new Vector3(newPosition.x, 0, newPosition.y);
        }
    }
}