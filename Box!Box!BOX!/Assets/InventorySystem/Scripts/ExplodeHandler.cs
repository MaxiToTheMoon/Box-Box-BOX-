using System.Collections.Generic;
using UnityEngine;

public class ExplodeHandler : MonoBehaviour
{
    [SerializeField] GameObject model; // root del modello (con le parti come figli)

    Rigidbody[] parts;
    Transform[] originalParents;
    Vector3[] originalPositions;
    Quaternion[] originalRotations;

    Renderer[] intactRenderers;

    void Awake()
    {
        // Parti rotte: Rigidbody figli del model, escluso il model stesso
        var partList = new List<Rigidbody>();
        foreach (Rigidbody rb in model.GetComponentsInChildren<Rigidbody>(true))
        {
            if (rb.transform != model.transform)
                partList.Add(rb);
        }
        parts = partList.ToArray();

        originalParents = new Transform[parts.Length];
        originalPositions = new Vector3[parts.Length];
        originalRotations = new Quaternion[parts.Length];

        var partSet = new HashSet<Rigidbody>(parts);

        for (int i = 0; i < parts.Length; i++)
        {
            Transform t = parts[i].transform;
            originalParents[i] = t.parent;
            originalPositions[i] = t.localPosition;
            originalRotations[i] = t.localRotation;
        }

        // Renderer del modello intatto: tutti quelli che NON stanno dentro una parte
        var intactList = new List<Renderer>();
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
        {
            Rigidbody owner = r.GetComponentInParent<Rigidbody>(true);
            if (owner == null || !partSet.Contains(owner))
                intactList.Add(r);
        }
        intactRenderers = intactList.ToArray();

        // Stato iniziale: auto intatta, parti nascoste
        HideParts();
        SetIntactVisible(true);
    }

    public void Explode(Vector3 externalForce, bool apply)
    {
        SetIntactVisible(false);

        foreach (Rigidbody rb in parts)
        {
            rb.transform.SetParent(null);
            rb.gameObject.SetActive(true);

            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            if (rb.TryGetComponent(out Collider col))
                col.enabled = true;

            rb.linearVelocity = externalForce * 0.0001f;

            //Vector3 dir = (rb.worldCenterOfMass - model.transform.position).normalized;
            //rb.AddForce(dir * 2f, ForceMode.Force);

            Vector3 dir = rb.worldCenterOfMass - model.transform.position;
            dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Random.onUnitSphere;
            dir = (dir + Vector3.up * 0.2f + Random.insideUnitSphere * 0.2f).normalized;

            float strength = (apply ? 2.5f : 0.1f) * Random.Range(0.2f, 0.7f);
            rb.AddForce(dir * strength, ForceMode.Force);

            

        }
    }

    public void ResetParts(Vector3 spawnPosition)
    {
        HideParts();
        SetIntactVisible(true);
    }

    void HideParts()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            Rigidbody rb = parts[i];

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;

            if (rb.TryGetComponent(out Collider col))
                col.enabled = false;

            Transform t = rb.transform;
            t.SetParent(originalParents[i], false);
            t.localPosition = originalPositions[i];
            t.localRotation = originalRotations[i];

            rb.gameObject.SetActive(rb.name.ToLower().Contains("wheel"));
        }
    }

    void SetIntactVisible(bool visible)
    {
        foreach (Renderer r in intactRenderers)
            r.enabled = visible;
    }
}