using UnityEngine;

// Make a scriptable Object InvestFeedbackSO
[CreateAssetMenu(fileName = "InvestFeedbackSO", menuName = "InvestFeedbackSO", order = 1)]
public class InvestFeedbackSO : ScriptableObject
{
    public AudioClip investSound;
    public string investSentence;
    public int positiveness=0;
}
