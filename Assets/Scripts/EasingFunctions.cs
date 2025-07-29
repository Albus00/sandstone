using UnityEngine;

public static class EasingFunctions
{
  public static float EaseInOutCirc(float t)
  {
    return t < 0.5f
      ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f
      : (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
  }
}