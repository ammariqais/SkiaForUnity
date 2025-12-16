using SkiaSharp.Unity.HB;
using UnityEngine;

public class hbtest : MonoBehaviour {
    [SerializeField]
    private string message;
    // Start is called before the first frame update
    void Start() {
        GetComponent<HB_TEXTBlock>().text = message;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A)) {
            GetComponent<HB_TEXTBlock>().text = "اهلا بكم في العالم";
        }
        
        if (Input.GetKeyUp(KeyCode.B)) {
            GetComponent<HB_TEXTBlock>().text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        }
        
    }
}
