using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionMonke
{
    public class MonkeAvatar : MonoBehaviour
    {
        private GameObject actionMonkeAvatar;
        private Transform actionMonkeCamera;
        
        private struct RendererPair { public Renderer orig; public Renderer clone; }
        private List<RendererPair> rendererPairs = new List<RendererPair>();



        private bool isCloning = false;

        public void UpdateMonke()
        {
            if (isCloning) return;
            isCloning = true;
            StartCoroutine(WaitAndClone(0.5f));
        }

        IEnumerator WaitAndClone(float delay)
        {
            yield return new WaitForSeconds(delay);
            DoClone();
            isCloning = false;
        }

        private void DoClone()
        {
            rendererPairs.Clear();
            if (actionMonkeAvatar != null) Destroy(actionMonkeAvatar);
            if (VRRig.LocalRig == null) return;

            actionMonkeAvatar = SafeClone(VRRig.LocalRig.gameObject);
            actionMonkeAvatar.name = "ActionMonke";
            
            GameObject camObj = new GameObject("ActionMonkeCamera");
            actionMonkeCamera = camObj.transform;
            actionMonkeCamera.SetParent(actionMonkeAvatar.transform, false);

            Transform camTransform = Camera.main.transform;
            if (camTransform != null)
            {
                foreach (Transform child in camTransform)
                {
                    if (child.GetComponentInChildren<MeshRenderer>(true) != null || 
                        child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null ||
                        child.GetComponentInChildren<SpriteRenderer>(true) != null)
                    {
                        if (child.GetComponent<Camera>() != null) continue;
                        
                        string childName = child.name.ToLower();
                        if (childName.Contains("fader") || childName.Contains("blackout") || childName.Contains("vignette") || childName.Contains("blinder") || childName.Contains("sphere") || childName.Contains("collider") || childName.Contains("trigger")) continue;

                        GameObject cosmeticClone = SafeClone(child.gameObject);
                        cosmeticClone.transform.SetParent(actionMonkeCamera, false);
                        cosmeticClone.transform.localPosition = child.localPosition;
                        cosmeticClone.transform.localRotation = child.localRotation;
                        cosmeticClone.transform.localScale = child.localScale;
                        
                        foreach (var origRen in child.GetComponentsInChildren<Renderer>(true))
                        {
                            Renderer[] cloneRens = cosmeticClone.GetComponentsInChildren<Renderer>(true);
                            foreach (var cloneRen in cloneRens)
                            {
                                if (cloneRen.name == origRen.name && cloneRen.GetType() == origRen.GetType())
                                {
                                    rendererPairs.Add(new RendererPair { orig = origRen, clone = cloneRen });
                                    break;
                                }
                            }
                        }
                    }
                }
            }


            actionMonkeAvatar.transform.localScale = Vector3.one * 0.35f;
            actionMonkeAvatar.transform.position = new Vector3(-68.8798f, 11.9632f, -81.5467f);
            actionMonkeAvatar.transform.rotation = Quaternion.Euler(0f, 123.19f, 0f);
            actionMonkeAvatar.SetActive(true);
        }

        private GameObject SafeClone(GameObject original)
        {
            GameObject clone = new GameObject(original.name + "_Clone");
            clone.layer = original.layer;
            clone.transform.localPosition = original.transform.localPosition;
            clone.transform.localRotation = original.transform.localRotation;
            clone.transform.localScale = original.transform.localScale;
            clone.SetActive(original.activeSelf);

            if (original.TryGetComponent<MeshFilter>(out var mf)) clone.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            if (original.TryGetComponent<MeshRenderer>(out var mr))
            {
                var newMr = clone.AddComponent<MeshRenderer>();
                newMr.sharedMaterials = mr.sharedMaterials;
                newMr.enabled = mr.enabled;
                
                string lowerName = original.name.ToLower();
                if (lowerName.Contains("fader") || lowerName.Contains("blackout") || lowerName.Contains("vignette") || lowerName == "head" || lowerName == "hand.l" || lowerName == "hand.r" || lowerName == "lefthand" || lowerName == "righthand" || lowerName == "eye.l" || lowerName == "eye.r" || lowerName == "lefteye" || lowerName == "righteye")
                {
                    newMr.enabled = false;
                }
                
                if (mf != null && mf.sharedMesh != null)
                {
                    string meshName = mf.sharedMesh.name.ToLower();
                    if (meshName.Contains("sphere") || meshName.Contains("cube") || meshName.Contains("capsule") || meshName.Contains("cylinder") || meshName.Contains("quad") || meshName.Contains("plane"))
                    {
                        newMr.enabled = false;
                    }
                }
                
                rendererPairs.Add(new RendererPair { orig = mr, clone = newMr });
            }
            if (original.TryGetComponent<SkinnedMeshRenderer>(out var smr))
            {
                var newSmr = clone.AddComponent<SkinnedMeshRenderer>();
                newSmr.sharedMesh = smr.sharedMesh;
                newSmr.sharedMaterials = smr.sharedMaterials;
                
                if (VRRig.LocalRig != null && smr == VRRig.LocalRig.mainSkin)
                {
                    newSmr.enabled = true;
                    clone.layer = 0;
                }
                else
                {
                    newSmr.enabled = smr.enabled;
                }
                
                rendererPairs.Add(new RendererPair { orig = smr, clone = newSmr });
            }
            
            if (original.TryGetComponent<SpriteRenderer>(out var sr))
            {
                var newSr = clone.AddComponent<SpriteRenderer>();
                newSr.sprite = sr.sprite;
                newSr.color = sr.color;
                newSr.sharedMaterial = sr.sharedMaterial;
                newSr.enabled = sr.enabled;
                rendererPairs.Add(new RendererPair { orig = sr, clone = newSr });
            }
            if (original.TryGetComponent<LineRenderer>(out var lr))
            {
                var newLr = clone.AddComponent<LineRenderer>();
                newLr.positionCount = lr.positionCount;
                Vector3[] positions = new Vector3[lr.positionCount];
                lr.GetPositions(positions);
                newLr.SetPositions(positions);
                newLr.startWidth = lr.startWidth;
                newLr.endWidth = lr.endWidth;
                newLr.startColor = lr.startColor;
                newLr.endColor = lr.endColor;
                newLr.sharedMaterials = lr.sharedMaterials;
                newLr.enabled = lr.enabled;
                rendererPairs.Add(new RendererPair { orig = lr, clone = newLr });
            }
            foreach (Transform child in original.transform)
            {
                GameObject childClone = SafeClone(child.gameObject);
                childClone.transform.SetParent(clone.transform, false);
            }

            if (original == VRRig.LocalRig.gameObject)
            {
                var origSmrs = original.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var cloneSmrs = clone.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                var origTfs = original.GetComponentsInChildren<Transform>(true);
                var cloneTfs = clone.GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < origSmrs.Length; i++)
                {
                    Transform[] newBones = new Transform[origSmrs[i].bones.Length];
                    for (int b = 0; b < origSmrs[i].bones.Length; b++)
                    {
                        int idx = System.Array.IndexOf(origTfs, origSmrs[i].bones[b]);
                        newBones[b] = idx >= 0 ? cloneTfs[idx] : origSmrs[i].bones[b];
                    }
                    
                    cloneSmrs[i].bones = newBones;
                    if (origSmrs[i].rootBone != null)
                    {
                        int rootIdx = System.Array.IndexOf(origTfs, origSmrs[i].rootBone);
                        cloneSmrs[i].rootBone = rootIdx >= 0 ? cloneTfs[rootIdx] : origSmrs[i].rootBone;
                    }
                }
            }
            return clone;
        }

        private Transform FindHeadBone(Transform parent)
        {
            if (parent.name.ToLower() == "head")
            {
                if (parent.parent != null)
                {
                    string pName = parent.parent.name.ToLower();
                    if (pName.Contains("chest") || pName.Contains("spine") || pName.Contains("neck") || pName.Contains("rig"))
                    {
                        return parent;
                    }
                }
            }
            foreach (Transform child in parent)
            {
                Transform found = FindHeadBone(child);
                if (found != null) return found;
            }
            return null;
        }

        private Transform FindBoneContaining(Transform parent, string name)
        {
            if (parent.name.ToLower().Contains(name.ToLower())) return parent;
            foreach (Transform child in parent)
            {
                Transform found = FindBoneContaining(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private void MapBones(Transform dummy, Transform orig)
        {
            if (dummy == null || orig == null) return;
            
            for (int i = 0; i < orig.childCount; i++)
            {
                Transform origChild = orig.GetChild(i);
                Transform dummyChild = dummy.Find(origChild.name + "_Clone");
                if (dummyChild != null)
                {
                    dummyChild.localPosition = origChild.localPosition;
                    dummyChild.localRotation = origChild.localRotation;
                    MapBones(dummyChild, origChild);
                }
            }
        }

        void LateUpdate()
        {
            if (actionMonkeAvatar == null || VRRig.LocalRig == null) return;
            
            MapBones(actionMonkeAvatar.transform, VRRig.LocalRig.transform);

            if (actionMonkeCamera != null && Camera.main != null)
            {
                actionMonkeCamera.localPosition = VRRig.LocalRig.transform.InverseTransformPoint(Camera.main.transform.position);
                actionMonkeCamera.localRotation = Quaternion.Inverse(VRRig.LocalRig.transform.rotation) * Camera.main.transform.rotation;
            }

            MaterialPropertyBlock syncBlock = new MaterialPropertyBlock();
            for (int i = 0; i < rendererPairs.Count; i++)
            {
                if (rendererPairs[i].orig != null && rendererPairs[i].clone != null)
                {
                    rendererPairs[i].orig.GetPropertyBlock(syncBlock);
                    rendererPairs[i].clone.SetPropertyBlock(syncBlock);
                }
            }
        }
    }
}
