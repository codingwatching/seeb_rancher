using Assets.Scripts.Utilities.SaveSystem.Components;
using Assets.Tiling;
using UnityEngine;

namespace Assets.Scripts.Tiling
{
    [RequireComponent(typeof(GreenhouseMember))]
    public class GreenhouseMemberSaver : MonoBehaviour, ISaveableData
    {
        public string UniqueSaveIdentifier => "GreenhouseMember";

        public ISaveableData[] GetDependencies()
        {
            return new ISaveableData[0];
        }

        public object GetSaveObject()
        {
            var member = GetComponent<GreenhouseMember>();
            return member.CoordinatePosition;
        }

        public void SetupFromSaveObject(object save)
        {
            if (save is UniversalCoordinate coordinate)
            {
                var member = GetComponent<GreenhouseMember>();
                member.SetPosition(coordinate);
            }
        }
    }
}