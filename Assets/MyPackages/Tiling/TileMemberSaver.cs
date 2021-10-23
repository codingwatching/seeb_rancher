using Dman.SceneSaveSystem;
using UnityEngine;

namespace Dman.Tiling
{
    [RequireComponent(typeof(TileMember))]
    public class TileMemberSaver : MonoBehaviour, ISaveableData
    {
        public string UniqueSaveIdentifier => "GreenhouseMember";

        public object GetSaveObject()
        {
            var member = GetComponent<TileMember>();
            return member.CoordinatePosition;
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is UniversalCoordinate coordinate)
            {
                var member = GetComponent<TileMember>();
                member.SetPosition(coordinate);
            }
        }
    }
}