public class TeamInputData
{
    public float Sum = 0f;
    public int Count = 0;

    public void AddInput(float input)
    {
        Sum += input;
        Count++;
    }

    public float GetAverage()
    {
        if (Count == 0) return 0f;
        return Sum / Count;
    }

    public void Reset()
    {
        Sum = 0f;
        Count = 0;
    }
}
