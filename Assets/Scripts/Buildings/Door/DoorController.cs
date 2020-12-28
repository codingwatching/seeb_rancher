using Assets.Scripts.UI.Manipulators.Scripts;
using UnityEngine;

public class DoorController : MonoBehaviour, IManipulatorClickReciever
{
    public GameObject NextPhaseUI;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SelfHit(RaycastHit hit)
    {
        NextPhaseUI.SetActive(true);
    }
}
