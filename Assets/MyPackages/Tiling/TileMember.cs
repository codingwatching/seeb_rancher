using UnityEngine;

namespace Dman.Tiling
{
    /// <summary>
    /// Provides methods to navigate through a tileMap
    ///     stores information about the current location in the tileMap
    ///     
    /// </summary>
    public class TileMember : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        protected UniversalCoordinate coordinatePosition;
        public UniversalCoordinate CoordinatePosition => coordinatePosition;
        public AllRanges allRanges;

        public void SetPosition(TileMember otherMember)
        {
            SetPosition(otherMember.coordinatePosition);
        }

        public void SetPosition(UniversalCoordinate position)
        {
            var newPosition = allRanges.TransformCoordinate(position);
            transform.position = new Vector3(newPosition.x, 0, newPosition.y);
            coordinatePosition = position;
        }
    }
}