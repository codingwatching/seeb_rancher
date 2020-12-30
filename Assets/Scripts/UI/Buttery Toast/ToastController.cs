using TMPro;
using UnityEngine;

public class ToastController : MonoBehaviour
{
    public float lifetime;
    public float raiseMultiplier = 1;
    public AnimationCurve raise;
    public AnimationCurve fadeout;

    public TextMeshProUGUI textComponent;

    private float spawnTime;

    public GameObject destroyWhenDone;

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        var factor = (Time.time - spawnTime) / lifetime;
        if (factor > 1)
        {
            Destroy(destroyWhenDone);
            return;
        }
        var raiseAmt = raise.Evaluate(factor);
        var fadeAmt = 1 - fadeout.Evaluate(factor);

        textComponent.alpha = fadeAmt;

        transform.localPosition = Vector3.up * (raiseAmt * raiseMultiplier);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        var maxPoint = transform.parent.TransformPoint(Vector3.up * raiseMultiplier);
        Gizmos.DrawCube(maxPoint, (Vector3.right + Vector3.forward) * 2 + Vector3.up * .2f);
    }
}
