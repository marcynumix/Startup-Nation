using UnityEngine;

public class MetricsUI : MonoBehaviour
{
    public static MetricsUI instance;

    public float errorRate=0.2f;
    public float offset=0.2f;

    public float animationSpeed=1;
        
    float xpValue=0;
    float moneyValue=0;
    float scienceValue=0;
    float fameValue=0;

    float[] targetValues = new float[4];
    public RectTransform[] sliders = new RectTransform[4];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    // Update is called once per frame
    void Update()
    {
        for(int i=0; i<sliders.Length; i++)
        {
            AnimateMetric(sliders[i], targetValues[i]);
        }
    }

    private void AnimateMetric(RectTransform rectTransform, float value)
    {
        // animate scale between 0 and value
        float scaleY = Mathf.Lerp(rectTransform.localScale.y, value, Time.deltaTime * animationSpeed);
        rectTransform.localScale = new Vector3(1, scaleY, 1);
    }

    public void SetMetrics(float successRate)
    {
        int[] order = GetRandomOrder();

        // S STARTUP
        Debug.Log("Success Rate: "+successRate);
        if(successRate>0.9){
            targetValues[order[0]]=1;
            targetValues[order[1]]=1;
            targetValues[order[2]]=0.75f;
            targetValues[order[3]]=0.75f;
        }
        // A STARTUP
        if(successRate>0.75){
            targetValues[order[0]]=1;
            targetValues[order[1]]=1;
            targetValues[order[2]]=0.5f;
            targetValues[order[3]]=0.5f;
        }
        // B STARTUP
        else if(successRate>0.5){
            targetValues[order[0]]=1;
            targetValues[order[1]]=0.5f;
            targetValues[order[2]]=0.5f;
            targetValues[order[3]]=0.25f;
        }
        // C STARTUP
        else if(successRate>0.25){
            targetValues[order[0]]=0.75f;
            targetValues[order[1]]=0.5f;
            targetValues[order[2]]=0.25f;
            targetValues[order[3]]=0.25f;
        }
        // D STARTUP
        else{
            targetValues[order[0]]=0.5f;
            targetValues[order[1]]=0.25f;
            targetValues[order[2]]=0.25f;
            targetValues[order[3]]=0.25f;
        }
        
    }

    // method to get a list of ids from 0 to 4 in a random order
    private int[] GetRandomOrder()
    {
        int[] order = new int[4];
        for(int i=0; i<4; i++)
        {
            order[i] = i;
        }
        for(int i=0; i<4; i++)
        {
            int temp = order[i];
            int randomIndex = Random.Range(i, 4);
            order[i] = order[randomIndex];
            order[randomIndex] = temp;
        }
        return order;
    }

}
