using UnityEngine;

public static class ListUtility
{
    public static T GetRandom<T>(this System.Collections.Generic.List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new System.InvalidOperationException("Cannot get a random element from an empty or null list.");
        }
        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }
}
