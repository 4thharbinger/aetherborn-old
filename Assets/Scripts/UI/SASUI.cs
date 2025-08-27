using UnityEngine;
using UnityEngine.UI;

public class SASUI : MonoBehaviour
{
    public Button off;
    public Button stabilize;
    public Button prograde;
    public Button retrogade;

    public ControlSource controlSource;
    private void Awake()
    {
        off.onClick.AddListener(() => { controlSource.sasMode = SASMode.Off; });
        stabilize.onClick.AddListener(() => { controlSource.sasMode = SASMode.Stabilize; });
        prograde.onClick.AddListener(() => { controlSource.sasMode = SASMode.Prograde; });
        retrogade.onClick.AddListener(() => { controlSource.sasMode = SASMode.Retrograde; });
    }
}
