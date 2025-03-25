using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float restoreDefaultMaterialTime = 0.2f;

    private Material defaultMaterial;
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer.material;
    }

    public float GetRestoreDefaultMaterialTime() {
        return restoreDefaultMaterialTime;
    }


    public IEnumerator FlashRoutine() {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(restoreDefaultMaterialTime);
        spriteRenderer.material = defaultMaterial;
    }
}
