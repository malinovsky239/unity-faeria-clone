using UnityEngine;

namespace Assets.Scripts
{
    public class IconRotation : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(Vector3.back, Constants.SwordIconRotationSpeed * Time.deltaTime, Space.World);
        }
    }
}