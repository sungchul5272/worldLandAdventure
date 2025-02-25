using UnityEngine;

[ExecuteInEditMode] // 에디터에서 실시간 반영
public class TileColorControl : MonoBehaviour
{
    Renderer _rend;
    MaterialPropertyBlock _propBlock;

    [SerializeField] Color _color;

    void Start()
    {
        ApplyColor();
    }

    void OnValidate()
    {
        ApplyColor();
    }

    void ApplyColor()
    {
        if (_rend == null)
        {
            _rend = GetComponent<Renderer>();
            if (_rend == null)
            {
                return;
            }
        }

        if (_propBlock == null)
        {
            _propBlock = new MaterialPropertyBlock();
        }

        _rend.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_TileColor", _color);
        _rend.SetPropertyBlock(_propBlock);
    }
}
