using UnityEngine;

namespace Optifx.PixelArt
{
    [RequireComponent(typeof(Collider))]
    public class UnderWaterFogTrigger : MonoBehaviour
    {
        [Header("Réglages du Layer")]
        [Tooltip("Nom du layer correspondant à l'eau (ex. 'Water').")]
        [SerializeField] private string waterLayerName = "Water";

        [Header("Réglages du Fog Sous l’eau")]
        [SerializeField, Tooltip("Densité du fog sous l’eau.")] 
        private float underwaterFogDensity = 0.05f;

        [SerializeField, Tooltip("Couleur du fog sous l’eau.")] 
        private Color underwaterFogColor = Color.cyan;

        private float _initialFogDensity;
        private Color _initialFogColor;
        private int _waterLayer;
        private bool _isUnderwater;

        private void Start()
        {
            // Sauvegarde les valeurs initiales du fog
            _initialFogDensity = RenderSettings.fogDensity;
            _initialFogColor = RenderSettings.fogColor;

            // Récupère l’index du layer à détecter
            _waterLayer = LayerMask.NameToLayer(waterLayerName);
            if (_waterLayer == -1)
            {
                Debug.LogError($"Le layer '{waterLayerName}' n'existe pas dans le projet !");
            }

            // S'assure que le collider est bien configuré
            Collider col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                Debug.LogWarning($"Le collider de '{gameObject.name}' n'est pas configuré comme trigger. Activez 'Is Trigger' pour un comportement correct.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == _waterLayer)
                SetUnderwater(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == _waterLayer)
                SetUnderwater(false);
        }

        private void SetUnderwater(bool state)
        {
            if (_isUnderwater == state) return; // évite de réappliquer inutilement les mêmes valeurs
            _isUnderwater = state;

            RenderSettings.fogDensity = state ? underwaterFogDensity : _initialFogDensity;
            RenderSettings.fogColor = state ? underwaterFogColor : _initialFogColor;
        }
    }
}
