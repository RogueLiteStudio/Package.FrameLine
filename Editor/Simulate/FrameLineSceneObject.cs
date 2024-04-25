using System.Collections.Generic;
using UnityEngine;

namespace FrameLine
{
    [System.Serializable]
    public class FrameLineSceneObject
    {
        public string ClipGUID;
        public string AssetName;
        public GameObject SceneObject;
        public GameObject AttachObject;
        public string AttachNode;
        [SerializeField]
        private readonly List<ParticleSystem> particleSystems = new List<ParticleSystem>();
        [SerializeField]
        private readonly List<Animator> animators = new List<Animator>();
        [SerializeField]
        private readonly List<Animation> animations = new List<Animation>();
        [SerializeField]
        private Renderer[] renderers;
        protected MaterialPropertyBlock propertyBlock;
        public MaterialPropertyBlock PropertyBlock
        {
            get
            {
                propertyBlock ??= new MaterialPropertyBlock();
                return propertyBlock;
            }
        }
        public void SetObject(GameObject obj)
        {
            SceneObject = obj;
            if (SceneObject)
            {
                particleSystems.Clear();
                animators.Clear();
                animations.Clear();
                renderers = SceneObject.GetComponentsInChildren<Renderer>();
                SceneObject.GetComponentsInChildren(particleSystems);
                SceneObject.GetComponentsInChildren(animators);
                SceneObject.GetComponentsInChildren(animations);
                renderers = SceneObject.GetComponentsInChildren<Renderer>(true);
            }
        }

        public void SetActive(bool active)
        {
            if (SceneObject && SceneObject.activeSelf != active)
                SceneObject.SetActive(active);
        }
        public void Destroy()
        {
            if (SceneObject)
            {
                Object.DestroyImmediate(SceneObject);
                SceneObject = null;
            }
        }

        public void UpdateAttach(FrameLineSimulate simulate, GameObject root, string attachNode)
        {
            if (root != AttachObject || attachNode != AttachNode)
            {
                AttachNode = attachNode;
                AttachObject = root;
                if (!SceneObject)
                    return;
                if (AttachObject == null)
                {
                    SceneObject.transform.SetParent(simulate.SceneRoot, false);
                }
                else
                {
                    Transform parent = AttachObject.transform;
                    if (!string.IsNullOrEmpty(AttachNode))
                        parent = parent.RecursiveFindChild(AttachNode);
                    if (parent == null)
                        parent = AttachObject.transform;
                    SceneObject.transform.SetParent(parent, false);
                }
            }
        }

        public void Simulate(float time)
        {
            foreach (var p in particleSystems)
            {
                p.Simulate(time, true, true, false);
            }
            foreach (var a in animators)
            {
                AnimatorClipInfo[] clips = a.GetCurrentAnimatorClipInfo(0);
                for (int i = 0; i < clips.Length; i++)
                {
                    var clip = clips[i].clip;
                    if (clip)
                        clip.SampleAnimation(a.gameObject, time);
                }
            }
            foreach (var a in animations)
            {
                var clip = a.clip;
                if (clip)
                    clip.SampleAnimation(a.gameObject, time);
            }
            //防止动画中有设置材质的情况，在最后设置，可以发现动画中的材质设置
            if (propertyBlock != null && !propertyBlock.isEmpty)
            {
                foreach (var r in renderers)
                {
                    r.SetPropertyBlock(propertyBlock);
                }
                propertyBlock.Clear();
            }
        }
    }
}
