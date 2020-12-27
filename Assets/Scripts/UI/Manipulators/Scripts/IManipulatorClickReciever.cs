using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI.Manipulators.Scripts
{
    public interface IManipulatorClickReciever
    {
        public void SelfHit(RaycastHit hit);
    }
}
