using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexAnimationRenderer
{
    private MeshVertexAnimations   meshAnimations;
    private Material               material;
    private int                    defaultAnimation;

    private ComputeBuffer[] animationBuffers;
    MaterialPropertyBlock   matProperties;

    public VertexAnimationRenderer(MeshVertexAnimations animations, Material material, int defaultAnimation)
    {
        this.meshAnimations     = animations;
        this.material           = material;
        this.defaultAnimation   = defaultAnimation;
    }

    ~VertexAnimationRenderer()
    {
        Disable();
    }

    public void Enable()
    {
        Disable();

        matProperties = new MaterialPropertyBlock();

        animationBuffers = new ComputeBuffer[meshAnimations.animations.Length];

        for(int i = 0; i < meshAnimations.animations.Length; ++i)
        {
            int animationLength = meshAnimations.animations[i].vertices.Length;
            if(animationLength <= 0)
            {
                Debug.LogWarning($"Cannot create compute buffer of length 0, [{i}] animation={meshAnimations.animations[i].name}");
                continue;
            }
            animationBuffers[i] = new ComputeBuffer(animationLength, sizeof(float) * 3, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
            var nativeBuffer = animationBuffers[i].BeginWrite<Vector3>(0, animationLength);
            nativeBuffer.CopyFrom(meshAnimations.animations[i].vertices);
            animationBuffers[i].EndWrite<Vector3>(animationLength);
        }

        SetAnimation(defaultAnimation);
    }

    public void Disable()
    {
        if (animationBuffers != null)
        {
            for (int i = 0; i < animationBuffers.Length; ++i)
            {
                if (animationBuffers[i] != null)
                    animationBuffers[i].Dispose();
            }
        }
    }

    public void SetAnimation(int index)
    {
        if(index < meshAnimations.animations.Length)
        {
            material.SetBuffer("_AnimBuffer",   animationBuffers[index]);
            material.SetInt("_VertexCount",     meshAnimations.VertexCount);
            material.SetInt("_Frames",          meshAnimations.animations[index].frames);
        }
        else
        {
            Debug.LogError($"Invalid animation index: {index}");
        }
    }

    public void Draw(int count, ComputeBuffer buffer, Bounds bounds)
    {
        matProperties.SetBuffer("_InstanceBuffer", buffer);
        Graphics.DrawMeshInstancedProcedural(meshAnimations.mesh, 0, material, bounds, count, matProperties);
    }
}
