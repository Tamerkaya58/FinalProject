using UnityEngine;

/// <summary>
/// A general utility class for calculating boundaries of GameObjects.
/// This can be used both in the Editor and during Gameplay.
/// </summary>
public static class BoundsUtility
{
    /// <summary>
    /// Calculates the encapsulated bounds of all renderers in the object and its children.
    /// </summary>
    /// <param name="obj">The GameObject to calculate bounds for.</param>
    /// <returns>The total Bounds of the object.</returns>
    public static Bounds GetTotalBounds(GameObject obj)
    {
        if (obj == null) return new Bounds(Vector3.zero, Vector3.zero);

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            // If no renderers are found, return a zero-sized bounds at the object's position
            return new Bounds(obj.transform.position, Vector3.zero);
        }

        // Initialize bounds with the first renderer
        Bounds bounds = renderers[0].bounds;

        // Encapsulate all other renderers found in the hierarchy
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Extension method to easily get bounds from any GameObject.
    /// Usage: myGameObject.GetBounds();
    /// </summary>
    public static Bounds GetBounds(this GameObject obj)
    {
        return GetTotalBounds(obj);
    }
}
