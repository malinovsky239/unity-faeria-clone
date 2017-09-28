using UnityEngine;

namespace Assets.Scripts
{
    public class CameraSizeAdjustment : MonoBehaviour
    {
        private const float WidthInUnits = 21.6f;
        private const float HeightInUnits = 10;
        private const float TargetAspectRatio = WidthInUnits / HeightInUnits;

        private void Awake()
        {
            Resize();
        }

        private void Resize()
        {
            var aspectRatio = (float)Screen.width / Screen.height;
            if (aspectRatio < TargetAspectRatio)
            {
                var correctCameraHeight = TargetAspectRatio / aspectRatio * HeightInUnits;
                GetComponent<Camera>().orthographicSize = correctCameraHeight / 2f;
            }
        }
    }
}